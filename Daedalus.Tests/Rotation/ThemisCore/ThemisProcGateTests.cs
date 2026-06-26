using Moq;
using Daedalus.Data;
using Daedalus.Services.Action;
using Daedalus.Tests.Mocks;
using Xunit;

namespace Daedalus.Tests.Rotation.ThemisCore;

/// <summary>
/// Regression guards for PLD proc-gated actions. Previously Blade of Honor and the
/// Confiteor blades inferred readiness from cooldown (IsActionReady), which is always
/// true for no-recast combo/proc actions and caused per-frame requeue spam.
/// Correct source is button replacement via GetAdjustedActionId (RSR parity).
/// </summary>
public class ThemisProcGateTests
{
    private static Mock<IActionService> WithAdjust(uint baseId, uint adjustedId)
    {
        var mock = MockBuilders.CreateMockActionService();
        mock.Setup(x => x.GetAdjustedActionId(baseId)).Returns(adjustedId);
        return mock;
    }

    [Fact]
    public void HasBladeOfHonor_True_WhenImperatorReplacedByBladeOfHonor()
    {
        var svc = WithAdjust(PLDActions.Imperator.ActionId, PLDActions.BladeOfHonor.ActionId);
        var ctx = ThemisTestContext.Create(actionService: svc, level: 100);
        Assert.True(ctx.HasBladeOfHonor);
    }

    [Fact]
    public void HasBladeOfHonor_False_WhenProcInactive()
    {
        // Default identity mapping: Imperator stays Imperator => no proc.
        var ctx = ThemisTestContext.Create(level: 100);
        Assert.False(ctx.HasBladeOfHonor);
    }

    [Theory]
    [InlineData(25748u, 2)] // Confiteor -> Blade of Faith
    [InlineData(25749u, 3)] // Confiteor -> Blade of Truth
    [InlineData(25750u, 4)] // Confiteor -> Blade of Valor
    public void ConfiteorStep_FollowsButtonReplacement(uint adjustedConfiteorId, int expectedStep)
    {
        var svc = WithAdjust(PLDActions.Confiteor.ActionId, adjustedConfiteorId);
        var ctx = ThemisTestContext.Create(actionService: svc, level: 100);
        Assert.Equal(expectedStep, ctx.ConfiteorStep);
    }

    [Fact]
    public void ConfiteorStep_Zero_WhenNotInChain()
    {
        // Identity mapping + no Requiescat buff => not in chain.
        var ctx = ThemisTestContext.Create(level: 100);
        Assert.Equal(0, ctx.ConfiteorStep);
    }
}
