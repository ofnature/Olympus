using System.Collections.Generic;

namespace Daedalus.Config;

/// <summary>
/// Per-fight strategy overrides, keyed by duty territory type. Edited in the Raid window and
/// applied non-destructively onto the rotation's effective config when the player is in a keyed duty.
/// MVP scope: targeting only.
/// </summary>
public sealed class RaidConfig
{
    /// <summary>Per-duty targeting overrides, keyed by <c>TerritoryType</c> row id.</summary>
    public Dictionary<uint, RaidTargetingStrategy> TargetingByTerritory { get; set; } = new();

    /// <summary>Returns the override for a territory, or null when none is saved.</summary>
    public RaidTargetingStrategy? GetTargeting(uint territoryType) =>
        TargetingByTerritory.TryGetValue(territoryType, out var strategy) ? strategy : null;

    /// <summary>Returns the active (enabled) override for a territory, or null.</summary>
    public RaidTargetingStrategy? GetActiveTargeting(uint territoryType)
    {
        var strategy = GetTargeting(territoryType);
        return strategy is { Enabled: true } ? strategy : null;
    }
}
