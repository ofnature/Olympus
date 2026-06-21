namespace Olympus.Services.Targeting;

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
}
