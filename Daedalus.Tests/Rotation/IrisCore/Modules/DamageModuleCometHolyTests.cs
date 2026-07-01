using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Moq;
using Daedalus.Rotation.IrisCore.Abilities;
using Daedalus.Rotation.IrisCore.Modules;
using Daedalus.Services.Targeting;
using Daedalus.Tests.Mocks;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Daedalus.Tests.Rotation.IrisCore;
using Xunit;

namespace Daedalus.Tests.Rotation.IrisCore.Modules;

/// <summary>
/// Regression guards for the Comet in Black keystone bug (2026-07-01). "Black paint" is NOT a gauge
/// flag — it's paint stacks + the Monochrome Tones STATUS (RSR: Holy = Paint>0 &amp;&amp; !Monochrome,
/// Comet = Paint>0 &amp;&amp; Monochrome). The old gauge read probed CreatureFlags &amp; 0x10 → always
/// false → Comet never fired → Monochrome never cleared → Holy morph-rejected every GCD, Subtractive
/// re-press locked, zero instant GCDs → no weave slots → muses/Striking/Starry all starved.
/// </summary>
public class DamageModuleCometHolyTests
{
    private readonly DamageModule _module = new();

    [Fact]
    public void Comet_Pushed_WhenBlackPaintAvailable()
    {
        var scheduler = Collect(whitePaint: 5, hasBlackPaint: true, hasMonochromeTones: true);
        Assert.Contains(scheduler.InspectGcdQueue(), c => c.Behavior == IrisAbilities.CometInBlack);
    }

    [Fact]
    public void Holy_NotPushed_WhileMonochromeTonesUp()
    {
        // The button is morphed to Comet — pushing the raw Holy id just gets rejected every GCD.
        var scheduler = Collect(whitePaint: 5, hasBlackPaint: true, hasMonochromeTones: true);
        Assert.DoesNotContain(scheduler.InspectGcdQueue(), c => c.Behavior == IrisAbilities.HolyInWhite);
    }

    [Fact]
    public void Holy_Pushed_WithPooledPaintAndNoMonochrome()
    {
        var scheduler = Collect(whitePaint: 5, hasBlackPaint: false, hasMonochromeTones: false, paletteGauge: 100);
        Assert.Contains(scheduler.InspectGcdQueue(), c => c.Behavior == IrisAbilities.HolyInWhite);
    }

    [Fact]
    public void Comet_NotPushed_WithoutBlackPaint()
    {
        var scheduler = Collect(whitePaint: 5, hasBlackPaint: false, hasMonochromeTones: false);
        Assert.DoesNotContain(scheduler.InspectGcdQueue(), c => c.Behavior == IrisAbilities.CometInBlack);
    }

    private Daedalus.Rotation.Common.Scheduling.RotationScheduler Collect(
        int whitePaint,
        bool hasBlackPaint,
        bool hasMonochromeTones,
        int paletteGauge = 100)
    {
        var enemy = CreateMockEnemy();
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);
        targeting.Setup(x => x.FindEnemyForAction(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<uint>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);

        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = IrisTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            inCombat: true,
            paletteGauge: paletteGauge,
            whitePaint: whitePaint,
            hasWhitePaint: whitePaint > 0,
            hasBlackPaint: hasBlackPaint,
            hasMonochromeTones: hasMonochromeTones);

        _module.CollectCandidates(context, scheduler, isMoving: false);
        return scheduler;
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
