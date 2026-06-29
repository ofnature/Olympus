namespace Daedalus.Services.Targeting;

/// <summary>
/// Pure policy helpers for combat death retargeting. Extracted so unit tests
/// can validate the three-layer rules without Dalamud runtime mocks.
/// </summary>
internal static class CombatRetargetPolicy
{
    public static bool IsAggregateStrategy(EnemyTargetingStrategy strategy) =>
        strategy is EnemyTargetingStrategy.LowestHp
            or EnemyTargetingStrategy.HighestHp
            or EnemyTargetingStrategy.Nearest
            or EnemyTargetingStrategy.TankAssist;

    /// <summary>
    /// Layer 2: do not pause damage targeting when the player is in combat,
    /// the hard target is invalid, and live hostiles are nearby.
    /// </summary>
    public static bool ShouldUnpauseForCombatRetarget(
        bool pauseWhenNoTarget,
        bool hasValidUserSelectedEnemy,
        bool hasLiveStickyTarget,
        bool playerInCombat,
        bool hardTargetInvalid,
        bool hasLiveHostilesNearby)
    {
        if (!pauseWhenNoTarget)
            return true;

        if (hasValidUserSelectedEnemy || hasLiveStickyTarget)
            return true;

        return playerInCombat && hardTargetInvalid && hasLiveHostilesNearby;
    }

    /// <summary>
    /// Layer 3: relax StrictCurrentTargetStrategy fallback when target died mid-combat
    /// and the configured strategy is aggregate.
    /// </summary>
    public static bool ShouldRelaxStrictOnCombatDeath(
        bool strictCurrentTargetStrategy,
        EnemyTargetingStrategy enemyStrategy,
        bool isCombatRetargetScenario) =>
        isCombatRetargetScenario
        && strictCurrentTargetStrategy
        && IsAggregateStrategy(enemyStrategy);

    /// <summary>
    /// Strategy used to pick the game-target write on combat death retarget.
    /// Explicit strategies (CurrentTarget/FocusTarget) fall back to LowestHp for the pick.
    /// </summary>
    public static EnemyTargetingStrategy ResolveAutoRetargetStrategy(EnemyTargetingStrategy configured) =>
        IsAggregateStrategy(configured) ? configured : EnemyTargetingStrategy.LowestHp;

    /// <summary>
    /// Unreachable-target retarget (split-boss recovery): switch off a followed target that is a
    /// valid living enemy but out of effective action range, when a reachable attackable hostile
    /// exists. Gated on a grace period so a brief out-of-range blip (knockback, repositioning) and
    /// normal gap-closing to a single far target never trigger it.
    /// </summary>
    /// <param name="featureEnabled"><see cref="TargetingConfig.RetargetUnreachableTarget"/>.</param>
    /// <param name="playerInCombat">Player is effectively in combat.</param>
    /// <param name="heldTargetIsLivingEnemy">The followed target resolves to a live, attackable enemy (not gone/dead/immune).</param>
    /// <param name="heldTargetInRange">The followed target is within effective action range.</param>
    /// <param name="gracePassed">The target has been continuously out of range past the grace window.</param>
    /// <param name="hasReachableAlternative">Another attackable hostile is in range.</param>
    public static bool ShouldRetargetUnreachable(
        bool featureEnabled,
        bool playerInCombat,
        bool heldTargetIsLivingEnemy,
        bool heldTargetInRange,
        bool gracePassed,
        bool hasReachableAlternative) =>
        featureEnabled
        && playerInCombat
        && heldTargetIsLivingEnemy
        && !heldTargetInRange
        && gracePassed
        && hasReachableAlternative;
}
