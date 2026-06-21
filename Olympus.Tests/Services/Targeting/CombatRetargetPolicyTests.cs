using Olympus.Services.Targeting;
using Xunit;

namespace Olympus.Tests.Services.Targeting;

public sealed class CombatRetargetPolicyTests
{
    [Theory]
    [InlineData(EnemyTargetingStrategy.LowestHp, true)]
    [InlineData(EnemyTargetingStrategy.HighestHp, true)]
    [InlineData(EnemyTargetingStrategy.Nearest, true)]
    [InlineData(EnemyTargetingStrategy.TankAssist, true)]
    [InlineData(EnemyTargetingStrategy.CurrentTarget, false)]
    [InlineData(EnemyTargetingStrategy.FocusTarget, false)]
    public void IsAggregateStrategy_ClassifiesCorrectly(EnemyTargetingStrategy strategy, bool expected) =>
        Assert.Equal(expected, CombatRetargetPolicy.IsAggregateStrategy(strategy));

    [Fact]
    public void ShouldUnpauseForCombatRetarget_WhenInCombatWithDeadTargetAndHostiles_ReturnsTrue()
    {
        var unpause = CombatRetargetPolicy.ShouldUnpauseForCombatRetarget(
            pauseWhenNoTarget: true,
            hasValidUserSelectedEnemy: false,
            hasLiveStickyTarget: false,
            playerInCombat: true,
            hardTargetInvalid: true,
            hasLiveHostilesNearby: true);

        Assert.True(unpause);
    }

    [Fact]
    public void ShouldUnpauseForCombatRetarget_WhenOutOfCombat_StaysPaused()
    {
        var unpause = CombatRetargetPolicy.ShouldUnpauseForCombatRetarget(
            pauseWhenNoTarget: true,
            hasValidUserSelectedEnemy: false,
            hasLiveStickyTarget: false,
            playerInCombat: false,
            hardTargetInvalid: true,
            hasLiveHostilesNearby: true);

        Assert.False(unpause);
    }

    [Fact]
    public void ShouldUnpauseForCombatRetarget_WhenNoHostiles_StaysPaused()
    {
        var unpause = CombatRetargetPolicy.ShouldUnpauseForCombatRetarget(
            pauseWhenNoTarget: true,
            hasValidUserSelectedEnemy: false,
            hasLiveStickyTarget: false,
            playerInCombat: true,
            hardTargetInvalid: true,
            hasLiveHostilesNearby: false);

        Assert.False(unpause);
    }

    [Fact]
    public void ShouldRelaxStrictOnCombatDeath_WhenAggregateStrategyAndCombatScenario_ReturnsTrue()
    {
        var relax = CombatRetargetPolicy.ShouldRelaxStrictOnCombatDeath(
            strictCurrentTargetStrategy: true,
            enemyStrategy: EnemyTargetingStrategy.LowestHp,
            isCombatRetargetScenario: true);

        Assert.True(relax);
    }

    [Fact]
    public void ShouldRelaxStrictOnCombatDeath_WhenExplicitStrategy_ReturnsFalse()
    {
        var relax = CombatRetargetPolicy.ShouldRelaxStrictOnCombatDeath(
            strictCurrentTargetStrategy: true,
            enemyStrategy: EnemyTargetingStrategy.CurrentTarget,
            isCombatRetargetScenario: true);

        Assert.False(relax);
    }

    [Theory]
    [InlineData(EnemyTargetingStrategy.LowestHp, EnemyTargetingStrategy.LowestHp)]
    [InlineData(EnemyTargetingStrategy.Nearest, EnemyTargetingStrategy.Nearest)]
    [InlineData(EnemyTargetingStrategy.CurrentTarget, EnemyTargetingStrategy.LowestHp)]
    [InlineData(EnemyTargetingStrategy.FocusTarget, EnemyTargetingStrategy.LowestHp)]
    public void ResolveAutoRetargetStrategy_PicksAggregateOrFallsBackToLowestHp(
        EnemyTargetingStrategy configured,
        EnemyTargetingStrategy expected) =>
        Assert.Equal(expected, CombatRetargetPolicy.ResolveAutoRetargetStrategy(configured));
}
