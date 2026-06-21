using Moq;
using Olympus.Data;
using Olympus.Rotation.HermesCore.Context;
using Olympus.Rotation.HermesCore.Helpers;
using Olympus.Services.Action;

namespace Olympus.Tests.Rotation.HermesCore.Helpers;

public class HermesNinjutsuDiagnosticsTests
{
    [Fact]
    public void EvaluateNeedsSuiton_AoEPull_DeferredForDoton()
    {
        var actionService = new Mock<IActionService>();
        actionService.Setup(s => s.IsActionReady(NINActions.KunaisBane.ActionId)).Returns(true);

        var config = new Configuration();
        config.Ninja.EnableAoERotation = true;
        config.Ninja.AoEMinTargets = 3;

        var ctx = new Mock<IHermesContext>();
        ctx.Setup(c => c.Configuration).Returns(config);
        ctx.Setup(c => c.HasSuiton).Returns(false);
        ctx.Setup(c => c.ActionService).Returns(actionService.Object);
        ctx.Setup(c => c.Player.Level).Returns((byte)100);

        var result = HermesNinjutsuDiagnostics.EvaluateNeedsSuiton(
            ctx.Object, 100, enemyCount: 4, out var reason);

        Assert.False(result);
        Assert.Contains("Doton", reason);
    }

    [Fact]
    public void EvaluateNeedsSuiton_TwoTargets_OpensWithDotonBeforeSuiton()
    {
        var actionService = new Mock<IActionService>();
        actionService.Setup(s => s.IsActionReady(NINActions.KunaisBane.ActionId)).Returns(true);

        var config = new Configuration();
        config.Ninja.EnableAoERotation = true;
        config.Ninja.UseDotonForAoE = true;

        var ctx = new Mock<IHermesContext>();
        ctx.Setup(c => c.Configuration).Returns(config);
        ctx.Setup(c => c.HasSuiton).Returns(false);
        ctx.Setup(c => c.ActionService).Returns(actionService.Object);
        ctx.Setup(c => c.Player.Level).Returns((byte)100);

        var result = HermesNinjutsuDiagnostics.EvaluateNeedsSuiton(
            ctx.Object, 100, enemyCount: 2, out var reason);

        Assert.False(result);
        Assert.Contains("opening Doton", reason);
    }

    [Fact]
    public void EvaluateShouldStartNinjutsu_AoEPull_UsesFillerNotSuiton()
    {
        var actionService = new Mock<IActionService>();
        actionService.Setup(s => s.IsActionReady(NINActions.KunaisBane.ActionId)).Returns(true);
        actionService.Setup(s => s.GetAdjustedActionId(NINActions.Ten.ActionId))
            .Returns(NINActions.Ten.ActionId);
        actionService.Setup(s => s.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(1u);

        var config = new Configuration();
        config.Ninja.EnableAoERotation = true;
        config.Ninja.AoEMinTargets = 3;

        var ctx = new Mock<IHermesContext>();
        ctx.Setup(c => c.Configuration).Returns(config);
        ctx.Setup(c => c.HasSuiton).Returns(false);
        ctx.Setup(c => c.HasKassatsu).Returns(false);
        ctx.Setup(c => c.CanExecuteGcd).Returns(true);
        ctx.Setup(c => c.ActionService).Returns(actionService.Object);
        ctx.Setup(c => c.Player.Level).Returns((byte)100);

        var result = HermesNinjutsuDiagnostics.EvaluateShouldStartNinjutsu(
            ctx.Object, 100, enemyCount: 4, out var reason);

        Assert.True(result);
        Assert.Contains("Filler path", reason);
    }

    [Fact]
    public void EvaluateShouldStartNinjutsu_NeedsSuiton_True_EvenWhenTenOnCooldown()
    {
        var actionService = new Mock<IActionService>();
        actionService.Setup(s => s.IsActionReady(NINActions.KunaisBane.ActionId)).Returns(true);
        actionService.Setup(s => s.IsActionReady(NINActions.Ten.ActionId)).Returns(false);

        var ctx = new Mock<IHermesContext>();
        ctx.Setup(c => c.Configuration).Returns(new Configuration());
        ctx.Setup(c => c.HasSuiton).Returns(false);
        ctx.Setup(c => c.HasKassatsu).Returns(false);
        ctx.Setup(c => c.ActionService).Returns(actionService.Object);
        ctx.Setup(c => c.Player.Level).Returns((byte)100);

        var result = HermesNinjutsuDiagnostics.EvaluateShouldStartNinjutsu(
            ctx.Object, 100, enemyCount: 1, out var reason);

        Assert.True(result);
        Assert.Contains("NeedsSuiton", reason);
        Assert.Contains("waiting for Ten", reason);
    }

    [Fact]
    public void EvaluateShouldStartNinjutsu_False_DuringBurstPrepWindow()
    {
        var actionService = new Mock<IActionService>();
        actionService.Setup(s => s.IsActionReady(NINActions.KunaisBane.ActionId)).Returns(true);
        actionService.Setup(s => s.IsActionReady(NINActions.Ten.ActionId)).Returns(true);

        var ctx = new Mock<IHermesContext>();
        ctx.Setup(c => c.Configuration).Returns(new Configuration());
        ctx.Setup(c => c.HasSuiton).Returns(true);
        ctx.Setup(c => c.HasKassatsu).Returns(false);
        ctx.Setup(c => c.CanExecuteGcd).Returns(true);
        ctx.Setup(c => c.ActionService).Returns(actionService.Object);
        ctx.Setup(c => c.Player.Level).Returns((byte)92);

        var result = HermesNinjutsuDiagnostics.EvaluateShouldStartNinjutsu(
            ctx.Object, 92, enemyCount: 1, out var reason);

        Assert.False(result);
        Assert.Contains("Burst prep", reason);
    }

    [Fact]
    public void EvaluateShouldStartNinjutsu_False_WhenDotonLatchActive()
    {
        var actionService = new Mock<IActionService>();
        actionService.Setup(s => s.IsActionReady(NINActions.KunaisBane.ActionId)).Returns(true);
        actionService.Setup(s => s.GetAdjustedActionId(NINActions.Ten.ActionId))
            .Returns(NINActions.Ten.ActionId);
        actionService.Setup(s => s.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(2u);

        var config = new Configuration();
        config.Ninja.EnableAoERotation = true;
        config.Ninja.AoEMinTargets = 3;

        var mudra = new MudraHelper();
        mudra.MarkDotonExecuted();

        var ctx = new Mock<IHermesContext>();
        ctx.Setup(c => c.Configuration).Returns(config);
        ctx.Setup(c => c.HasSuiton).Returns(false);
        ctx.Setup(c => c.HasKassatsu).Returns(false);
        ctx.Setup(c => c.CanExecuteGcd).Returns(true);
        ctx.Setup(c => c.ActionService).Returns(actionService.Object);
        ctx.Setup(c => c.Player.Level).Returns((byte)100);
        ctx.Setup(c => c.MudraHelper).Returns(mudra);

        var result = HermesNinjutsuDiagnostics.EvaluateShouldStartNinjutsu(
            ctx.Object, 100, enemyCount: 4, out var reason);

        Assert.False(result);
        Assert.Contains("Doton active", reason);
    }
}
