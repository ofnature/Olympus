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
/// Regression guards for the subtractive route gating and the motif repaint starvation (2026-07-01).
/// Spectrum alone does NOT morph the combo buttons — routing on it pushed un-morphed Blizzard ids while
/// suppressing the base combo (a stall window during Starry). And the repaint path's hard IsCasting
/// return starved motif painting during continuous casting, so the weapon canvas was never painted and
/// the whole Hammer chain sat dormant.
/// </summary>
public class DamageModuleSubtractiveRouteTests
{
    private readonly DamageModule _module = new();

    [Fact]
    public void BaseCombo_StillPushed_WhenOnlySpectrumUp()
    {
        // Spectrum-but-no-stacks must NOT flip the route: the buttons haven't morphed yet.
        var scheduler = Collect(ctxMutator: null, hasSubtractiveSpectrum: true);
        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == IrisAbilities.FireInRed);
        Assert.DoesNotContain(gcd, c => c.Behavior == IrisAbilities.BlizzardInCyan);
    }

    [Fact]
    public void HammerMotif_Pushed_DuringSlidecastWindow_WhenSteelMuseReady()
    {
        // The starvation case: continuously casting (the caster norm) with the weapon canvas empty and
        // Steel Muse ready. The old hard `IsCasting` return meant this NEVER pushed mid-chain.
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(PCTActions.SteelMuse.ActionId)).Returns(true);
        actionService.Setup(x => x.GetAdjustedActionId(PCTActions.WeaponMotif.ActionId))
            .Returns(PCTActions.HammerMotif.ActionId);

        var scheduler = Collect(
            actionService: actionService,
            isCasting: true,
            canSlidecast: true,
            needsWeaponMotif: true);

        Assert.Contains(scheduler.InspectGcdQueue(), c => c.Behavior == IrisAbilities.HammerMotif);
    }

    [Fact]
    public void HammerMotif_NotPushed_MidCastOutsideSlidecastWindow()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(PCTActions.SteelMuse.ActionId)).Returns(true);

        var scheduler = Collect(
            actionService: actionService,
            isCasting: true,
            canSlidecast: false,
            needsWeaponMotif: true);

        Assert.DoesNotContain(scheduler.InspectGcdQueue(), c => c.Behavior == IrisAbilities.HammerMotif);
    }

    [Fact]
    public void HammerMotif_NotRePushed_WhenItWasTheLastGcd()
    {
        // The canvas gauge only fills at cast END — while the motif itself is casting it still reads
        // "needed". Without the WasLastGcd guard the motif gets queued twice (the double Wing Motif
        // seen in the 2026-07-01 PCT log).
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionReady(PCTActions.SteelMuse.ActionId)).Returns(true);
        actionService.Setup(x => x.GetAdjustedActionId(PCTActions.WeaponMotif.ActionId))
            .Returns(PCTActions.HammerMotif.ActionId);
        actionService.Setup(x => x.WasLastGcd(PCTActions.HammerMotif.ActionId)).Returns(true);

        var scheduler = Collect(
            actionService: actionService,
            isCasting: true,
            canSlidecast: true,
            needsWeaponMotif: true);

        Assert.DoesNotContain(scheduler.InspectGcdQueue(), c => c.Behavior == IrisAbilities.HammerMotif);
    }

    private Daedalus.Rotation.Common.Scheduling.RotationScheduler Collect(
        object? ctxMutator = null,
        Mock<Daedalus.Services.Action.IActionService>? actionService = null,
        bool hasSubtractiveSpectrum = false,
        bool isCasting = false,
        bool canSlidecast = false,
        bool needsWeaponMotif = false)
    {
        var enemy = CreateMockEnemy();
        var targeting = MockBuilders.CreateMockTargetingService();
        targeting.Setup(x => x.FindEnemy(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<float>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);
        targeting.Setup(x => x.FindEnemyForAction(
                It.IsAny<EnemyTargetingStrategy>(), It.IsAny<uint>(), It.IsAny<IPlayerCharacter>()))
            .Returns(enemy.Object);

        actionService ??= MockBuilders.CreateMockActionService();
        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = IrisTestContext.Create(
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            inCombat: true,
            isCasting: isCasting,
            canSlidecast: canSlidecast,
            hasSubtractiveSpectrum: hasSubtractiveSpectrum,
            needsWeaponMotif: needsWeaponMotif);

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
