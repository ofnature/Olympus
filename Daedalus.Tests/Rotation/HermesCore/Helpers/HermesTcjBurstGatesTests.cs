using Moq;
using Daedalus.Data;
using Daedalus.Rotation.HermesCore.Context;
using Daedalus.Rotation.HermesCore.Helpers;
using Daedalus.Rotation.HermesCore.Modules;
using Daedalus.Services;
using Daedalus.Tests.Mocks;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Daedalus.Tests.Rotation.HermesCore;
using Xunit;

namespace Daedalus.Tests.Rotation.HermesCore.Helpers;

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
