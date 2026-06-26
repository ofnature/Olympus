using Daedalus.Config;
using Daedalus.Rotation.Common.Helpers;
using Daedalus.Services.Targeting;
using Xunit;

namespace Daedalus.Tests.Rotation.Common.Helpers;

public class TankTargetingHelperTests
{
    [Theory]
    [InlineData(EnemyTargetingStrategy.LowestHp)]
    [InlineData(EnemyTargetingStrategy.Nearest)]
    [InlineData(EnemyTargetingStrategy.TankAssist)]
    public void ResolveEnemyStrategy_IgnoreAddsOffWithCoTank_ReturnsConfigured(EnemyTargetingStrategy configured)
    {
        var tank = new TankConfig { IgnoreAddsWithCoTank = false };

        var result = TankTargetingHelper.ResolveEnemyStrategy(tank, configured, hasCoTank: true);

        Assert.Equal(configured, result);
    }

    [Fact]
    public void ResolveEnemyStrategy_IgnoreAddsOnButSolo_ReturnsConfigured()
    {
        var tank = new TankConfig { IgnoreAddsWithCoTank = true };

        var result = TankTargetingHelper.ResolveEnemyStrategy(
            tank, EnemyTargetingStrategy.LowestHp, hasCoTank: false);

        Assert.Equal(EnemyTargetingStrategy.LowestHp, result);
    }

    [Fact]
    public void ResolveEnemyStrategy_IgnoreAddsOnWithCoTank_ForcesCurrentTarget()
    {
        var tank = new TankConfig { IgnoreAddsWithCoTank = true };

        var result = TankTargetingHelper.ResolveEnemyStrategy(
            tank, EnemyTargetingStrategy.LowestHp, hasCoTank: true);

        Assert.Equal(EnemyTargetingStrategy.CurrentTarget, result);
    }
}
