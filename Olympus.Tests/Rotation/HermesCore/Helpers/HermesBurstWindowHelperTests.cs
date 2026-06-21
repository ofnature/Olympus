using Moq;
using Olympus.Data;
using Olympus.Rotation.HermesCore.Helpers;
using Olympus.Services.Action;
using Olympus.Tests.Mocks;
using Xunit;

namespace Olympus.Tests.Rotation.HermesCore.Helpers;

public class HermesBurstWindowHelperTests
{
    [Theory]
    [InlineData(5f, true)]
    [InlineData(0f, false)]
    [InlineData(16.9f, true)]
    [InlineData(17f, false)]
    [InlineData(18.5f, false)]
    public void IsWithinWindow_ElapsedThreshold(float elapsed, bool expected)
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetRecastTimeElapsed(NINActions.TrickAttack.ActionId)).Returns(elapsed);

        var result = HermesBurstWindowHelper.IsWithinWindow(
            actionService.Object,
            NINActions.TrickAttack.ActionId,
            HermesBurstWindowHelper.TrickAttackWindowSeconds);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsInMugWindow_TrueWhenDokumoriRecentlyUsed()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetRecastTimeElapsed(NINActions.Dokumori.ActionId)).Returns(8f);

        Assert.True(HermesBurstWindowHelper.IsInMugWindow(actionService.Object, level: 100));
    }

    [Fact]
    public void IsInMugWindow_FalseWhenElapsedBeyondWindow()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetRecastTimeElapsed(NINActions.Dokumori.ActionId)).Returns(25f);

        Assert.False(HermesBurstWindowHelper.IsInMugWindow(actionService.Object, level: 100));
    }

    [Fact]
    public void IsInTrickAttackWindow_TrueWhenKunaisBaneRecentlyUsed()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetRecastTimeElapsed(NINActions.KunaisBane.ActionId)).Returns(10f);

        Assert.True(HermesBurstWindowHelper.IsInTrickAttackWindow(actionService.Object, level: 100));
    }

    [Fact]
    public void IsInTrickAttackWindow_TrueWhenTrickAttackRecentlyUsedBelow92()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetRecastTimeElapsed(NINActions.TrickAttack.ActionId)).Returns(12f);

        Assert.True(HermesBurstWindowHelper.IsInTrickAttackWindow(actionService.Object, level: 90));
    }

    [Fact]
    public void IsInTrickAttackWindow_FalseWhenElapsedBeyondWindow()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetRecastTimeElapsed(NINActions.KunaisBane.ActionId)).Returns(20f);

        Assert.False(HermesBurstWindowHelper.IsInTrickAttackWindow(actionService.Object, level: 100));
    }
}
