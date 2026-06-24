using Olympus.Config;
using Olympus.Services.Targeting;

namespace Olympus.Rotation.Common.Helpers;

/// <summary>
/// Shared targeting helpers for tank rotations.
/// </summary>
public static class TankTargetingHelper
{
    /// <summary>
    /// Resolves the effective enemy-targeting strategy for a tank.
    /// When <see cref="TankConfig.IgnoreAddsWithCoTank"/> is enabled and a co-tank is present, this
    /// forces <see cref="EnemyTargetingStrategy.CurrentTarget"/> so the tank sticks to the player's
    /// selected enemy instead of auto-acquiring loose adds. Otherwise the configured strategy is
    /// returned unchanged. Tank-swap/Provoke logic is unaffected (it does not flow through here).
    /// </summary>
    /// <param name="tank">Tank configuration.</param>
    /// <param name="configured">The configured enemy-targeting strategy.</param>
    /// <param name="hasCoTank">Whether another tank is present in the party.</param>
    public static EnemyTargetingStrategy ResolveEnemyStrategy(
        TankConfig tank,
        EnemyTargetingStrategy configured,
        bool hasCoTank)
        => tank.IgnoreAddsWithCoTank && hasCoTank
            ? EnemyTargetingStrategy.CurrentTarget
            : configured;
}
