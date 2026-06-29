using Daedalus.Services.Targeting;

namespace Daedalus.Config;

/// <summary>
/// A per-fight targeting strategy override. When <see cref="Enabled"/>, these values replace the
/// global <see cref="TargetingConfig"/> targeting fields while the player is in the keyed duty.
/// Applied non-destructively onto the rotation's effective configuration (see
/// <c>DutyConfigurationService</c>) — the saved global config is never mutated.
/// MVP scope: targeting only.
/// </summary>
public sealed class RaidTargetingStrategy
{
    /// <summary>When false, the global targeting settings are used for this fight (override inert).</summary>
    public bool Enabled { get; set; }

    /// <summary>Duty display name captured when the override was created (for the saved list / no relookup).</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Per-fight enemy selection strategy.</summary>
    public EnemyTargetingStrategy EnemyStrategy { get; set; } = EnemyTargetingStrategy.LowestHp;

    /// <summary>Per-fight "switch off unreachable targets" (split-boss recovery).</summary>
    public bool RetargetUnreachableTarget { get; set; } = true;

    /// <summary>Per-fight strict explicit-target mode.</summary>
    public bool StrictCurrentTargetStrategy { get; set; } = true;

    /// <summary>Per-fight skip-invulnerable-enemies filtering.</summary>
    public bool EnableInvulnerabilityFiltering { get; set; } = true;

    /// <summary>
    /// Seeds a new override from the current global targeting settings, so enabling a per-fight
    /// strategy starts from "same as global" rather than arbitrary defaults.
    /// </summary>
    public static RaidTargetingStrategy FromGlobal(TargetingConfig global) => new()
    {
        Enabled = true,
        EnemyStrategy = global.EnemyStrategy,
        RetargetUnreachableTarget = global.RetargetUnreachableTarget,
        StrictCurrentTargetStrategy = global.StrictCurrentTargetStrategy,
        EnableInvulnerabilityFiltering = global.EnableInvulnerabilityFiltering,
    };

    /// <summary>Copies the override fields onto the given (effective) targeting config.</summary>
    public void ApplyOnto(TargetingConfig target)
    {
        target.EnemyStrategy = EnemyStrategy;
        target.RetargetUnreachableTarget = RetargetUnreachableTarget;
        target.StrictCurrentTargetStrategy = StrictCurrentTargetStrategy;
        target.EnableInvulnerabilityFiltering = EnableInvulnerabilityFiltering;
    }
}
