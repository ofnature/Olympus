using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Moq;
using Daedalus.Data;
using Daedalus.Rotation.ThemisCore.Abilities;
using Daedalus.Rotation.ThemisCore.Modules;
using Daedalus.Services.Action;
using Daedalus.Services.Tank;
using Daedalus.Services.Targeting;
using Daedalus.Tests.Mocks;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Xunit;

namespace Daedalus.Tests.Rotation.ThemisCore.Modules;

/// <summary>
/// "Don't chase lost mobs": when a mob has slipped to another player and is out of melee, the tank
/// should NOT dash after it with the gap-closer (Intervene). Provoke (EnmityModule) + Shield Lob
/// reclaim it in place. An out-of-melee mob we have NOT lost still gets the normal dash-to-engage.
/// </summary>
public sealed class ThemisLostMobChaseTests
{
    private readonly DamageModule _module = new();

    private static Mock<ITargetingService> BuildOutOfMeleeTargeting(IBattleNpc enemy)
    {
        var targeting = MockBuilders.CreateMockTargetingService();
        // Out of melee: the melee-action lookup misses, but the 20y engage lookup finds the mob.
        targeting.Setup(x => x.FindEnemyForAction(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<uint>(), It.IsAny<IPlayerCharacter>()))
            .Returns((IBattleNpc?)null);
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy);
        return targeting;
    }

    private static Mock<IBattleNpc> CreateMockEnemy(ulong id = 12345UL)
    {
        var mock = new Mock<IBattleNpc>();
        mock.Setup(x => x.GameObjectId).Returns(id);
        mock.Setup(x => x.CurrentHp).Returns(10000u);
        mock.Setup(x => x.MaxHp).Returns(10000u);
        return mock;
    }

    private static Mock<IEnmityService> EnmityWithLostState(bool lost)
    {
        var enmity = new Mock<IEnmityService>();
        enmity.Setup(x => x.HasLostAggroToOther(It.IsAny<IBattleChara>(), It.IsAny<uint>())).Returns(lost);
        return enmity;
    }

    [Fact]
    public void LostMob_OutOfMelee_DoesNotQueueIntervene_ButQueuesShieldLob()
    {
        var enemy = CreateMockEnemy();
        var targeting = BuildOutOfMeleeTargeting(enemy.Object);
        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = ThemisTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            enmityService: EnmityWithLostState(true)); // PullRangedMobs off (default), SuppressGapCloserOnLostMob on (default)

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.DoesNotContain(scheduler.InspectOgcdQueue(), c => c.Behavior == ThemisAbilities.Intervene);
        Assert.Contains(scheduler.InspectGcdQueue(), c => c.Behavior == ThemisAbilities.ShieldLob);
    }

    [Fact]
    public void NotLostMob_OutOfMelee_StillQueuesIntervene()
    {
        var enemy = CreateMockEnemy();
        var targeting = BuildOutOfMeleeTargeting(enemy.Object);
        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = ThemisTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            enmityService: EnmityWithLostState(false));

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.Contains(scheduler.InspectOgcdQueue(), c => c.Behavior == ThemisAbilities.Intervene);
    }

    [Fact]
    public void LostMob_ButSuppressionDisabled_StillQueuesIntervene()
    {
        var enemy = CreateMockEnemy();
        var targeting = BuildOutOfMeleeTargeting(enemy.Object);
        var actionService = MockBuilders.CreateMockActionService();
        var config = ThemisTestContext.CreateDefaultPaladinConfiguration();
        config.Tank.SuppressGapCloserOnLostMob = false;
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = ThemisTestContext.Create(
            config: config,
            actionService: actionService,
            targetingService: targeting,
            enmityService: EnmityWithLostState(true));

        _module.CollectCandidates(context, scheduler, isMoving: false);

        Assert.Contains(scheduler.InspectOgcdQueue(), c => c.Behavior == ThemisAbilities.Intervene);
    }
}
