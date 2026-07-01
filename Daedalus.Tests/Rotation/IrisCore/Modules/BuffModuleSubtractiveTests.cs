using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Moq;
using Daedalus.Data;
using Daedalus.Rotation.IrisCore.Abilities;
using Daedalus.Rotation.IrisCore.Modules;
using Daedalus.Services.Targeting;
using Daedalus.Tests.Mocks;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Daedalus.Tests.Rotation.IrisCore;
using Xunit;

namespace Daedalus.Tests.Rotation.IrisCore.Modules;

/// <summary>
/// Regression guards for the Subtractive Palette press (the "subtractive never fired" bug, 2026-07-01).
/// The old gates were inverted: Spectrum RETURNED instead of pressing (Spectrum makes the press free —
/// RSR: PaletteGauge >= 50 || HasSubtractiveSpectrum), and a circular "SavePaletteForComet" hold waited
/// on black paint, which only exists AFTER the press. Net effect in-game: an entire fight with 75 gauge
/// and a free Spectrum proc and zero subtractive casts.
/// </summary>
public class BuffModuleSubtractiveTests
{
    private readonly BuffModule _module = new();

    [Fact]
    public void SubtractivePalette_Pushed_WhenSpectrumUp_EvenAtLowGauge()
    {
        // Spectrum (from Starry Muse) = free press. Old code returned when Spectrum was up.
        var scheduler = Collect(paletteGauge: 25, hasSubtractiveSpectrum: true);
        Assert.Contains(scheduler.InspectOgcdQueue(), c => c.Behavior == IrisAbilities.SubtractivePalette);
    }

    [Fact]
    public void SubtractivePalette_Pushed_At75GaugeOutsideBurst()
    {
        // The exact in-game failure: 75 gauge, no black paint, outside burst — the circular
        // "save for Comet" hold (waiting for black paint that only the press creates) blocked this forever.
        var scheduler = Collect(paletteGauge: 75);
        Assert.Contains(scheduler.InspectOgcdQueue(), c => c.Behavior == IrisAbilities.SubtractivePalette);
    }

    [Fact]
    public void SubtractivePalette_NotPushed_WhileMonochromeTonesUp()
    {
        // Black paint unspent — the game disallows the press until Comet in Black consumes it
        // (RSR ActionCheck: !HasMonochromeTones).
        var scheduler = Collect(paletteGauge: 100, hasMonochromeTones: true);
        Assert.DoesNotContain(scheduler.InspectOgcdQueue(), c => c.Behavior == IrisAbilities.SubtractivePalette);
    }

    [Fact]
    public void SubtractivePalette_Held_BelowPoolThresholdOutsideBurst_WithoutSpectrum()
    {
        // Pooling preserved: 50-74 gauge outside burst with no Spectrum still holds for the burst window.
        var scheduler = Collect(paletteGauge: 60);
        Assert.DoesNotContain(scheduler.InspectOgcdQueue(), c => c.Behavior == IrisAbilities.SubtractivePalette);
    }

    [Fact]
    public void SubtractivePalette_NotPushed_WhenStacksAlreadyActive()
    {
        var scheduler = Collect(paletteGauge: 100, hasSubtractivePalette: true);
        Assert.DoesNotContain(scheduler.InspectOgcdQueue(), c => c.Behavior == IrisAbilities.SubtractivePalette);
    }

    private Daedalus.Rotation.Common.Scheduling.RotationScheduler Collect(
        int paletteGauge,
        bool hasSubtractiveSpectrum = false,
        bool hasMonochromeTones = false,
        bool hasSubtractivePalette = false)
    {
        var enemy = CreateMockEnemy();
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);

        var actionService = MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = IrisTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            inCombat: true,
            canExecuteOgcd: true,
            paletteGauge: paletteGauge,
            canUseSubtractivePalette: paletteGauge >= 50 || hasSubtractiveSpectrum,
            subtractivePaletteReady: !hasSubtractivePalette,
            hasSubtractiveSpectrum: hasSubtractiveSpectrum,
            hasMonochromeTones: hasMonochromeTones,
            hasSubtractivePalette: hasSubtractivePalette);

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
