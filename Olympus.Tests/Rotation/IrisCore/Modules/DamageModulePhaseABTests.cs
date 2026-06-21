using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Moq;
using Olympus.Config.DPS;
using Olympus.Data;
using Olympus.Rotation.IrisCore.Abilities;
using Olympus.Rotation.IrisCore.Modules;
using Olympus.Services;
using Olympus.Services.Targeting;
using Olympus.Tests.Mocks;
using Olympus.Tests.Rotation.Common.Scheduling;
using Olympus.Tests.Rotation.IrisCore;
using Xunit;

namespace Olympus.Tests.Rotation.IrisCore.Modules;

/// <summary>
/// Phase A/B regression guards: AoE toggle, subtractive route, motif sub-toggles, hammer stack gate.
/// </summary>
public class DamageModulePhaseABTests
{
    private readonly DamageModule _module = new();

    [Theory]
    [InlineData(5, true, true)]
    [InlineData(5, false, false)]
    [InlineData(2, true, false)]
    public void ShouldUseAoe_RespectsToggleAndTargetCount(int enemyCount, bool aoeEnabled, bool expected)
    {
        Assert.Equal(expected, PCTActions.ShouldUseAoe(enemyCount, level: 100, minTargets: 3, aoeEnabled));
    }

    [Fact]
    public void BaseCombo_Pushed_WhenNotInSubtractiveRoute()
    {
        var enemy = CreateMockEnemy();
        var targeting = BuildTargeting(enemy);
        var actionService = MockBuilders.CreateMockActionService();

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = IrisTestContext.Create(
            config: Config(),
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            inCombat: true);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == IrisAbilities.FireInRed);
        Assert.DoesNotContain(gcd, c => c.Behavior == IrisAbilities.BlizzardInCyan);
    }

    [Fact]
    public void BaseCombo_NotPushed_WhenSubtractivePaletteActive()
    {
        var enemy = CreateMockEnemy();
        var targeting = BuildTargeting(enemy);
        var actionService = MockBuilders.CreateMockActionService();

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = IrisTestContext.Create(
            config: Config(),
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            inCombat: true,
            hasSubtractivePalette: true);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == IrisAbilities.BlizzardInCyan);
        Assert.DoesNotContain(gcd, c => c.Behavior == IrisAbilities.FireInRed);
    }

    [Fact]
    public void PrepaintHammerMotif_NotPushed_WhenHammerMotifDisabled()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetAdjustedActionId(PCTActions.WeaponMotif.ActionId))
            .Returns(PCTActions.HammerMotif.ActionId);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = IrisTestContext.Create(
            config: Config(c =>
            {
                c.PrepaintOption = PrepaintOption.WeaponOnly;
                c.EnableHammerMotif = false;
            }),
            actionService: actionService,
            level: 100,
            inCombat: false,
            needsWeaponMotif: true,
            needsCreatureMotif: false,
            needsLandscapeMotif: false);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.DoesNotContain(gcd, c => c.Behavior == IrisAbilities.HammerMotif);
    }

    [Fact]
    public void PrepaintHammerMotif_Pushed_WhenEnabledAndProbeReady()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetAdjustedActionId(PCTActions.WeaponMotif.ActionId))
            .Returns(PCTActions.HammerMotif.ActionId);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = IrisTestContext.Create(
            config: Config(c => c.PrepaintOption = PrepaintOption.WeaponOnly),
            actionService: actionService,
            level: 100,
            inCombat: false,
            needsWeaponMotif: true,
            needsCreatureMotif: false,
            needsLandscapeMotif: false);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == IrisAbilities.HammerMotif);
    }

    [Fact]
    public void PrepaintStarrySkyMotif_NotPushed_WhenStarrySkyMotifDisabled()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetAdjustedActionId(PCTActions.LandscapeMotif.ActionId))
            .Returns(PCTActions.StarrySkyMotif.ActionId);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = IrisTestContext.Create(
            config: Config(c =>
            {
                c.PrepaintOption = PrepaintOption.LandscapeOnly;
                c.EnableStarrySkyMotif = false;
            }),
            actionService: actionService,
            level: 100,
            inCombat: false,
            needsLandscapeMotif: true,
            needsCreatureMotif: false,
            needsWeaponMotif: false);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.DoesNotContain(gcd, c => c.Behavior == IrisAbilities.StarrySkyMotif);
    }

    [Fact]
    public void HammerCombo_NotPushed_WhenHammerTimeStacksBelowThree()
    {
        var enemy = CreateMockEnemy();
        var targeting = BuildTargeting(enemy);
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetAdjustedActionId(PCTActions.HammerStamp.ActionId))
            .Returns(PCTActions.HammerStamp.ActionId);
        actionService.Setup(x => x.GetCooldownRemaining(PCTActions.StarryMuse.ActionId))
            .Returns(120f);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = IrisTestContext.Create(
            config: Config(),
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            inCombat: true,
            hasHammerTime: true,
            hammerTimeStacks: 2,
            hammerComboStep: 0);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.DoesNotContain(gcd, c => c.Behavior == IrisAbilities.HammerStamp);
    }

    [Fact]
    public void HammerCombo_Pushed_WhenThreeStacksAndStampProbeReady()
    {
        var enemy = CreateMockEnemy();
        var targeting = BuildTargeting(enemy);
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetAdjustedActionId(PCTActions.HammerStamp.ActionId))
            .Returns(PCTActions.HammerStamp.ActionId);
        actionService.Setup(x => x.GetCooldownRemaining(PCTActions.StarryMuse.ActionId))
            .Returns(120f);

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = IrisTestContext.Create(
            config: Config(),
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            inCombat: true,
            hasHammerTime: true,
            hammerTimeStacks: 3,
            hammerComboStep: 0);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == IrisAbilities.HammerStamp);
    }

    [Fact]
    public void BaseCombo_UsesAoeVariant_WhenShouldUseAoeTrue()
    {
        var enemy = CreateMockEnemy();
        var targeting = BuildTargeting(enemy);
        var actionService = MockBuilders.CreateMockActionService();

        var scheduler = SchedulerFactory.CreateForTest(actionService: actionService);
        var context = IrisTestContext.Create(
            config: Config(),
            actionService: actionService,
            targetingService: targeting,
            level: 100,
            inCombat: true,
            shouldUseAoe: true);

        _module.CollectCandidates(context, scheduler, isMoving: false);

        var gcd = scheduler.InspectGcdQueue();
        Assert.Contains(gcd, c => c.Behavior == IrisAbilities.Fire2InRed);
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
