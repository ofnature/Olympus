using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Moq;
using Olympus.Data;
using Olympus.Models.Action;
using Olympus.Rotation.ThemisCore.Abilities;
using Olympus.Rotation.ThemisCore.Context;
using Olympus.Rotation.ThemisCore.Modules;
using Olympus.Services.Action;
using Olympus.Services.Targeting;
using Olympus.Tests.Mocks;
using Olympus.Tests.Rotation.Common.Scheduling;
using Xunit;

namespace Olympus.Tests.Rotation.ThemisCore;

/// <summary>
/// Regression guards for combo continuation deadlocks: when the module pushes a
/// combo finisher but the scheduler ActionStatus gate blocks it, the starter
/// fallback at lower priority must still dispatch.
/// </summary>
public sealed class ThemisComboFallbackTests
{
    private readonly DamageModule _module = new();

    [Fact]
    public void CollectCandidates_AoEComboStep2_BothProminenceAndTotalEclipsePushed()
    {
        var targeting = BuildMeleeTargeting(enemyCount: 3);
        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = CreateAoEComboContext(targeting, actionService, comboStep: 2);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == ThemisAbilities.Prominence && c.Priority == 6);
        Assert.Contains(gcd, c => c.Behavior == ThemisAbilities.TotalEclipse && c.Priority == 7);
    }

    [Fact]
    public void Dispatch_AoEComboStep2_ProminenceActionStatusBlocked_DispatchesTotalEclipseFallback()
    {
        var targeting = BuildMeleeTargeting(enemyCount: 3);
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.CanExecuteActionId(It.IsAny<uint>()))
            .Returns((uint id) => id != PLDActions.Prominence.ActionId);
        actionService.Setup(x => x.ExecuteGcd(It.IsAny<ActionDefinition>(), It.IsAny<ulong>()))
            .Returns(true);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = CreateAoEComboContext(targeting, actionService, comboStep: 2);

        _module.CollectCandidates(context, scheduler, isMoving: false);
        var result = scheduler.DispatchGcd(context);

        Assert.True(result.Dispatched);
        Assert.Same(ThemisAbilities.TotalEclipse, result.Winner);
        actionService.Verify(
            x => x.ExecuteGcd(
                It.Is<ActionDefinition>(a => a.ActionId == PLDActions.TotalEclipse.ActionId),
                It.IsAny<ulong>()),
            Times.Once);
        actionService.Verify(
            x => x.ExecuteGcd(
                It.Is<ActionDefinition>(a => a.ActionId == PLDActions.Prominence.ActionId),
                It.IsAny<ulong>()),
            Times.Never);
    }

    [Fact]
    public void CollectCandidates_STComboStep2_BothRiotBladeAndFastBladePushed()
    {
        var targeting = BuildMeleeTargeting(enemyCount: 1);
        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = ThemisTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            objectTable: BuildObjectTableForMelee(targeting, enemyId: 12345UL),
            level: 100,
            comboStep: 2,
            lastComboAction: PLDActions.FastBlade.ActionId,
            comboTimeRemaining: 30f);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == ThemisAbilities.RiotBlade && c.Priority == 6);
        Assert.Contains(gcd, c => c.Behavior == ThemisAbilities.FastBlade && c.Priority == 7);
    }

    [Fact]
    public void CollectCandidates_STComboStep3_BothFinisherAndFastBladePushed()
    {
        var targeting = BuildMeleeTargeting(enemyCount: 1);
        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = ThemisTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            objectTable: BuildObjectTableForMelee(targeting, enemyId: 12345UL),
            level: 100,
            comboStep: 3,
            lastComboAction: PLDActions.RiotBlade.ActionId,
            comboTimeRemaining: 30f);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == ThemisAbilities.RoyalAuthority && c.Priority == 6);
        Assert.Contains(gcd, c => c.Behavior == ThemisAbilities.FastBlade && c.Priority == 7);
    }

    private static ThemisContext CreateAoEComboContext(
        Mock<ITargetingService> targeting,
        Mock<IActionService> actionService,
        int comboStep)
    {
        var enemy = CreateMockEnemy(12345UL);
        var player = MockBuilders.CreateMockPlayerCharacter();
        var objectTable = MockBuilders.CreateMockObjectTable();
        objectTable.Setup(x => x.SearchById(player.Object.GameObjectId)).Returns(player.Object);
        objectTable.Setup(x => x.SearchById(enemy.Object.GameObjectId)).Returns(enemy.Object);

        return ThemisTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            objectTable: objectTable,
            level: 100,
            comboStep: comboStep,
            lastComboAction: PLDActions.TotalEclipse.ActionId,
            comboTimeRemaining: 30f);
    }

    private static Mock<ITargetingService> BuildMeleeTargeting(int enemyCount)
    {
        var enemy = CreateMockEnemy(12345UL);
        var targeting = MockBuilders.CreateMockTargetingService(countEnemiesInRange: enemyCount);
        targeting.Setup(x => x.FindEnemyForAction(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<uint>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);
        return targeting;
    }

    private static Mock<IBattleNpc> CreateMockEnemy(ulong objectId)
    {
        var mock = new Mock<IBattleNpc>();
        mock.Setup(x => x.GameObjectId).Returns(objectId);
        mock.Setup(x => x.CurrentHp).Returns(10000u);
        mock.Setup(x => x.MaxHp).Returns(10000u);
        return mock;
    }

    private static Mock<IObjectTable> BuildObjectTableForMelee(Mock<ITargetingService> targeting, ulong enemyId)
    {
        var enemy = CreateMockEnemy(enemyId);
        var player = MockBuilders.CreateMockPlayerCharacter();
        var objectTable = MockBuilders.CreateMockObjectTable();
        objectTable.Setup(x => x.SearchById(player.Object.GameObjectId)).Returns(player.Object);
        objectTable.Setup(x => x.SearchById(enemy.Object.GameObjectId)).Returns(enemy.Object);
        return objectTable;
    }
}
