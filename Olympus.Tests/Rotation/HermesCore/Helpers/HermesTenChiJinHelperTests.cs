using Olympus.Data;
using Olympus.Rotation.HermesCore.Helpers;
using Xunit;

namespace Olympus.Tests.Rotation.HermesCore.Helpers;

public class HermesTenChiJinHelperTests
{
    [Fact]
    public void Step1_TenAdjustedToFumaSt_ReturnsTenButton()
    {
        Assert.True(HermesTenChiJinHelper.TryGetNextStep(
            NINActions.TenChiJinAdjusted.FumaShurikenSt, 0, 0,
            enemyCount: 1, aoeMinTargets: 3, hasDotonActive: false,
            _ => false, out var step));

        Assert.Equal(NINActions.Ten.ActionId, step.MudraButtonActionId);
        Assert.Equal(NINActions.TenChiJinAdjusted.FumaShurikenSt, step.AdjustedNinjutsuId);
    }

    [Fact]
    public void Step1_AoeEnemyCount_PrefersFumaAoEId()
    {
        Assert.True(HermesTenChiJinHelper.TryGetNextStep(
            NINActions.TenChiJinAdjusted.FumaShurikenSt, 0, 0,
            enemyCount: 3, aoeMinTargets: 3, hasDotonActive: false,
            _ => false, out var step));

        Assert.Equal(NINActions.TenChiJinAdjusted.FumaShurikenAoE, step.AdjustedNinjutsuId);
    }

    [Fact]
    public void Step2_ChiAdjustedToRaiton_ReturnsChiButton()
    {
        Assert.True(HermesTenChiJinHelper.TryGetNextStep(
            0, NINActions.TenChiJinAdjusted.Raiton, 0,
            enemyCount: 1, aoeMinTargets: 3, hasDotonActive: false,
            _ => false, out var step));

        Assert.Equal(NINActions.Chi.ActionId, step.MudraButtonActionId);
        Assert.Equal("TCJ: Raiton", step.DebugName);
    }

    [Fact]
    public void Step3_JinAdjustedToSuiton_ReturnsJinButton()
    {
        Assert.True(HermesTenChiJinHelper.TryGetNextStep(
            0, 0, NINActions.TenChiJinAdjusted.Suiton,
            enemyCount: 1, aoeMinTargets: 3, hasDotonActive: false,
            _ => false, out var step));

        Assert.Equal(NINActions.Jin.ActionId, step.MudraButtonActionId);
        Assert.Equal("TCJ: Suiton", step.DebugName);
    }

    [Fact]
    public void Step3_ChiAdjustedToDoton_SkippedWhenDotonActive()
    {
        Assert.False(HermesTenChiJinHelper.TryGetNextStep(
            0, NINActions.TenChiJinAdjusted.Doton, 0,
            enemyCount: 3, aoeMinTargets: 3, hasDotonActive: true,
            _ => false, out _));
    }

    [Fact]
    public void WasLastAction_SuppressesRepeatStep()
    {
        Assert.False(HermesTenChiJinHelper.TryGetNextStep(
            NINActions.TenChiJinAdjusted.FumaShurikenSt, 0, 0,
            enemyCount: 1, aoeMinTargets: 3, hasDotonActive: false,
            id => id == NINActions.TenChiJinAdjusted.FumaShurikenSt, out _));
    }

    [Fact]
    public void FullSequence_ThreeSteps_WithoutActionGate()
    {
        uint lastAction = 0;

        Assert.True(HermesTenChiJinHelper.TryGetNextStep(
            NINActions.TenChiJinAdjusted.FumaShurikenSt, 0, 0,
            1, 3, false, id => id == lastAction, out var step1));
        lastAction = step1.AdjustedNinjutsuId;

        Assert.True(HermesTenChiJinHelper.TryGetNextStep(
            0, NINActions.TenChiJinAdjusted.Raiton, 0,
            1, 3, false, id => id == lastAction, out var step2));
        lastAction = step2.AdjustedNinjutsuId;

        Assert.True(HermesTenChiJinHelper.TryGetNextStep(
            0, 0, NINActions.TenChiJinAdjusted.Suiton,
            1, 3, false, id => id == lastAction, out var step3));

        Assert.Equal(NINActions.TenChiJinAdjusted.FumaShurikenSt, step1.AdjustedNinjutsuId);
        Assert.Equal(NINActions.TenChiJinAdjusted.Raiton, step2.AdjustedNinjutsuId);
        Assert.Equal(NINActions.TenChiJinAdjusted.Suiton, step3.AdjustedNinjutsuId);
    }

    [Fact]
    public void BuffDurationConstant_IsSixSeconds()
    {
        Assert.Equal(6f, HermesTenChiJinHelper.BuffDurationSeconds);
    }
}
