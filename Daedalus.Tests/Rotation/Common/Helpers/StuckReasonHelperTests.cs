using System.Collections.Generic;
using Daedalus.Rotation.Common.Helpers;
using Xunit;

namespace Daedalus.Tests.Rotation.Common.Helpers;

public sealed class StuckReasonHelperTests
{
    [Fact]
    public void Null_WhenDispatched()
    {
        Assert.Null(StuckReasonHelper.Describe(dispatched: true, new[] { "FellCleave: Cooldown 3.2s" }));
    }

    [Fact]
    public void Null_WhenNoCandidates()
    {
        // Empty queue = module pushed nothing; its own state explains why (no target / not in combat).
        Assert.Null(StuckReasonHelper.Describe(dispatched: false, new List<string>()));
    }

    [Fact]
    public void Summarizes_FailReasons()
    {
        var s = StuckReasonHelper.Describe(dispatched: false,
            new[] { "FellCleave: Cooldown 3.2s", "InnerChaos: ProcBuff 1234" });
        Assert.Equal("Stuck — FellCleave: Cooldown 3.2s; InnerChaos: ProcBuff 1234", s);
    }

    [Fact]
    public void Truncates_LongLists()
    {
        var reasons = new[] { "A: x", "B: x", "C: x", "D: x", "E: x", "F: x" };
        var s = StuckReasonHelper.Describe(dispatched: false, reasons);
        Assert.NotNull(s);
        Assert.Contains("+2 more", s);
    }
}
