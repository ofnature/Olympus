using System.Linq;
using Daedalus.Rotation.AresCore.Abilities;
using Daedalus.Rotation.AresCore.Modules;
using Daedalus.Rotation.Common.Scheduling;
using Daedalus.Tests.Rotation.AresCore;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Xunit;

namespace Daedalus.Tests.Rotation.AresCore.Modules;

/// <summary>
/// Bloodwhetting / Raw Intuition now uses a dedicated <c>BloodwhettingThreshold</c> (decoupled from the
/// shared major-cooldown <c>MitigationThreshold</c>) and weaves at oGCD priority 2 so it isn't starved out
/// of the single weave slot by damage oGCDs (Upheaval/Orogeny pri 2, Onslaught pri 4) at low HP.
/// </summary>
public sealed class AresBloodwhettingThresholdTests
{
    private readonly MitigationModule _module = new();

    private AbilityCandidate? CollectBloodwhetting(
        float hpPercent,
        float bloodwhettingThreshold,
        float mitigationThreshold = 0.70f,
        bool enableBloodwhetting = true,
        bool hasBloodwhetting = false)
    {
        const uint maxHp = 100000;
        var currentHp = (uint)(maxHp * hpPercent);

        var config = AresTestContext.CreateDefaultWarriorConfiguration();
        config.Tank.EnableBloodWhetting = enableBloodwhetting;
        config.Tank.BloodwhettingThreshold = bloodwhettingThreshold;
        config.Tank.MitigationThreshold = mitigationThreshold;

        var scheduler = SchedulerFactory.CreateForTest(config: config);
        var context = AresTestContext.CreateMock(
            config: config,
            level: 100,
            currentHp: currentHp,
            maxHp: maxHp,
            hasBloodwhetting: hasBloodwhetting,
            canExecuteOgcd: true);

        _module.CollectCandidates(context, scheduler, isMoving: false);
        return scheduler.InspectOgcdQueue()
            .Cast<AbilityCandidate?>()
            .FirstOrDefault(c => c!.Value.Behavior == AresAbilities.RawIntuition);
    }

    [Fact]
    public void Fires_WhenHpAtOrBelowDedicatedThreshold()
    {
        Assert.NotNull(CollectBloodwhetting(hpPercent: 0.17f, bloodwhettingThreshold: 0.70f));
    }

    [Fact]
    public void DoesNotFire_WhenHpAboveDedicatedThreshold()
    {
        Assert.Null(CollectBloodwhetting(hpPercent: 0.85f, bloodwhettingThreshold: 0.70f));
    }

    [Fact]
    public void UsesDedicatedThreshold_NotSharedMitigationThreshold()
    {
        // Shared mitigation gate is conservative (30%) but Bloodwhetting's own slider is generous (80%).
        // At 60% HP it must fire off its OWN threshold, proving the two are decoupled.
        Assert.NotNull(CollectBloodwhetting(
            hpPercent: 0.60f, bloodwhettingThreshold: 0.80f, mitigationThreshold: 0.30f));
    }

    [Fact]
    public void DoesNotFire_WhenHpAboveDedicatedThreshold_EvenIfBelowSharedThreshold()
    {
        // Shared gate would allow it (HP below 90%) but the dedicated slider is tighter (40%).
        Assert.Null(CollectBloodwhetting(
            hpPercent: 0.60f, bloodwhettingThreshold: 0.40f, mitigationThreshold: 0.90f));
    }

    [Fact]
    public void WeavesAtPriority2_AboveDamageOgcds()
    {
        var candidate = CollectBloodwhetting(hpPercent: 0.17f, bloodwhettingThreshold: 0.70f);
        Assert.NotNull(candidate);
        Assert.Equal(2, candidate!.Value.Priority);
    }

    [Fact]
    public void DoesNotFire_WhenDisabled()
    {
        Assert.Null(CollectBloodwhetting(
            hpPercent: 0.17f, bloodwhettingThreshold: 0.70f, enableBloodwhetting: false));
    }

    [Fact]
    public void DoesNotFire_WhenBloodwhettingAlreadyActive()
    {
        Assert.Null(CollectBloodwhetting(
            hpPercent: 0.17f, bloodwhettingThreshold: 0.70f, hasBloodwhetting: true));
    }
}
