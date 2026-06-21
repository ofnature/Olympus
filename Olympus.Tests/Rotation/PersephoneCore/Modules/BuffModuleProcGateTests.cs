using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Moq;
using Olympus.Data;
using Olympus.Rotation.PersephoneCore.Abilities;
using Olympus.Rotation.PersephoneCore.Modules;
using Olympus.Services;
using Olympus.Services.Targeting;
using Olympus.Tests.Mocks;
using Olympus.Tests.Rotation.Common.Scheduling;
using Xunit;

namespace Olympus.Tests.Rotation.PersephoneCore.Modules;

/// <summary>
/// Regression guards for SMN proc-gated oGCDs (Phase A parity with RSR).
/// </summary>
public class BuffModuleProcGateTests
{
    private readonly BuffModule _module = new();

    [Fact]
    public void SearingFlash_NotPushed_WithoutRubysGlimmer()
    {
        var enemy = CreateMockEnemy();
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);

        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(SMNActions.SearingFlash.ActionId)).Returns(true);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = PersephoneTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            hasRubysGlimmer: false,
            hasSearingLight: true);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var ogcd = scheduler.InspectOgcdQueue();
        Assert.DoesNotContain(ogcd, c => c.Behavior == PersephoneAbilities.SearingFlash);
    }

    [Fact]
    public void SearingFlash_Pushed_WhenRubysGlimmerActive()
    {
        var enemy = CreateMockEnemy();
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);

        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(SMNActions.SearingFlash.ActionId)).Returns(true);
        actionService.Setup(x => x.PlayerHasStatus(SMNActions.StatusIds.RubysGlimmer)).Returns(true);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = PersephoneTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            hasRubysGlimmer: true);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var ogcd = scheduler.InspectOgcdQueue();
        Assert.Contains(ogcd, c => c.Behavior == PersephoneAbilities.SearingFlash);
    }

    [Fact]
    public void MountainBuster_NotPushed_WithoutSlotReplacement()
    {
        var enemy = CreateMockEnemy();
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);

        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = PersephoneTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            hasTitansFavor: true,
            mountainBusterReady: false);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var ogcd = scheduler.InspectOgcdQueue();
        Assert.DoesNotContain(ogcd, c => c.Behavior == PersephoneAbilities.MountainBuster);
    }

    [Fact]
    public void EnergyDrain_NotPushed_OutsideDemiPhase()
    {
        var enemy = CreateMockEnemy();
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);

        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(SMNActions.EnergyDrain.ActionId)).Returns(true);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = PersephoneTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            energyDrainReady: true,
            isDemiSummonActive: false);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var ogcd = scheduler.InspectOgcdQueue();
        Assert.DoesNotContain(ogcd, c => c.Behavior == PersephoneAbilities.EnergyDrain);
        Assert.DoesNotContain(ogcd, c => c.Behavior == PersephoneAbilities.EnergySiphon);
    }

    [Fact]
    public void EnergyDrain_Pushed_DuringDemiPhase()
    {
        var enemy = CreateMockEnemy();
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);
        targeting.Setup(x => x.CountEnemiesInRange(It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(1);

        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(SMNActions.EnergyDrain.ActionId)).Returns(true);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = PersephoneTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            energyDrainReady: true,
            isDemiSummonActive: true,
            demiSummonTimer: 15f);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var ogcd = scheduler.InspectOgcdQueue();
        Assert.Contains(ogcd, c => c.Behavior == PersephoneAbilities.EnergyDrain);
    }

    private static Mock<IBattleNpc> CreateMockEnemy(ulong objectId = 99999UL)
    {
        var mock = new Mock<IBattleNpc>();
        mock.Setup(x => x.GameObjectId).Returns(objectId);
        mock.Setup(x => x.CurrentHp).Returns(10000u);
        mock.Setup(x => x.MaxHp).Returns(10000u);
        return mock;
    }
}
