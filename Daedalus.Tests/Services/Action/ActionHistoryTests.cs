using Daedalus.Services.Action;
using Xunit;

namespace Daedalus.Tests.Services.Action;

public class ActionHistoryTests
{
    [Fact]
    public void NewInstance_HasNoHistory()
    {
        var h = new ActionHistory();
        Assert.Equal(0u, h.LastGcdId);
        Assert.Equal(0u, h.LastOgcdId);
        Assert.False(h.WasLastGcd(1234u));
        Assert.False(h.WasLastOgcd(1234u));
    }

    [Fact]
    public void RecordAction_SetsLastActionOnly()
    {
        var h = new ActionHistory();
        h.RecordAction(18873u);
        Assert.True(h.WasLastAction(18873u));
        Assert.False(h.WasLastGcd(18873u));
        Assert.False(h.WasLastOgcd(18873u));
    }

    [Fact]
    public void RecordGcd_SetsLastGcdAndLastAction()
    {
        var h = new ActionHistory();
        h.RecordGcd(2240u);
        Assert.True(h.WasLastGcd(2240u));
        Assert.True(h.WasLastAction(2240u));
        Assert.False(h.WasLastOgcd(2240u));
        Assert.Equal(0u, h.LastOgcdId);
    }

    [Fact]
    public void RecordOgcd_SetsLastOgcdAndId()
    {
        var h = new ActionHistory();
        h.RecordOgcd(2248u);
        Assert.True(h.WasLastOgcd(2248u));
        Assert.Equal(2248u, h.LastOgcdId);
        Assert.False(h.WasLastGcd(2248u));
    }

    [Fact]
    public void GcdAndOgcd_TrackedIndependently()
    {
        var h = new ActionHistory();
        h.RecordGcd(2240u);
        h.RecordOgcd(2248u);
        Assert.True(h.WasLastGcd(2240u));
        Assert.True(h.WasLastOgcd(2248u));
    }

    [Fact]
    public void ZeroId_NeverMatches()
    {
        var h = new ActionHistory();
        Assert.False(h.WasLastGcd(0u));
        Assert.False(h.WasLastOgcd(0u));
    }

    [Fact]
    public void TrickAttackAfterMug_SequencingScenario()
    {
        // NIN: Mug/Dokumori must land before Trick Attack/Kunai's Bane fires.
        const uint mug = 2248u;
        const uint trickAttack = 2258u;
        var h = new ActionHistory();

        // Mug dispatched → a Trick-Attack gate keyed on WasLastOgcd(mug) now opens.
        h.RecordOgcd(mug);
        Assert.True(h.WasLastOgcd(mug));

        // Trick Attack dispatched → the gate closes (Mug is no longer the last oGCD).
        h.RecordOgcd(trickAttack);
        Assert.False(h.WasLastOgcd(mug));
        Assert.True(h.WasLastOgcd(trickAttack));
    }
}
