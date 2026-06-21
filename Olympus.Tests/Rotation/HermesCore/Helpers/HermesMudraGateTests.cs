using Moq;
using Olympus.Data;
using Olympus.Rotation.HermesCore.Helpers;
using Olympus.Tests.Mocks;
using Olympus.Tests.Rotation.HermesCore;
using Xunit;

namespace Olympus.Tests.Rotation.HermesCore.Helpers;

public class HermesMudraGateTests
{
    [Theory]
    [InlineData(false, 0f, false)]
    [InlineData(false, 1f, false)]
    [InlineData(true, 0.624f, false)]
    [InlineData(true, 0.625f, true)]
    [InlineData(true, 1.5f, true)]
    public void ShouldBlockComboGcds_MatchesRsr496Gate(bool hasGameMudraStatus, float gcdRemaining, bool expected)
    {
        Assert.Equal(expected, HermesMudraGate.ShouldBlockComboGcds(hasGameMudraStatus, gcdRemaining));
    }

    [Theory]
    [InlineData(false, false, 0f, false)]
    [InlineData(false, true, 0.625f, false)]
    [InlineData(true, false, 0f, true)]
    [InlineData(true, true, 0.625f, true)]
    public void ShouldBlockComboGcds_WithContext_OnlyDuringActiveSequence(
        bool isSequenceActive, bool hasGameMudraStatus, float gcdRemaining, bool expected)
    {
        var helper = new MudraHelper();
        if (isSequenceActive)
            helper.StartSequence(NINActions.NinjutsuType.Suiton);

        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GcdRemaining).Returns(gcdRemaining);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId))
            .Returns(NINActions.Ninjutsu.ActionId);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId))
            .Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(1u);

        var context = HermesTestContext.Create(
            actionService: actionService,
            mudraHelper: helper,
            hasGameMudraStatus: hasGameMudraStatus,
            canExecuteGcd: true);

        Assert.Equal(expected, HermesMudraGate.ShouldBlockComboGcds(context));
    }

    [Fact]
    public void ShouldBlockComboGcds_QueuedSequenceWithTenOnCd_AllowsCombo()
    {
        var helper = new MudraHelper();
        helper.StartSequence(NINActions.NinjutsuType.Raiton);

        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId))
            .Returns(NINActions.Ninjutsu.ActionId);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId))
            .Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(0u);
        actionService.Setup(x => x.GetCooldownRemaining(NINActions.Ten.ActionId)).Returns(5f);
        actionService.Setup(x => x.GcdRemaining).Returns(0f);

        var context = HermesTestContext.Create(
            actionService: actionService,
            mudraHelper: helper,
            hasGameMudraStatus: false,
            canExecuteGcd: true);

        Assert.False(HermesMudraGate.ShouldBlockComboGcds(context));
    }

    [Fact]
    public void ShouldBlockComboGcds_MidSequenceWithTenOnCd_BlocksCombo()
    {
        var helper = new MudraHelper();
        helper.StartSequence(NINActions.NinjutsuType.Suiton);
        helper.NotifyMudraPressed();
        helper.NotifyMudraPressed();

        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId))
            .Returns(NINActions.Ninjutsu.ActionId);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId))
            .Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(0u);
        actionService.Setup(x => x.GetCooldownRemaining(NINActions.Ten.ActionId)).Returns(20f);
        actionService.Setup(x => x.GcdRemaining).Returns(0f);

        var context = HermesTestContext.Create(
            actionService: actionService,
            mudraHelper: helper,
            hasGameMudraStatus: false,
            canExecuteGcd: true);

        Assert.True(HermesMudraGate.ShouldBlockComboGcds(context));
    }

    [Fact]
    public void ShouldBlockComboGcds_PendingNinjutsuFinishWithMudraCount_BlocksCombo()
    {
        var helper = new MudraHelper();
        helper.StartSequence(NINActions.NinjutsuType.Suiton);
        helper.NotifyMudraPressed();
        helper.NotifyMudraPressed();
        helper.NotifyMudraPressed();

        var actionService = MockBuilders.CreateMockActionService(canExecuteGcd: true);
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ninjutsu.ActionId))
            .Returns(NINActions.Suiton.ActionId);
        actionService.Setup(x => x.CanExecuteActionId(NINActions.Ninjutsu.ActionId)).Returns(false);
        actionService.Setup(x => x.GcdRemaining).Returns(0f);

        var context = HermesTestContext.Create(
            actionService: actionService,
            mudraHelper: helper,
            hasGameMudraStatus: false,
            canExecuteGcd: true);

        Assert.True(HermesMudraGate.ShouldBlockComboGcds(context));
    }
}
