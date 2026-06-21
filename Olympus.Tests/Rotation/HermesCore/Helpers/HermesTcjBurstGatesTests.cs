using Moq;
using Olympus.Data;
using Olympus.Rotation.HermesCore.Context;
using Olympus.Rotation.HermesCore.Helpers;
using Olympus.Rotation.HermesCore.Modules;
using Olympus.Services;
using Olympus.Tests.Mocks;
using Olympus.Tests.Rotation.Common.Scheduling;
using Olympus.Tests.Rotation.HermesCore;
using Xunit;

namespace Olympus.Tests.Rotation.HermesCore.Helpers;

public class HermesTcjBurstGatesTests
{
    [Fact]
    public void CanPushTenChiJinOgcd_BlocksWhenKassatsuActive()
    {
        var context = HermesTestContext.Create(hasKassatsu: true, inTrickAttack: true);
        Assert.False(HermesTcjBurstGates.CanPushTenChiJinOgcd(context));
    }

    [Fact]
    public void CanPushTenChiJinOgcd_BlocksWhenGameMudraStatusActive()
    {
        var context = HermesTestContext.Create(hasGameMudraStatus: true, inTrickAttack: true);
        Assert.False(HermesTcjBurstGates.CanPushTenChiJinOgcd(context));
    }

    [Fact]
    public void CanPushTenChiJinOgcd_BlocksWhenSuitonBuffActive()
    {
        var context = HermesTestContext.Create(hasSuiton: true, inTrickAttack: true);
        Assert.False(HermesTcjBurstGates.CanPushTenChiJinOgcd(context));
    }

    [Fact]
    public void CanPushTenChiJinOgcd_AllowsInTrickAttackWindow()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(NINActions.TenChiJin.ActionId)).Returns(true);

        var context = HermesTestContext.Create(
            actionService: actionService,
            inTrickAttack: true,
            hasSuiton: false);

        Assert.True(HermesTcjBurstGates.CanPushTenChiJinOgcd(context));
    }

    [Fact]
    public void CanPushTenChiJinOgcd_AllowsWhileMoving()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(NINActions.TenChiJin.ActionId)).Returns(true);

        var context = HermesTestContext.Create(
            actionService: actionService,
            inTrickAttack: true,
            isMoving: true);

        Assert.True(HermesTcjBurstGates.CanPushTenChiJinOgcd(context));
    }

    [Fact]
    public void PassesTenChargeGuard_BlocksLongTenRecast()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(NINActions.Ten.ActionId)).Returns(false);
        actionService.Setup(x => x.GetRecastTimeElapsed(NINActions.Ten.ActionId)).Returns(35f);

        Assert.False(HermesTcjBurstGates.PassesTenChargeGuard(actionService.Object));
    }
}
