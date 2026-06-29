using Daedalus.Rotation.ApolloCore.Helpers;
using Xunit;

namespace Daedalus.Tests.Rotation.ApolloCore.Helpers;

/// <summary>
/// Tests for the WHM Lily cap-prevention dump decision: dump at 3/3, or at 2/3 when the next Lily is
/// about to tick (so a regen is never wasted while sitting full).
/// </summary>
public sealed class LilyOvercapPolicyTests
{
    [Fact]
    public void DumpsAtFullLilies()
    {
        Assert.True(LilyOvercapPolicy.ShouldDump(3, 20f));
    }

    [Fact]
    public void DumpsAtTwoThirds_WhenNextLilyImminent()
    {
        Assert.True(LilyOvercapPolicy.ShouldDump(2, 1.5f));
    }

    [Fact]
    public void DoesNotDumpAtTwoThirds_WhenNextLilyFarOff()
    {
        Assert.False(LilyOvercapPolicy.ShouldDump(2, 12f));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void DoesNotDumpBelowTwoThirds(int lilyCount)
    {
        // Even with the timer about to tick, 0/1 Lilies are far from overcap.
        Assert.False(LilyOvercapPolicy.ShouldDump(lilyCount, 0.5f));
    }

    [Fact]
    public void TwoThirds_AtExactlyTheWindowBoundary_Dumps()
    {
        Assert.True(LilyOvercapPolicy.ShouldDump(2, LilyOvercapPolicy.NearlyFullOvercapSeconds));
    }
}
