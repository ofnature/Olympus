using Moq;
using Daedalus.Data;
using Daedalus.Rotation;
using Daedalus.Rotation.ThemisCore.Helpers;
using Daedalus.Services.Action;
using Daedalus.Tests.Mocks;
using Xunit;
namespace Daedalus.Tests.Rotation.ThemisCore;

/// <summary>
/// Pure-switch tests for Themis.ComputeComboStep. Catches the regression class
/// where a future patch changes a step value, removes a case from the switch,
/// or breaks the comboTimer guard. Note: PLD's combo step values are "next
/// expected step" indices (FastBlade returns 2 because RiotBlade is step 2).
/// </summary>
public class ThemisComboStepTests
{
    [Fact]
    public void IsProminenceAvailable_WhenNotLearned_ReturnsFalse()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionLearned(PLDActions.Prominence.ActionId)).Returns(false);

        Assert.False(Themis.IsProminenceAvailable(actionService.Object, level: 80));
    }

    [Fact]
    public void IsProminenceAvailable_MidComboHotbarReplacement_ReturnsTrue()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.IsActionLearned(PLDActions.Prominence.ActionId)).Returns(false);
        actionService.Setup(x => x.GetAdjustedActionId(PLDActions.TotalEclipse.ActionId))
            .Returns(PLDActions.Prominence.ActionId);

        Assert.True(Themis.IsProminenceAvailable(actionService.Object, level: 80));
    }

    [Theory]
    [InlineData(7381u, 30f, true)]   // Total Eclipse
    [InlineData(7381u, 0f, false)]
    [InlineData(9u, 30f, false)]      // Fast Blade
    public void IsInAoECombo_DetectsUnfinishedAoEChain(uint lastComboAction, float comboTimer, bool expected)
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetAdjustedActionId(PLDActions.TotalEclipse.ActionId))
            .Returns(PLDActions.Prominence.ActionId);
        actionService.Setup(x => x.WasLastGcd(PLDActions.Prominence.ActionId)).Returns(false);

        Assert.Equal(expected, Themis.IsInAoECombo(actionService.Object, lastComboAction, comboTimer));
    }

    [Fact]
    public void IsInAoECombo_AfterProminenceLastGcd_ReturnsFalse()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetAdjustedActionId(PLDActions.TotalEclipse.ActionId))
            .Returns(PLDActions.Prominence.ActionId);
        actionService.Setup(x => x.WasLastGcd(PLDActions.Prominence.ActionId)).Returns(true);

        Assert.False(Themis.IsInAoECombo(
            actionService.Object,
            PLDActions.TotalEclipse.ActionId,
            comboTimeRemaining: 30f));
    }

    [Fact]
    public void IsInAoECombo_DoesNotRequireHotbarSubstitution()
    {
        // PLD AoE is NOT a button-replacement combo, so GetAdjustedActionId(TotalEclipse) stays Total
        // Eclipse in-game. Detection must rely on combo state alone — requiring the substitution was the
        // bug that made the rotation spam Total Eclipse and never fire Prominence.
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetAdjustedActionId(PLDActions.TotalEclipse.ActionId))
            .Returns(PLDActions.TotalEclipse.ActionId); // no substitution (mirrors the real game)
        actionService.Setup(x => x.WasLastGcd(PLDActions.Prominence.ActionId)).Returns(false);

        Assert.True(Themis.IsInAoECombo(
            actionService.Object,
            PLDActions.TotalEclipse.ActionId,
            comboTimeRemaining: 30f));
    }

    [Theory]
    [InlineData(9u, 30f, true)]      // Fast Blade
    [InlineData(15u, 30f, true)]      // Riot Blade
    [InlineData(7381u, 30f, false)]   // Total Eclipse
    [InlineData(9u, 0f, false)]
    public void IsInSingleTargetCombo_DetectsUnfinishedStChain(uint lastComboAction, float comboTimer, bool expected)
    {
        Assert.Equal(expected, Themis.IsInSingleTargetCombo(lastComboAction, comboTimer));
    }

    [Theory]
    [InlineData(0u, 30f, 0)]       // No combo
    [InlineData(9u, 0f, 0)]        // Fast Blade - timer expired
    [InlineData(9u, 30f, 2)]       // Fast Blade → step 2 (RiotBlade next)
    [InlineData(15u, 30f, 3)]      // Riot Blade → step 3 (RoyalAuthority next)
    [InlineData(7381u, 30f, 2)]    // Total Eclipse (AoE) → step 2 (Prominence next)
    [InlineData(99999u, 30f, 0)]   // Unknown action
    public void ComputeComboStep_MapsActionAndTimer(uint comboAction, float comboTimer, int expected)
    {
        Assert.Equal(expected, Themis.ComputeComboStep(comboAction, comboTimer));
    }

    [Theory]
    [InlineData(36918u, 2)]  // Supplication
    [InlineData(36919u, 3)]  // Sepulchre
    public void ComputeAtonementStep_UsesButtonReplacement(uint adjustedId, int expectedStep)
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetAdjustedActionId(PLDActions.Atonement.ActionId)).Returns(adjustedId);

        var player = MockBuilders.CreateMockPlayerCharacter(level: 100);
        player.Setup(x => x.StatusList).Returns((Dalamud.Game.ClientState.Statuses.StatusList?)null!);

        var step = Themis.ComputeAtonementStep(
            actionService.Object,
            new ThemisStatusHelper(),
            player.Object,
            100);

        Assert.Equal(expectedStep, step);
    }

    [Fact]
    public void ComputeAtonementStep_NoProcOrReplacement_ReturnsZero()
    {
        var actionService = MockBuilders.CreateMockActionService();
        actionService.Setup(x => x.GetAdjustedActionId(PLDActions.Atonement.ActionId))
            .Returns(PLDActions.Atonement.ActionId);

        var player = MockBuilders.CreateMockPlayerCharacter(level: 100);
        player.Setup(x => x.StatusList).Returns((Dalamud.Game.ClientState.Statuses.StatusList?)null!);

        var step = Themis.ComputeAtonementStep(
            actionService.Object,
            new ThemisStatusHelper(),
            player.Object,
            100);

        Assert.Equal(0, step);
    }
}
