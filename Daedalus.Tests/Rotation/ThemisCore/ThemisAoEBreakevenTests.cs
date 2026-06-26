using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Moq;
using Daedalus.Data;
using Daedalus.Models.Action;
using Daedalus.Rotation.ThemisCore.Abilities;
using Daedalus.Rotation.ThemisCore.Context;
using Daedalus.Rotation.ThemisCore.Modules;
using Daedalus.Services.Action;
using Daedalus.Services.Targeting;
using Daedalus.Tests.Mocks;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Xunit;

namespace Daedalus.Tests.Rotation.ThemisCore;

public sealed class ThemisAoEBreakevenTests
{
    private readonly DamageModule _module = new();

    [Fact]
    public void CollectCandidates_TwoEnemies_WithPaladinOverrideTwo_PushesAoECombo()
    {
        var targeting = BuildMeleeTargeting(enemyCount: 2);
        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var config = ThemisTestContext.CreateDefaultPaladinConfiguration();
        config.Tank.AoEMinTargets = 3;
        config.Tank.PaladinAoEMinTargetsOverride = 2;
        config.Tank.EnableAoEDamage = true;

        var context = ThemisTestContext.Create(
            config: config,
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            comboStep: 0);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == ThemisAbilities.TotalEclipse);
    }

    [Fact]
    public void CollectCandidates_TwoEnemies_InheritingGlobalThree_PushesSingleTargetCombo()
    {
        var targeting = BuildMeleeTargeting(enemyCount: 2);
        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var config = ThemisTestContext.CreateDefaultPaladinConfiguration();
        config.Tank.AoEMinTargets = 3;
        config.Tank.PaladinAoEMinTargetsOverride = null;
        config.Tank.EnableAoEDamage = true;

        var context = ThemisTestContext.Create(
            config: config,
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            comboStep: 0);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == ThemisAbilities.FastBlade);
        Assert.DoesNotContain(gcd, c => c.Behavior == ThemisAbilities.TotalEclipse);
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
}
