using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Moq;
using Daedalus.Data;
using Daedalus.Rotation.ThemisCore.Abilities;
using Daedalus.Rotation.ThemisCore.Modules;
using Daedalus.Services.Action;
using Daedalus.Services.Targeting;
using Daedalus.Tests.Mocks;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Xunit;

namespace Daedalus.Tests.Rotation.ThemisCore.Modules;

/// <summary>
/// Wall-to-wall add tagging: while moving with a melee target on the current pack, a stray add that
/// isn't on us yet should get tagged with Shield Lob so nothing is left behind. Must not fire when
/// stationary, mid-combo, or with the toggle off.
/// </summary>
public sealed class ThemisMovingAddTagTests
{
    private readonly DamageModule _module = new();

    private static Mock<IBattleNpc> Enemy(ulong id)
    {
        var mock = new Mock<IBattleNpc>();
        mock.Setup(x => x.GameObjectId).Returns(id);
        mock.Setup(x => x.CurrentHp).Returns(10000u);
        mock.Setup(x => x.MaxHp).Returns(10000u);
        return mock;
    }

    /// <summary>Melee target present (in-melee branch) plus a stray add not on us.</summary>
    private static Mock<ITargetingService> BuildTargeting(IBattleNpc meleeTarget, IBattleNpc? stray)
    {
        var targeting = MockBuilders.CreateMockTargetingService(countEnemiesInRange: 1);
        targeting.Setup(x => x.FindEnemyForAction(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<uint>(), It.IsAny<IPlayerCharacter>()))
            .Returns(meleeTarget);
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(meleeTarget);
        targeting.Setup(x => x.CountEnemiesInRangeOfTarget(
                It.IsAny<float>(), It.IsAny<IBattleNpc>(), It.IsAny<IPlayerCharacter>()))
            .Returns(1);
        targeting.Setup(x => x.FindEnemyNotTargetingPlayer(It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(stray);
        return targeting;
    }

    private bool QueuedShieldLobTag(ITargetingService targeting, bool isMoving, bool toggle = true,
        uint lastComboAction = 0, float comboTimer = 0f, ulong strayId = 222UL)
    {
        var actionService = MockBuilders.CreateMockActionService();
        var config = ThemisTestContext.CreateDefaultPaladinConfiguration();
        config.Tank.TagAddsWhileMovingWithRangedAttack = toggle;
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = ThemisTestContext.Create(
            config: config,
            actionService: actionService,
            targetingService: (Mock<ITargetingService>)Mock.Get(targeting),
            level: 100,
            lastComboAction: lastComboAction,
            comboTimeRemaining: comboTimer);

        _module.CollectCandidates(context, scheduler, isMoving: isMoving);

        return System.Linq.Enumerable.Any(scheduler.InspectGcdQueue(),
            c => c.Behavior == ThemisAbilities.ShieldLob && c.TargetId == strayId);
    }

    [Fact]
    public void TagsStrayAdd_WhenMovingAndNotMidCombo()
    {
        var targeting = BuildTargeting(Enemy(111UL).Object, Enemy(222UL).Object);
        Assert.True(QueuedShieldLobTag(targeting.Object, isMoving: true));
    }

    [Fact]
    public void DoesNotTag_WhenStationary()
    {
        var targeting = BuildTargeting(Enemy(111UL).Object, Enemy(222UL).Object);
        Assert.False(QueuedShieldLobTag(targeting.Object, isMoving: false));
    }

    [Fact]
    public void DoesNotTag_WhenMidCombo()
    {
        var targeting = BuildTargeting(Enemy(111UL).Object, Enemy(222UL).Object);
        Assert.False(QueuedShieldLobTag(
            targeting.Object, isMoving: true,
            lastComboAction: PLDActions.FastBlade.ActionId, comboTimer: 30f));
    }

    [Fact]
    public void DoesNotTag_WhenToggleOff()
    {
        var targeting = BuildTargeting(Enemy(111UL).Object, Enemy(222UL).Object);
        Assert.False(QueuedShieldLobTag(targeting.Object, isMoving: true, toggle: false));
    }

    [Fact]
    public void DoesNotTag_WhenNoStrayAdd()
    {
        var targeting = BuildTargeting(Enemy(111UL).Object, stray: null);
        Assert.False(QueuedShieldLobTag(targeting.Object, isMoving: true));
    }
}
