using Daedalus.Config;
using Daedalus.Services.Targeting;
using Xunit;

namespace Daedalus.Tests.Config;

public sealed class RaidStrategyTests
{
    [Fact]
    public void FromGlobal_SeedsFromCurrentTargetingSettings()
    {
        var global = new TargetingConfig
        {
            EnemyStrategy = EnemyTargetingStrategy.TankAssist,
            RetargetUnreachableTarget = false,
            StrictCurrentTargetStrategy = false,
            EnableInvulnerabilityFiltering = false,
        };

        var strategy = RaidTargetingStrategy.FromGlobal(global);

        Assert.True(strategy.Enabled);
        Assert.Equal(EnemyTargetingStrategy.TankAssist, strategy.EnemyStrategy);
        Assert.False(strategy.RetargetUnreachableTarget);
        Assert.False(strategy.StrictCurrentTargetStrategy);
        Assert.False(strategy.EnableInvulnerabilityFiltering);
    }

    [Fact]
    public void ApplyOnto_CopiesOnlyTargetingFields()
    {
        var target = new TargetingConfig
        {
            EnemyStrategy = EnemyTargetingStrategy.LowestHp,
            RetargetUnreachableTarget = true,
            // A field outside MVP scope must be left alone.
            PauseWhenNoTarget = true,
        };

        var strategy = new RaidTargetingStrategy
        {
            EnemyStrategy = EnemyTargetingStrategy.FocusTarget,
            RetargetUnreachableTarget = false,
            StrictCurrentTargetStrategy = false,
            EnableInvulnerabilityFiltering = false,
        };

        strategy.ApplyOnto(target);

        Assert.Equal(EnemyTargetingStrategy.FocusTarget, target.EnemyStrategy);
        Assert.False(target.RetargetUnreachableTarget);
        Assert.False(target.StrictCurrentTargetStrategy);
        Assert.False(target.EnableInvulnerabilityFiltering);
        Assert.True(target.PauseWhenNoTarget); // untouched
    }

    [Fact]
    public void GetActiveTargeting_ReturnsNull_WhenNoEntry()
    {
        var config = new RaidConfig();
        Assert.Null(config.GetActiveTargeting(1234));
    }

    [Fact]
    public void GetActiveTargeting_ReturnsNull_WhenDisabled()
    {
        var config = new RaidConfig();
        config.TargetingByTerritory[1234] = new RaidTargetingStrategy { Enabled = false };

        Assert.Null(config.GetActiveTargeting(1234));
        Assert.NotNull(config.GetTargeting(1234)); // still retrievable for editing
    }

    [Fact]
    public void GetActiveTargeting_ReturnsStrategy_WhenEnabled()
    {
        var config = new RaidConfig();
        var strategy = new RaidTargetingStrategy { Enabled = true };
        config.TargetingByTerritory[1234] = strategy;

        Assert.Same(strategy, config.GetActiveTargeting(1234));
    }
}
