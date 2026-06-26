using Moq;
using Daedalus.Data;
using Daedalus.Rotation.HermesCore.Helpers;
using Daedalus.Services.Action;
using Xunit;

namespace Daedalus.Tests.Rotation.HermesCore.Helpers;

public sealed class TenMudraChargeTrackerTests
{
    [Fact]
    public void GetSnapshot_WithChargeAvailable_IsPressableImmediately()
    {
        var actionService = new Mock<IActionService>();
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId)).Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(1u);
        actionService.Setup(x => x.GetMaxCharges(NINActions.Ten.ActionId, It.IsAny<uint>())).Returns((ushort)2);
        actionService.Setup(x => x.GetCooldownRemaining(NINActions.Ten.ActionId)).Returns(0f);
        actionService.Setup(x => x.GcdRemaining).Returns(0f);
        actionService.Setup(x => x.GetRecastTimeElapsed(NINActions.Ten.ActionId)).Returns(0f);

        var snapshot = TenMudraChargeTracker.GetSnapshot(actionService.Object, 100);

        Assert.Equal(1u, snapshot.CurrentCharges);
        Assert.Equal(2, snapshot.MaxCharges);
        Assert.True(snapshot.IsPressable);
        Assert.Equal(0f, snapshot.SecondsUntilPressable);
    }

    [Fact]
    public void GetSnapshot_NoCharges_CountsDownToGcdWindow()
    {
        var actionService = new Mock<IActionService>();
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId)).Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(0u);
        actionService.Setup(x => x.GetMaxCharges(NINActions.Ten.ActionId, It.IsAny<uint>())).Returns((ushort)2);
        actionService.Setup(x => x.GetCooldownRemaining(NINActions.Ten.ActionId)).Returns(5f);
        actionService.Setup(x => x.GcdRemaining).Returns(2f);
        actionService.Setup(x => x.GetRecastTimeElapsed(NINActions.Ten.ActionId)).Returns(10f);

        var snapshot = TenMudraChargeTracker.GetSnapshot(actionService.Object, 100);

        Assert.False(snapshot.IsPressable);
        Assert.Equal(3f, snapshot.SecondsUntilPressable, precision: 2);
        Assert.Equal(5f, snapshot.NextChargeRemaining, precision: 2);
        Assert.Equal(15f, snapshot.ChargeRecastTotal, precision: 2);
    }

    [Fact]
    public void GetSnapshot_CooldownWithinGcd_IsPressable()
    {
        var actionService = new Mock<IActionService>();
        actionService.Setup(x => x.GetAdjustedActionId(NINActions.Ten.ActionId)).Returns(NINActions.Ten.ActionId);
        actionService.Setup(x => x.GetCurrentCharges(NINActions.Ten.ActionId)).Returns(0u);
        actionService.Setup(x => x.GetMaxCharges(NINActions.Ten.ActionId, It.IsAny<uint>())).Returns((ushort)2);
        actionService.Setup(x => x.GetCooldownRemaining(NINActions.Ten.ActionId)).Returns(0.3f);
        actionService.Setup(x => x.GcdRemaining).Returns(2f);
        actionService.Setup(x => x.GetRecastTimeElapsed(NINActions.Ten.ActionId)).Returns(14.7f);

        var snapshot = TenMudraChargeTracker.GetSnapshot(actionService.Object, 100);

        Assert.True(snapshot.IsPressable);
        Assert.Equal(0f, snapshot.SecondsUntilPressable);
    }

    [Fact]
    public void FormatWaitSummary_WhenNotPressable_IncludesChargeFraction()
    {
        var snapshot = new TenMudraChargeSnapshot
        {
            CurrentCharges = 0,
            MaxCharges = 2,
            NextChargeRemaining = 8f,
            SecondsUntilPressable = 6f,
            IsPressable = false,
        };

        var text = TenMudraChargeTracker.FormatWaitSummary(snapshot);

        Assert.Contains("0/2", text);
        Assert.Contains("6.0s", text);
    }
}
