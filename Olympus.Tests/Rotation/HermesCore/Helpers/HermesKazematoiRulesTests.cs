using Olympus.Rotation.HermesCore.Helpers;
using Olympus.Services.Positional;
using Xunit;

namespace Olympus.Tests.Rotation.HermesCore.Helpers;

public class HermesKazematoiRulesTests
{
    [Theory]
    [InlineData(0, true, false, true)]
    [InlineData(1, false, true, true)]
    [InlineData(3, false, true, true)]
    [InlineData(4, false, true, false)]
    [InlineData(5, false, true, false)]
    public void Rules_MatchRsr(int kazematoi, bool buildArmor, bool spendAeolian, bool fallbackArmor)
    {
        Assert.Equal(buildArmor, HermesKazematoiRules.ShouldBuildWithArmorCrush(kazematoi));
        Assert.Equal(spendAeolian, HermesKazematoiRules.ShouldSpendWithAeolian(kazematoi));
        Assert.Equal(fallbackArmor, HermesKazematoiRules.ShouldFallbackArmorCrush(kazematoi));
    }

    [Theory]
    [InlineData(0, PositionalType.Flank)]
    [InlineData(1, PositionalType.Rear)]
    [InlineData(4, PositionalType.Rear)]
    public void GetFinisherPositional_MatchesRsr(int kazematoi, PositionalType expected)
    {
        Assert.Equal(expected, HermesKazematoiRules.GetFinisherPositional(kazematoi));
    }

    [Fact]
    public void ArmorCrushFallbackThreshold_IsFour()
    {
        Assert.Equal(4, HermesKazematoiRules.ArmorCrushFallbackThreshold);
    }
}
