using Daedalus.Rotation.EchidnaCore.Helpers;
using Xunit;

namespace Daedalus.Tests.Rotation.EchidnaCore.Helpers;

/// <summary>
/// Tests for the Reawaken self-buff entry gate. The key behavior: Hunter's Instinct and Swiftscaled must
/// both have enough duration to cover the burst, but Noxious Gnash (a per-target debuff) is deliberately NOT
/// a factor — gating on it would block Reawaken after a pack target-swap and overcap Serpent's Offering.
/// </summary>
public sealed class EchidnaReawakenPolicyTests
{
    [Fact]
    public void BuffsReady_WhenBothSelfBuffsComfortablyRemain()
    {
        Assert.True(EchidnaReawakenPolicy.BuffsReadyForReawaken(
            hasHuntersInstinct: true, huntersInstinctRemaining: 20f,
            hasSwiftscaled: true, swiftscaledRemaining: 20f));
    }

    [Fact]
    public void Blocks_WhenHuntersInstinctMissing()
    {
        Assert.False(EchidnaReawakenPolicy.BuffsReadyForReawaken(
            hasHuntersInstinct: false, huntersInstinctRemaining: 0f,
            hasSwiftscaled: true, swiftscaledRemaining: 20f));
    }

    [Fact]
    public void Blocks_WhenHuntersInstinctTooShort()
    {
        Assert.False(EchidnaReawakenPolicy.BuffsReadyForReawaken(
            hasHuntersInstinct: true, huntersInstinctRemaining: 5f,
            hasSwiftscaled: true, swiftscaledRemaining: 20f));
    }

    [Fact]
    public void Blocks_WhenSwiftscaledMissing()
    {
        Assert.False(EchidnaReawakenPolicy.BuffsReadyForReawaken(
            hasHuntersInstinct: true, huntersInstinctRemaining: 20f,
            hasSwiftscaled: false, swiftscaledRemaining: 0f));
    }

    [Fact]
    public void Blocks_WhenSwiftscaledTooShort()
    {
        Assert.False(EchidnaReawakenPolicy.BuffsReadyForReawaken(
            hasHuntersInstinct: true, huntersInstinctRemaining: 20f,
            hasSwiftscaled: true, swiftscaledRemaining: 9.9f));
    }

    [Fact]
    public void AllowsAtExactThreshold()
    {
        Assert.True(EchidnaReawakenPolicy.BuffsReadyForReawaken(
            hasHuntersInstinct: true, huntersInstinctRemaining: EchidnaReawakenPolicy.MinBuffSeconds,
            hasSwiftscaled: true, swiftscaledRemaining: EchidnaReawakenPolicy.MinBuffSeconds));
    }

    [Fact]
    public void NoxiousGnashIsNotAFactor_SelfBuffsAloneDecide()
    {
        // The policy signature has no Noxious Gnash input by design: a pack target-swap (debuff at 0) must
        // not block the burst. With self-buffs healthy, entry is allowed regardless of any debuff state.
        Assert.True(EchidnaReawakenPolicy.BuffsReadyForReawaken(
            hasHuntersInstinct: true, huntersInstinctRemaining: 15f,
            hasSwiftscaled: true, swiftscaledRemaining: 15f));
    }
}
