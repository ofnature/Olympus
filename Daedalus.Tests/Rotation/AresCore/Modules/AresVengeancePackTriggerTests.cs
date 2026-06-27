using System.Linq;
using Daedalus.Rotation.AresCore.Abilities;
using Daedalus.Rotation.AresCore.Modules;
using Daedalus.Rotation.Common.Scheduling;
using Daedalus.Tests.Rotation.AresCore;
using Daedalus.Tests.Rotation.Common.Scheduling;
using Xunit;

namespace Daedalus.Tests.Rotation.AresCore.Modules;

/// <summary>
/// Vengeance / Damnation now has a pull-size trigger (<c>VengeanceMinTargets</c>): on a wall-to-wall pull
/// of N+ engaged enemies it fires on cooldown for sustained AoE mitigation, in addition to the reactive
/// HP / damage-rate gate. Full HP + a big pack should still pop it.
/// </summary>
public sealed class AresVengeancePackTriggerTests
{
    private readonly MitigationModule _module = new();

    private AbilityCandidate? CollectVengeance(
        int enemyCount,
        int minTargets = 3,
        bool enableVengeance = true,
        bool damageGate = false,
        float hpPercent = 1.0f)
    {
        const uint maxHp = 100000;
        var currentHp = (uint)(maxHp * hpPercent);

        var config = AresTestContext.CreateDefaultWarriorConfiguration();
        config.Tank.EnableVengeance = enableVengeance;
        config.Tank.VengeanceMinTargets = minTargets;

        var scheduler = SchedulerFactory.CreateForTest(config: config);
        var context = AresTestContext.CreateMock(
            config: config,
            level: 100,
            currentHp: currentHp,
            maxHp: maxHp,
            enemyCount: enemyCount,
            tankCooldownShouldUseMajor: damageGate,
            canExecuteOgcd: true);

        _module.CollectCandidates(context, scheduler, isMoving: false);
        return scheduler.InspectOgcdQueue()
            .Cast<AbilityCandidate?>()
            .FirstOrDefault(c => c!.Value.Behavior == AresAbilities.Vengeance);
    }

    [Fact]
    public void Fires_OnBigPull_AtFullHp()
    {
        // 4 engaged enemies, full HP, reactive damage gate off — pull-size trigger alone fires it.
        Assert.NotNull(CollectVengeance(enemyCount: 4, minTargets: 3));
    }

    [Fact]
    public void DoesNotFire_BelowPullSize_WhenHpFine()
    {
        Assert.Null(CollectVengeance(enemyCount: 2, minTargets: 3));
    }

    [Fact]
    public void StillFires_OnHpDamageGate_WithFewEnemies()
    {
        // Single-target boss: pull-size trigger never fires, but the reactive HP/damage gate still does.
        Assert.NotNull(CollectVengeance(enemyCount: 1, minTargets: 3, damageGate: true));
    }

    [Fact]
    public void RespectsConfiguredPullSize()
    {
        Assert.Null(CollectVengeance(enemyCount: 4, minTargets: 5));
        Assert.NotNull(CollectVengeance(enemyCount: 4, minTargets: 4));
    }

    [Fact]
    public void DoesNotFire_WhenDisabled_EvenOnBigPull()
    {
        Assert.Null(CollectVengeance(enemyCount: 6, minTargets: 3, enableVengeance: false));
    }

    [Fact]
    public void WeavesAtPriority2()
    {
        var candidate = CollectVengeance(enemyCount: 4, minTargets: 3);
        Assert.NotNull(candidate);
        Assert.Equal(2, candidate!.Value.Priority);
    }
}
