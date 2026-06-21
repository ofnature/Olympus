using Moq;
using Olympus.Data;
using Olympus.Rotation.HermesCore.Context;
using Olympus.Rotation.HermesCore.Helpers;
using Olympus.Services;
using Olympus.Services.Action;

namespace Olympus.Tests.Rotation.HermesCore.Helpers;

public class HermesBurstPrepHelperTests
{
    [Fact]
    public void IsBurstPrepWindow_False_WithoutSuiton()
    {
        var ctx = CreateContext(hasSuiton: false, kunaisBaneReady: true);
        Assert.False(HermesBurstPrepHelper.IsBurstPrepWindow(ctx));
    }

    [Fact]
    public void IsBurstPrepWindow_True_WithSuitonAndKunaisBaneReady()
    {
        var ctx = CreateContext(hasSuiton: true, kunaisBaneReady: true, level: 92);
        Assert.True(HermesBurstPrepHelper.IsBurstPrepWindow(ctx));
    }

    [Fact]
    public void ShouldHoldComboGcds_AlwaysFalse_AbbPolicy()
    {
        var ctx = CreateContext(hasSuiton: true, kunaisBaneReady: true, level: 92);
        var mudra = new MudraHelper();

        Assert.False(HermesBurstPrepHelper.ShouldHoldComboGcds(ctx, mudra));
    }

    [Fact]
    public void WouldHoldKunaisBane_False_WhenBurstPoolingDisabled()
    {
        var ctx = CreateContext(hasSuiton: true, kunaisBaneReady: true, level: 92, enableBurstPooling: false);
        var burst = new Mock<IBurstWindowService>();
        burst.Setup(s => s.IsBurstImminent(It.IsAny<float>())).Returns(true);

        Assert.False(HermesBurstPrepHelper.WouldHoldKunaisBane(ctx, burst.Object));
    }

    [Fact]
    public void WouldHoldKunaisBane_True_WhenBurstPoolingEnabledAndImminent()
    {
        var ctx = CreateContext(hasSuiton: true, kunaisBaneReady: true, level: 92, enableBurstPooling: true);
        var burst = new Mock<IBurstWindowService>();
        burst.Setup(s => s.IsBurstImminent(It.IsAny<float>())).Returns(true);
        burst.Setup(s => s.IsInBurstWindow).Returns(false);

        Assert.True(HermesBurstPrepHelper.WouldHoldKunaisBane(ctx, burst.Object));
    }

    [Fact]
    public void ShouldSuppressAoE_False_WhenOnlyShadowWalkerWithoutKbReady()
    {
        var ctx = CreateContext(hasSuiton: true, kunaisBaneReady: false, level: 92);
        Assert.False(HermesBurstPrepHelper.ShouldSuppressAoE(ctx, enemyCount: 5, aoeThreshold: 3));
    }

    [Fact]
    public void ShouldSuppressAoE_True_DuringBurstPrep()
    {
        var ctx = CreateContext(hasSuiton: true, kunaisBaneReady: true, level: 92);
        Assert.True(HermesBurstPrepHelper.ShouldSuppressAoE(ctx, enemyCount: 5, aoeThreshold: 3));
    }

    [Fact]
    public void HasSuitonBurstLatch_SuppressesDuplicateSuiton()
    {
        var mudra = new MudraHelper();
        mudra.MarkSuitonExecuted();

        Assert.True(mudra.HasSuitonBurstLatch);
    }

    private static IHermesContext CreateContext(
        bool hasSuiton,
        bool kunaisBaneReady,
        byte level = 100,
        bool enableBurstPooling = false)
    {
        var actionService = new Mock<IActionService>();
        actionService.Setup(s => s.IsActionReady(NINActions.KunaisBane.ActionId)).Returns(kunaisBaneReady);
        actionService.Setup(s => s.IsActionReady(NINActions.TrickAttack.ActionId)).Returns(false);

        var config = new Configuration { Ninja = { EnableBurstPooling = enableBurstPooling } };

        var ctx = new Mock<IHermesContext>();
        ctx.Setup(c => c.HasSuiton).Returns(hasSuiton);
        ctx.Setup(c => c.ActionService).Returns(actionService.Object);
        ctx.Setup(c => c.Player.Level).Returns(level);
        ctx.Setup(c => c.Configuration).Returns(config);
        ctx.Setup(c => c.TimelineService).Returns((Olympus.Timeline.ITimelineService?)null);
        return ctx.Object;
    }
}
