using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Moq;
using Daedalus.Data;
using Daedalus.Rotation.AresCore.Abilities;
using Daedalus.Rotation.AresCore.Modules;
using Daedalus.Services.Targeting;
using Daedalus.Tests.Mocks;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Xunit;

namespace Daedalus.Tests.Rotation.AresCore.Modules;

/// <summary>
/// W2W transit ranged filler (2026-07-01). During wall-to-wall transit AutoDuty drops the hard
/// target, IsDamageTargetingPaused kills the whole damage module, and the chasing pack got ZERO
/// GCDs — a 12-14% uptime hole on both GNB and WAR W2W validation runs. The filler fires the
/// ranged GCD via FindNearbyEnemy (which bypasses the pause and only returns in-combat enemies,
/// so nothing new is ever pulled). Same shape on all four tanks; WAR is the representative test.
/// </summary>
public class AresTransitRangedFillerTests
{
    private readonly DamageModule _module = new();

    [Fact]
    public void TransitFiller_PushesTomahawk_WhenDamageTargetingPaused()
    {
        var targeting = PausedTargetingWithChasingEnemy(out _);
        var scheduler = SchedulerFactory.CreateForTest();
        var context = AresTestContext.CreateMock(targetingService: targeting);

        _module.CollectCandidates(context, scheduler, isMoving: true);

        Assert.Contains(scheduler.InspectGcdQueue(), c => c.Behavior == AresAbilities.Tomahawk);
    }

    [Fact]
    public void TransitFiller_Respects_ConfigToggle()
    {
        var targeting = PausedTargetingWithChasingEnemy(out _);
        var scheduler = SchedulerFactory.CreateForTest();
        var context = AresTestContext.CreateMock(targetingService: targeting);
        context.Configuration.Tank.TransitRangedFiller = false;

        _module.CollectCandidates(context, scheduler, isMoving: true);

        Assert.DoesNotContain(scheduler.InspectGcdQueue(), c => c.Behavior == AresAbilities.Tomahawk);
    }

    [Fact]
    public void TransitFiller_NothingPushed_WhenNoChasingEnemy()
    {
        // Paused AND nothing in combat nearby (true idle) — filler stays silent.
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.IsDamageTargetingPaused()).Returns(true);
        targeting.Setup(x => x.FindNearbyEnemy(It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns((IBattleNpc?)null);

        var scheduler = SchedulerFactory.CreateForTest();
        var context = AresTestContext.CreateMock(targetingService: targeting);

        _module.CollectCandidates(context, scheduler, isMoving: true);

        Assert.Empty(scheduler.InspectGcdQueue());
    }

    [Fact]
    public void TransitFiller_AlsoFires_WhenNothingInEngageRange()
    {
        // Not paused, but FindEnemyForAction + FindEnemy(20y) both come up empty (mobs trailing
        // further back) — the engageTarget==null dead-end must also try the filler.
        var chasing = CreateMockEnemy();
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.IsDamageTargetingPaused()).Returns(false);
        targeting.Setup(x => x.FindEnemyForAction(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<uint>(), It.IsAny<IPlayerCharacter>()))
            .Returns((IBattleNpc?)null);
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns((IBattleNpc?)null);
        targeting.Setup(x => x.FindNearbyEnemy(It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(chasing.Object);

        var scheduler = SchedulerFactory.CreateForTest();
        var context = AresTestContext.CreateMock(targetingService: targeting);

        _module.CollectCandidates(context, scheduler, isMoving: true);

        Assert.Contains(scheduler.InspectGcdQueue(), c => c.Behavior == AresAbilities.Tomahawk);
    }

    [Fact]
    public void StickyTargetBeyondMelee_DemotedToRangedFiller_NotComboGcds()
    {
        // The exact Why-Stuck state from the DRK W2W run: FindEnemyForAction returned a sticky target
        // 15.6y away, the module took the in-melee path, and combo GCDs ("Syphon Strike: out of range
        // 16y > 3y") failed range at dispatch forever. Beyond-melee targets must demote to the
        // out-of-melee branch (ranged GCD), never the melee combo.
        var farEnemy = CreateMockEnemy();
        farEnemy.Setup(x => x.Position).Returns(new System.Numerics.Vector3(16f, 0f, 0f));
        farEnemy.Setup(x => x.HitboxRadius).Returns(0.5f);

        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.IsDamageTargetingPaused()).Returns(false);
        targeting.Setup(x => x.FindEnemyForAction(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<uint>(), It.IsAny<IPlayerCharacter>()))
            .Returns(farEnemy.Object);
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(farEnemy.Object);

        var scheduler = SchedulerFactory.CreateForTest();
        var context = AresTestContext.CreateMock(targetingService: targeting);

        _module.CollectCandidates(context, scheduler, isMoving: true);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == AresAbilities.Tomahawk);
        Assert.DoesNotContain(gcd, c => c.Behavior == AresAbilities.HeavySwing);
    }

    private static Mock<ITargetingService> PausedTargetingWithChasingEnemy(out Mock<IBattleNpc> enemy)
    {
        enemy = CreateMockEnemy();
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.IsDamageTargetingPaused()).Returns(true);
        var captured = enemy.Object;
        targeting.Setup(x => x.FindNearbyEnemy(It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(captured);
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
