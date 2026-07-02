using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Daedalus.Config;
using Daedalus.Services.Targeting;

namespace Daedalus.Rotation.Common.Helpers;

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

    /// <summary>
    /// Pure-math melee reach check: 3y weapon reach + both hitbox radii + a small slack (the same
    /// model the nav code uses). Used to demote a sticky/hard target that's far outside melee to
    /// the out-of-melee branch (gap close + ranged GCD) instead of pushing combo GCDs that fail
    /// range at dispatch forever ("Syphon Strike: out of range 16y &gt; 3y" W2W transit stuck state).
    /// Deliberately NOT the native ActionManager range check — this must be evaluable in tests and
    /// cheap per frame.
    /// </summary>
    public static bool IsWithinMeleeReach(IGameObject player, IGameObject target)
    {
        var reach = 3f + player.HitboxRadius + target.HitboxRadius + 0.5f;
        return Vector3.DistanceSquared(player.Position, target.Position) <= reach * reach;
    }
}
