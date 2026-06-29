using Daedalus;
using Daedalus.Config;

namespace Daedalus.Services.Content;

/// <summary>
/// Maintains a rotation-facing configuration snapshot with optional duty-profile overlays.
/// Does not mutate the persisted user configuration.
/// </summary>
public sealed class DutyConfigurationService : IDutyConfigurationService
{
    private readonly Configuration _savedConfiguration;
    private readonly IDutyContentService _dutyContentService;

    public DutyConfigurationService(Configuration savedConfiguration, IDutyContentService dutyContentService)
    {
        _savedConfiguration = savedConfiguration;
        _dutyContentService = dutyContentService;
        RotationConfiguration = ConfigurationCopier.CreateRotationCopy(savedConfiguration);
        Refresh();
    }

    public Configuration RotationConfiguration { get; }

    public void Refresh()
    {
        ConfigurationCopier.CopyOnto(RotationConfiguration, _savedConfiguration);

        if (_savedConfiguration.EnableAutoDutyConfig)
        {
            var profile = _dutyContentService.EffectiveProfile;
            if (profile != EffectiveDutyProfile.None)
                ConfigurationPresets.ApplyDutyProfile(RotationConfiguration, profile);
        }

        // Per-fight strategy override (MVP: targeting). Applied last so it wins over the duty profile
        // for the specific fight, and only while in that duty. Never mutates the saved config.
        var raidStrategy = _savedConfiguration.Raid.GetActiveTargeting(_dutyContentService.CurrentTerritoryType);
        raidStrategy?.ApplyOnto(RotationConfiguration.Targeting);
    }
}
