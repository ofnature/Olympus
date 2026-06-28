using Moq;
using Daedalus.Data;
using Daedalus.Rotation.ThanatosCore.Abilities;
using Daedalus.Rotation.ThanatosCore.Modules;
using Daedalus.Services.Action;
using Daedalus.Tests.Mocks;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Daedalus.Tests.Rotation.ThanatosCore;
using Xunit;

namespace Daedalus.Tests.Rotation.ThanatosCore.Modules;

/// <summary>
/// Regression tests for the Ideal Host → Enshroud trigger. Ideal Host grants a FREE Enshroud (no Shroud
/// cost), so Enshroud must fire even at 0 Shroud when Ideal Host is up — previously it was gated on the
/// Shroud gauge and never recognized the free Enshroud.
/// </summary>
public class BuffModuleIdealHostTests
{
    private readonly BuffModule _module = new();

    private static Mock<IActionService> ReadyActions()
    {
        var actions = MockBuilders.CreateMockActionService();
        actions.Setup(x => x.IsActionReady(It.IsAny<uint>())).Returns(true);
        return actions;
    }

    [Fact]
    public void Enshroud_Pushed_WithIdealHost_EvenAtZeroShroud()
    {
        var actions = ReadyActions();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actions);
        var context = ThanatosTestContext.Create(
            actionService: actions, level: 100,
            shroud: 0, hasIdealHost: true,
            hasDeathsDesign: true, deathsDesignRemaining: 20f);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.Contains(scheduler.InspectOgcdQueue(), c => c.Behavior == ThanatosAbilities.Enshroud);
    }

    [Fact]
    public void Enshroud_NotPushed_WithoutIdealHost_AtZeroShroud()
    {
        var actions = ReadyActions();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actions);
        var context = ThanatosTestContext.Create(
            actionService: actions, level: 100,
            shroud: 0, hasIdealHost: false,
            hasDeathsDesign: true, deathsDesignRemaining: 20f);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.DoesNotContain(scheduler.InspectOgcdQueue(), c => c.Behavior == ThanatosAbilities.Enshroud);
    }
}
