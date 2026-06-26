using Daedalus.Data;
using Daedalus.Rotation;
using Xunit;

namespace Daedalus.Tests.Rotation.CirceCore;

/// <summary>
/// Unit tests for Circe.ComputeMeleeComboStep and Circe.ComputeMoulinetStep.
/// </summary>
public class CirceGaugeTests
{
    private const float ActiveComboTimer = 5f;

    // ----- ComputeMeleeComboStep -----

    [Fact]
    public void ComputeMeleeComboStep_Returns_0_When_Idle()
    {
        var step = Circe.ComputeMeleeComboStep(0, 0, 0f);
        Assert.Equal(0, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Returns_0_When_ManaStacks_AtLeast_1_But_TimerExpired()
    {
        var step = Circe.ComputeMeleeComboStep(1, 0, 0f);
        Assert.Equal(0, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Returns_1_When_ManaStacks_AtLeast_1_And_TimerActive()
    {
        var step = Circe.ComputeMeleeComboStep(1, 0, ActiveComboTimer);
        Assert.Equal(1, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Returns_1_When_ComboAction_IsRiposte_And_TimerActive()
    {
        var step = Circe.ComputeMeleeComboStep(0, RDMActions.Riposte.ActionId, ActiveComboTimer);
        Assert.Equal(1, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Returns_0_When_ComboAction_IsZwerchhau_But_TimerExpired()
    {
        var step = Circe.ComputeMeleeComboStep(0, RDMActions.EnchantedZwerchhau.ActionId, 0f);
        Assert.Equal(0, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Returns_2_When_ManaStacks_AtLeast_2_And_TimerActive()
    {
        var step = Circe.ComputeMeleeComboStep(2, 0, ActiveComboTimer);
        Assert.Equal(2, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Returns_2_When_ComboAction_IsZwerchhau_And_TimerActive()
    {
        var step = Circe.ComputeMeleeComboStep(0, RDMActions.Zwerchhau.ActionId, ActiveComboTimer);
        Assert.Equal(2, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Returns_3_When_ManaStacks_AtLeast_3_And_TimerActive()
    {
        var step = Circe.ComputeMeleeComboStep(3, 0, ActiveComboTimer);
        Assert.Equal(3, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Returns_3_When_ComboAction_IsRedoublement_And_TimerActive()
    {
        var step = Circe.ComputeMeleeComboStep(0, RDMActions.Redoublement.ActionId, ActiveComboTimer);
        Assert.Equal(3, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Returns_4_When_Verflare_And_TimerActive()
    {
        var step = Circe.ComputeMeleeComboStep(0, RDMActions.Verflare.ActionId, ActiveComboTimer);
        Assert.Equal(4, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Returns_5_When_Scorch_And_TimerActive()
    {
        var step = Circe.ComputeMeleeComboStep(0, RDMActions.Scorch.ActionId, ActiveComboTimer);
        Assert.Equal(5, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Returns_0_When_TimerExpired_For_LateChain()
    {
        var step = Circe.ComputeMeleeComboStep(0, RDMActions.Scorch.ActionId, 0f);
        Assert.Equal(0, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Returns_0_When_ManaStacks_And_Stale_ComboAction_Without_Timer()
    {
        var step = Circe.ComputeMeleeComboStep(2, RDMActions.EnchantedRiposte.ActionId, 0f);
        Assert.Equal(0, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Uses_ManaStacks_When_TimerActive_And_ComboAction_Unrecognized()
    {
        var step = Circe.ComputeMeleeComboStep(2, 0, ActiveComboTimer);
        Assert.Equal(2, step);
    }

    [Fact]
    public void ComputeMeleeComboStep_Prefers_LateChain_Over_ManaStacks()
    {
        var step = Circe.ComputeMeleeComboStep(3, RDMActions.Verflare.ActionId, ActiveComboTimer);
        Assert.Equal(4, step);
    }

    // ----- ComputeMoulinetStep -----

    [Fact]
    public void ComputeMoulinetStep_Returns_0_When_NoReplacement()
    {
        var step = Circe.ComputeMoulinetStep(RDMActions.EnchantedMoulinet.ActionId);
        Assert.Equal(0, step);
    }

    [Fact]
    public void ComputeMoulinetStep_Returns_1_When_AdjustedToDeux()
    {
        var step = Circe.ComputeMoulinetStep(RDMActions.EnchantedMoulinetDeux.ActionId);
        Assert.Equal(1, step);
    }

    [Fact]
    public void ComputeMoulinetStep_Returns_2_When_AdjustedToTrois()
    {
        var step = Circe.ComputeMoulinetStep(RDMActions.EnchantedMoulinetTrois.ActionId);
        Assert.Equal(2, step);
    }
}
