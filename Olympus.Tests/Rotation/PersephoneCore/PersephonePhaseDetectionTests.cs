using Moq;
using Olympus.Data;
using Olympus.Services.Action;
using Olympus.Tests.Mocks;
using Xunit;

namespace Olympus.Tests.Rotation.PersephoneCore;

/// <summary>
/// Regression guards for SMN demi phase detection via Astral Flow button replacement (RSR parity).
/// </summary>
public class PersephonePhaseDetectionTests
{
    private static Mock<IActionService> WithAstralFlowAdjust(uint adjustedId)
    {
        var mock = MockBuilders.CreateMockActionService();
        mock.Setup(x => x.GetAdjustedActionId(SMNActions.AstralFlow.ActionId)).Returns(adjustedId);
        return mock;
    }

    [Fact]
    public void IsBahamutPhase_True_WhenAstralFlowReplacedByDeathflare()
    {
        var svc = WithAstralFlowAdjust(SMNActions.Deathflare.ActionId);
        Assert.True(SMNActions.IsBahamutPhase(svc.Object));
    }

    [Fact]
    public void IsPhoenixPhase_True_WhenAstralFlowReplacedByRekindle()
    {
        var svc = WithAstralFlowAdjust(SMNActions.Rekindle.ActionId);
        Assert.True(SMNActions.IsPhoenixPhase(svc.Object));
    }

    [Fact]
    public void IsSolarBahamutPhase_True_WhenAstralFlowReplacedBySunflare()
    {
        var svc = WithAstralFlowAdjust(SMNActions.Sunflare.ActionId);
        Assert.True(SMNActions.IsSolarBahamutPhase(svc.Object));
    }

    [Fact]
    public void IsMountainBusterReady_True_WhenAstralFlowReplacedByMountainBuster()
    {
        var svc = WithAstralFlowAdjust(SMNActions.MountainBuster.ActionId);
        Assert.True(SMNActions.IsMountainBusterReady(svc.Object));
    }

    [Fact]
    public void ResolveDemiPhase_SetsExactlyOneFlag()
    {
        var svc = WithAstralFlowAdjust(SMNActions.Deathflare.ActionId);
        SMNActions.ResolveDemiPhase(svc.Object, out var bahamut, out var phoenix, out var solar);
        Assert.True(bahamut);
        Assert.False(phoenix);
        Assert.False(solar);
    }

    [Fact]
    public void ResolveDemiPhase_AllFalse_WhenProcInactive()
    {
        var svc = WithAstralFlowAdjust(SMNActions.AstralFlow.ActionId);
        SMNActions.ResolveDemiPhase(svc.Object, out var bahamut, out var phoenix, out var solar);
        Assert.False(bahamut);
        Assert.False(phoenix);
        Assert.False(solar);
    }
}
