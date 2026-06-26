using Dalamud.Game.ClientState.Objects.SubKinds;
using Moq;
using Daedalus.Data;
using Daedalus.Rotation.HermesCore.Context;
using Daedalus.Rotation.HermesCore.Helpers;
using Daedalus.Services;
using Daedalus.Tests.Mocks;
using Daedalus.Tests.Rotation.HermesCore;
using Xunit;

namespace Daedalus.Tests.Rotation.HermesCore.Helpers;

public class HermesNinjutsuMudraExecutorTests
{
    [Theory]
    [InlineData(2260u, "Ten")]
    [InlineData(2265u, "Jin")]
    [InlineData(2268u, "Chi")]
    [InlineData(2270u, "Doton")]
    public void ResolveStep_DotonPath_MatchesRsrSlotMapping(uint slotId, string expectedStep)
    {
        var stepName = HermesNinjutsuMudraExecutor.GetExpectedStepName(
            NINActions.NinjutsuType.Doton, slotId, level: 100, hasKassatsu: false);

        Assert.Equal(expectedStep, stepName);
    }

    [Theory]
    [InlineData(2260u, "Ten")]
    [InlineData(2265u, "Chi")]
    [InlineData(2267u, "Jin")]
    [InlineData(2271u, "Suiton")]
    public void ResolveStep_SuitonPath_MatchesRsrSlotMapping(uint slotId, string expectedStep)
    {
        var stepName = HermesNinjutsuMudraExecutor.GetExpectedStepName(
            NINActions.NinjutsuType.Suiton, slotId, level: 100, hasKassatsu: false);

        Assert.Equal(expectedStep, stepName);
    }

    [Fact]
    public void TryExecuteStep_WhenGcdRolling_ReturnsWaitingForGcd()
    {
        var context = CreateContext(canExecuteGcd: false, slotId: NINActions.Ninjutsu.ActionId);

        var result = HermesNinjutsuMudraExecutor.TryExecuteStep(
            context, target: null, NINActions.NinjutsuType.Suiton, out var debugState, out _);

        Assert.Equal(HermesMudraStepResult.WaitingForGcd, result);
        Assert.Contains("WaitingForGcd", debugState);
    }

    [Fact]
    public void TryExecuteStep_WhenWasLastTen_BlocksRepress()
    {
        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId))
            .Returns(NINActions.Ninjutsu.ActionId);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId))
            .Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(1u);
        actionService.Setup(x => x.WasLastAction(NINActions.Ten.ActionId)).Returns(true);

        var context = HermesTestContext.Create(actionService: actionService, inCombat: true);

        var result = HermesNinjutsuMudraExecutor.TryExecuteStep(
            context, target: null, NINActions.NinjutsuType.Suiton, out var debugState, out _);

        Assert.Equal(HermesMudraStepResult.WaitingForAcknowledge, result);
        Assert.Contains("acknowledge", debugState);
    }

    [Fact]
    public void TryExecuteStep_RabbitSlot_ReturnsAbortRabbit()
    {
        var context = CreateContext(canExecuteGcd: true, slotId: NINActions.RabbitMedium.ActionId);

        var result = HermesNinjutsuMudraExecutor.TryExecuteStep(
            context, target: null, NINActions.NinjutsuType.Suiton, out var debugState, out _);

        Assert.Equal(HermesMudraStepResult.AbortRabbit, result);
        Assert.Contains("Rabbit", debugState);
    }

    [Fact]
    public void TryExecuteStep_RabbitSlot_WhenGcdRolling_StillReturnsAbortRabbit()
    {
        var context = CreateContext(canExecuteGcd: false, slotId: NINActions.RabbitMedium.ActionId);

        var result = HermesNinjutsuMudraExecutor.TryExecuteStep(
            context, target: null, NINActions.NinjutsuType.Suiton, out var debugState, out _);

        Assert.Equal(HermesMudraStepResult.AbortRabbit, result);
        Assert.Contains("Rabbit", debugState);
    }

    [Fact]
    public void IsWaitingForSlotAcknowledge_WhenWasLastTen_ReturnsTrue()
    {
        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId))
            .Returns(NINActions.Ninjutsu.ActionId);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId))
            .Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.WasLastAction(NINActions.Ten.ActionId)).Returns(true);

        var context = HermesTestContext.Create(actionService: actionService, inCombat: true);

        Assert.True(HermesNinjutsuMudraExecutor.IsWaitingForSlotAcknowledge(
            context, NINActions.NinjutsuType.Suiton));
    }

    [Fact]
    public void WillConsumeOpenGcdForMudraStep_WhenTenReady_ReturnsTrue()
    {
        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId))
            .Returns(NINActions.Ninjutsu.ActionId);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId))
            .Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(1u);

        var context = HermesTestContext.Create(actionService: actionService, inCombat: true);

        Assert.True(HermesNinjutsuMudraExecutor.WillConsumeOpenGcdForMudraStep(
            context, NINActions.NinjutsuType.Raiton));
    }

    [Fact]
    public void WillConsumeOpenGcdForMudraStep_WhenTenOnCd_ReturnsFalse()
    {
        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId))
            .Returns(NINActions.Ninjutsu.ActionId);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId))
            .Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(0u);
        actionService.Setup(x => x.GetCooldownRemaining(NINActions.Ten.ActionId)).Returns(5f);
        actionService.Setup(x => x.GcdRemaining).Returns(0f);

        var context = HermesTestContext.Create(actionService: actionService, inCombat: true);

        Assert.False(HermesNinjutsuMudraExecutor.WillConsumeOpenGcdForMudraStep(
            context, NINActions.NinjutsuType.Raiton));
    }

    [Fact]
    public void IsStepBlockedOnOpenGcd_WhenTenNotPressable_ReturnsTrue()
    {
        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId))
            .Returns(NINActions.Ninjutsu.ActionId);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId))
            .Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(0u);
        actionService.Setup(x => x.GetCooldownRemaining(NINActions.Ten.ActionId)).Returns(5f);
        actionService.Setup(x => x.GcdRemaining).Returns(0f);

        var context = HermesTestContext.Create(actionService: actionService, inCombat: true);
        var helper = new MudraHelper();
        helper.StartSequence(NINActions.NinjutsuType.Suiton);
        Mock.Get(context).Setup(x => x.MudraHelper).Returns(helper);

        Assert.True(HermesNinjutsuMudraExecutor.IsStepBlockedOnOpenGcd(context, NINActions.NinjutsuType.Suiton));
    }

    [Fact]
    public void IsFirstMudraBlockedOnCharge_OnlyWhenSequenceNotStarted()
    {
        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId))
            .Returns(NINActions.Ninjutsu.ActionId);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId))
            .Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(0u);
        actionService.Setup(x => x.GetCooldownRemaining(NINActions.Ten.ActionId)).Returns(5f);
        actionService.Setup(x => x.GcdRemaining).Returns(0f);

        var helper = new MudraHelper();
        helper.StartSequence(NINActions.NinjutsuType.Raiton);

        var context = HermesTestContext.Create(
            actionService: actionService,
            mudraHelper: helper,
            inCombat: true);

        Assert.True(HermesNinjutsuMudraExecutor.IsFirstMudraBlockedOnCharge(
            context, NINActions.NinjutsuType.Raiton));

        helper.NotifyMudraPressed();
        Assert.False(HermesNinjutsuMudraExecutor.IsFirstMudraBlockedOnCharge(
            context, NINActions.NinjutsuType.Raiton));
    }

    [Fact]
    public void IsFirstMudraBlockedOnCharge_NotWhenWaitingOnNinjutsuFinish()
    {
        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId))
            .Returns(NINActions.Suiton.ActionId);
        actionService.Setup(x => x.CanExecuteActionId(NINActions.Ninjutsu.ActionId)).Returns(false);
        actionService.Setup(x => x.CanExecuteActionId(NINActions.Suiton.ActionId)).Returns(false);
        actionService.Setup(x => x.GcdRemaining).Returns(0f);

        var helper = new MudraHelper();
        helper.StartSequence(NINActions.NinjutsuType.Suiton);

        var context = HermesTestContext.Create(
            actionService: actionService,
            mudraHelper: helper,
            inCombat: true);

        Assert.True(HermesNinjutsuMudraExecutor.IsStepBlockedOnOpenGcd(
            context, NINActions.NinjutsuType.Suiton));
        Assert.False(HermesNinjutsuMudraExecutor.IsFirstMudraBlockedOnCharge(
            context, NINActions.NinjutsuType.Suiton));
    }

    [Fact]
    public void IsStepBlockedOnOpenGcd_AllowsPressWithChargesDespiteStrictStatus()
    {
        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId))
            .Returns(NINActions.Ninjutsu.ActionId);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId))
            .Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(1u);
        actionService.Setup(x => x.CanExecuteActionId(NINActions.Ten.ActionId)).Returns(false);
        actionService.Setup(x => x.GetCooldownRemaining(NINActions.Ten.ActionId)).Returns(15f);

        var context = HermesTestContext.Create(actionService: actionService, inCombat: true);
        var helper = new MudraHelper();
        helper.StartSequence(NINActions.NinjutsuType.Suiton);
        Mock.Get(context).Setup(x => x.MudraHelper).Returns(helper);

        Assert.False(HermesNinjutsuMudraExecutor.IsStepBlockedOnOpenGcd(context, NINActions.NinjutsuType.Suiton));
    }

    [Fact]
    public void CanPressMudraUsedUp_AllowsWeaveWhenCdClearsWithinGcd()
    {
        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(0u);
        actionService.Setup(x => x.GetCooldownRemaining(NINActions.Ten.ActionId)).Returns(0.3f);
        actionService.Setup(x => x.GcdRemaining).Returns(0.5f);

        var context = HermesTestContext.Create(actionService: actionService, inCombat: true);

        Assert.True(HermesNinjutsuMudraExecutor.CanPressMudraUsedUp(context, NINActions.Ten.ActionId));
    }

    private static IHermesContext CreateContext(bool canExecuteGcd, uint slotId)
    {
        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: canExecuteGcd);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId)).Returns(slotId);

        return HermesTestContext.Create(actionService: actionService, inCombat: true, canExecuteGcd: canExecuteGcd);
    }
}
