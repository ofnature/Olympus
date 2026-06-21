using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Moq;
using Olympus.Config.DPS;
using Olympus.Data;
using Olympus.Models;
using Olympus.Rotation.IrisCore.Abilities;
using Olympus.Rotation.IrisCore.Modules;
using Olympus.Services;
using Olympus.Services.Action;
using Olympus.Services.Targeting;
using Olympus.Tests.Mocks;
using Olympus.Tests.Rotation.Common.Scheduling;
using Olympus.Tests.Rotation.IrisCore;
using Xunit;

namespace Olympus.Tests.Rotation.IrisCore.Modules;

public class DamageModulePhaseCDTests
{
    [Fact]
    public void MogPortrait_NotPushed_WhenBurstPoolingAndBurstImminent()
    {
        var enemy = CreateMockEnemy();
        var targeting = BuildTargeting(enemy);
        var burst = new Mock<IBurstWindowService>();
        burst.Setup(x => x.IsBurstImminent(8f)).Returns(true);
        burst.Setup(x => x.IsInBurstWindow).Returns(false);

        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var module = new BuffModule(burst.Object);
        var context = IrisTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            inCombat: true,
            mogReady: true);

        module.CollectCandidates(context, scheduler, isMoving: false);

        var ogcd = scheduler.InspectOgcdQueue();
        Assert.DoesNotContain(ogcd, c => c.Behavior == IrisAbilities.MogOfTheAges);
    }

    [Fact]
    public void StrikingMuse_NotPushed_WhenStarryMuseWithinSixtySeconds()
    {
        var burst = new Mock<IBurstWindowService>();
        burst.Setup(x => x.IsInBurstWindow).Returns(false);

        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetCooldownRemaining(PCTActions.StarryMuse.ActionId)).Returns(30f);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var module = new BuffModule(burst.Object);
        var context = IrisTestContext.Create(
            actionService: actionService,
            level: 100,
            inCombat: true,
            hasWeaponCanvas: true,
            strikingMuseReady: true);

        module.CollectCandidates(context, scheduler, isMoving: false);

        var ogcd = scheduler.InspectOgcdQueue();
        Assert.DoesNotContain(ogcd, c => c.Behavior == IrisAbilities.StrikingMuse);
    }

    [Fact]
    public void RepaintMotif_NotPushed_WhenBurstImminent()
    {
        var enemy = CreateMockEnemy();
        var targeting = BuildTargeting(enemy);
        var burst = new Mock<IBurstWindowService>();
        burst.Setup(x => x.IsBurstImminent(10f)).Returns(true);
        burst.Setup(x => x.IsInBurstWindow).Returns(false);

        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetAdjustedActionId(PCTActions.WeaponMotif.ActionId))
            .Returns(PCTActions.HammerMotif.ActionId);
        actionService.Setup(x => x.GetCooldownRemaining(PCTActions.StarryMuse.ActionId)).Returns(120f);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var module = new DamageModule(burst.Object);
        var context = IrisTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            inCombat: true,
            needsWeaponMotif: true,
            needsCreatureMotif: false,
            needsLandscapeMotif: false);

        module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.DoesNotContain(gcd, c => c.Behavior == IrisAbilities.HammerMotif);
    }

    [Fact]
    public void BaseCombo_UsesSmartAoETarget_WhenShouldUseAoe()
    {
        var enemy = CreateMockEnemy(100UL);
        var smartTarget = CreateMockEnemy(200UL);
        var targeting = BuildTargeting(enemy);

        var smartAoE = new Mock<ISmartAoEService>();
        smartAoE.Setup(x => x.FindBestAoETarget(
                PCTActions.Fire2InRed.ActionId,
                It.IsAny<float>(),
                It.IsAny<IPlayerCharacter>(),
                It.IsAny<bool>()))
            .Returns(new AoEResult(smartTarget.Object, 4, 0f, AoEShape.Circle));

        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var module = new DamageModule(smartAoEService: smartAoE.Object);
        var context = IrisTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            inCombat: true,
            shouldUseAoe: true,
            nearbyEnemyCount: 3);

        module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        var fire2 = Assert.Single(gcd, c => c.Behavior == IrisAbilities.Fire2InRed);
        Assert.Equal(200UL, fire2.TargetId);
    }

    private static Configuration Config(Action<PictomancerConfig>? configure = null)
    {
        var config = IrisTestContext.CreateDefaultPctConfiguration();
        configure?.Invoke(config.Pictomancer);
        return config;
    }

    private static Mock<ITargetingService> BuildTargeting(Mock<IBattleNpc> enemy)
    {
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);
        return targeting;
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
