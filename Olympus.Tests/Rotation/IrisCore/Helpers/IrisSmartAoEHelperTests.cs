using Moq;
using Olympus.Data;
using Olympus.Rotation.IrisCore.Helpers;
using Olympus.Services.Targeting;
using Olympus.Tests.Rotation.IrisCore;
using Xunit;

namespace Olympus.Tests.Rotation.IrisCore.Helpers;

public class IrisSmartAoEHelperTests
{
    [Fact]
    public void RefineEnemyCount_DoesNotPromoteSingleTargetToAoE()
    {
        var smart = new Mock<ISmartAoEService>();
        smart.Setup(x => x.FindBestAoETarget(
                PCTActions.Fire2InRed.ActionId,
                It.IsAny<float>(),
                It.IsAny<Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter>(),
                false))
            .Returns(new Olympus.Models.AoEResult(null, 5, 0f, Olympus.Models.AoEShape.Circle));

        var ctx = IrisTestContext.Create(level: 100, nearbyEnemyCount: 1);
        Assert.Equal(1, IrisSmartAoEHelper.RefineEnemyCountForAoE(smart.Object, ctx, rawEnemyCount: 1));
    }

    [Fact]
    public void RefineEnemyCount_RefinesWhenRawCountMeetsThreshold()
    {
        var smart = new Mock<ISmartAoEService>();
        smart.Setup(x => x.FindBestAoETarget(
                PCTActions.Fire2InRed.ActionId,
                It.IsAny<float>(),
                It.IsAny<Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter>(),
                false))
            .Returns(new Olympus.Models.AoEResult(null, 5, 0f, Olympus.Models.AoEShape.Circle));

        var ctx = IrisTestContext.Create(level: 100, nearbyEnemyCount: 3);
        Assert.Equal(5, IrisSmartAoEHelper.RefineEnemyCountForAoE(smart.Object, ctx, rawEnemyCount: 3));
    }

    [Fact]
    public void RefineEnemyCount_RefinesWhenRawCountOneBelowBreakeven()
    {
        var smart = new Mock<ISmartAoEService>();
        smart.Setup(x => x.FindBestAoETarget(
                PCTActions.Fire2InRed.ActionId,
                It.IsAny<float>(),
                It.IsAny<Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter>(),
                false))
            .Returns(new Olympus.Models.AoEResult(null, 3, 0f, Olympus.Models.AoEShape.Circle));

        var ctx = IrisTestContext.Create(level: 100, nearbyEnemyCount: 2);
        Assert.Equal(3, IrisSmartAoEHelper.RefineEnemyCountForAoE(smart.Object, ctx, rawEnemyCount: 2));
    }
}
