using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace Daedalus.Services.Content;

/// <summary>
/// Classifies the current zone as dungeon, trial, raid, or open world using Lumina sheets.
/// </summary>
public sealed class DutyContentService : IDutyContentService
{
    private readonly IDataManager _dataManager;

    public DutyContentService(IDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    public DutyContentType CurrentDuty { get; private set; } = DutyContentType.Unknown;
    public EffectiveDutyProfile EffectiveProfile { get; private set; } = EffectiveDutyProfile.None;
    public uint CurrentTerritoryType { get; private set; }
    public string CurrentDutyName { get; private set; } = string.Empty;

    public string DutyLabel => EffectiveProfile switch
    {
        EffectiveDutyProfile.Dungeon => "Dungeon",
        EffectiveDutyProfile.Trial => "Trial",
        EffectiveDutyProfile.Raid => "Raid",
        EffectiveDutyProfile.HighEndRaid => "High-End",
        _ => CurrentDuty switch
        {
            DutyContentType.OpenWorld => "Open World",
            DutyContentType.Unknown => "Unknown",
            _ => CurrentDuty.ToString(),
        },
    };

    public void OnTerritoryChanged(ushort territoryType, bool isHighEndZone, int partyMemberCount)
    {
        CurrentTerritoryType = territoryType;
        CurrentDuty = ResolveDutyType(territoryType);
        CurrentDutyName = ResolveDutyName(territoryType);
        var trustOrEmptyParty = partyMemberCount == 0;
        EffectiveProfile = DutyContentClassifier.Resolve(
            CurrentDuty,
            isHighEndZone,
            partyMemberCount,
            trustOrEmptyParty);
    }

    private string ResolveDutyName(ushort territoryType)
    {
        if (territoryType == 0)
            return string.Empty;

        var row = _dataManager.GetExcelSheet<TerritoryType>()?.GetRowOrDefault(territoryType);
        var cfc = row?.ContentFinderCondition.ValueNullable;
        if (cfc is null)
            return string.Empty;

        return cfc.Value.Name.ExtractText();
    }

    private DutyContentType ResolveDutyType(ushort territoryType)
    {
        if (territoryType == 0)
            return DutyContentType.OpenWorld;

        var row = _dataManager.GetExcelSheet<TerritoryType>()?.GetRowOrDefault(territoryType);
        if (!row.HasValue)
            return DutyContentType.Unknown;

        if (row.Value.IsPvpZone)
            return DutyContentType.OpenWorld;

        var cfc = row.Value.ContentFinderCondition.ValueNullable;
        if (cfc is null)
            return DutyContentType.OpenWorld;

        return DutyContentClassifier.FromContentTypeRowId(cfc.Value.ContentType.Value.RowId);
    }
}
