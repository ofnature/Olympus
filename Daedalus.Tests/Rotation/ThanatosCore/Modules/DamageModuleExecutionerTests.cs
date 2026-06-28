using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Moq;
using Daedalus.Data;
using Daedalus.Rotation.ThanatosCore.Abilities;
using Daedalus.Rotation.ThanatosCore.Modules;
using Daedalus.Services;
using Daedalus.Services.Action;
using Daedalus.Services.Targeting;
using Daedalus.Tests.Mocks;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Daedalus.Tests.Rotation.ThanatosCore;
using Xunit;

namespace Daedalus.Tests.Rotation.ThanatosCore.Modules;

/// <summary>
/// Regression tests for the Lv.96+ Executioner finishers (Gluttony grants Executioner, not Soul Reaver),
/// which were never dispatched before — Gluttony's stacks went unspent.
/// </summary>
public class DamageModuleExecutionerTests
{
    private readonly DamageModule _module = new();

    private static (Mock<ITargetingService> targeting, Mock<IActionService> actions) Setup()
    {
        var enemy = new Mock<IBattleNpc>();
        enemy.Setup(x => x.GameObjectId).Returns(99999UL);
        enemy.Setup(x => x.CurrentHp).Returns(10000u);
        enemy.Setup(x => x.MaxHp).Returns(10000u);

        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemyForAction(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<uint>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);

        var actions = MockBuilders.CreateMockActionService();
        actions.Setup(x => x.IsActionReady(It.IsAny<uint>())).Returns(true);
        return (targeting, actions);
    }

    [Fact]
    public void ExecutionersGibbet_Pushed_WhenExecutionerActiveAtFlank()
    {
        var (targeting, actions) = Setup();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actions);
        var context = ThanatosTestContext.Create(
            actionService: actions, targetingService: targeting, level: 100,
            hasExecutioner: true, executionerStacks: 2, isAtFlank: true);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.Contains(scheduler.InspectGcdQueue(), c => c.Behavior == ThanatosAbilities.ExecutionersGibbet);
    }

    [Fact]
    public void ExecutionersGallows_Pushed_WhenExecutionerActiveAtRear()
    {
        var (targeting, actions) = Setup();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actions);
        var context = ThanatosTestContext.Create(
            actionService: actions, targetingService: targeting, level: 100,
            hasExecutioner: true, executionerStacks: 2, isAtRear: true);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.Contains(scheduler.InspectGcdQueue(), c => c.Behavior == ThanatosAbilities.ExecutionersGallows);
    }

    [Fact]
    public void SoulSpender_NotPushed_WhileExecutionerStacksPending()
    {
        // Don't fire another Gluttony/Blood Stalk while Executioner stacks are unspent (would waste them).
        var (targeting, actions) = Setup();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actions);
        var context = ThanatosTestContext.Create(
            actionService: actions, targetingService: targeting, level: 100,
            soul: 100, hasExecutioner: true, executionerStacks: 2, isAtFlank: true);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var ogcd = scheduler.InspectOgcdQueue();
        Assert.DoesNotContain(ogcd, c => c.Behavior == ThanatosAbilities.Gluttony);
        Assert.DoesNotContain(ogcd, c => c.Behavior == ThanatosAbilities.BloodStalk);
    }
}
