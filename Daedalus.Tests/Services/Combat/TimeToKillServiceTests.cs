using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Daedalus.Services.Combat;
using Xunit;

namespace Daedalus.Tests.Services.Combat;

/// <summary>
/// Unit tests for <see cref="TimeToKillService"/>. Uses the internal sampling seam
/// (<c>UpdateInternal</c> + <c>EnemyHpSnapshot</c>) and an injected clock so the
/// ~1 Hz throttle and TTK math are deterministic without mocking Dalamud types.
/// </summary>
public class TimeToKillServiceTests
{
    private static IEnumerable<TimeToKillService.EnemyHpSnapshot> One(ulong id, uint hp, bool dead = false)
        => new[] { new TimeToKillService.EnemyHpSnapshot(id, hp, dead) };

    [Fact]
    public void DecliningHp_ProducesFiniteTtk()
    {
        double now = 0;
        var svc = new TimeToKillService(() => now);

        svc.UpdateInternal(One(1, 10000));
        now = 1; svc.UpdateInternal(One(1, 9000));
        now = 2; svc.UpdateInternal(One(1, 8000));

        // oldest (t0,10000), newest (t2,8000): rate 1000 hp/s, currentHp 8000 -> 8s
        var ttk = svc.GetTtkSeconds(1UL);
        Assert.True(Math.Abs(ttk - 8f) < 0.01f, $"Expected ~8s, got {ttk}");
        Assert.True(Math.Abs(svc.AverageTtk - 8f) < 0.01f, $"Expected AverageTtk ~8s, got {svc.AverageTtk}");
    }

    [Fact]
    public void StableHp_ReturnsMaxValue()
    {
        double now = 0;
        var svc = new TimeToKillService(() => now);

        svc.UpdateInternal(One(1, 10000));
        now = 1; svc.UpdateInternal(One(1, 10000));
        now = 2; svc.UpdateInternal(One(1, 10000));

        Assert.Equal(float.MaxValue, svc.GetTtkSeconds(1UL));
        Assert.Equal(float.MaxValue, svc.AverageTtk);
    }

    [Fact]
    public void HealingTarget_ReturnsMaxValue()
    {
        double now = 0;
        var svc = new TimeToKillService(() => now);

        svc.UpdateInternal(One(1, 8000));
        now = 1; svc.UpdateInternal(One(1, 9000));
        now = 2; svc.UpdateInternal(One(1, 10000));

        Assert.Equal(float.MaxValue, svc.GetTtkSeconds(1UL));
    }

    [Fact]
    public void SingleSample_ReturnsMaxValue()
    {
        double now = 0;
        var svc = new TimeToKillService(() => now);

        svc.UpdateInternal(One(1, 10000));

        Assert.Equal(float.MaxValue, svc.GetTtkSeconds(1UL));
    }

    [Fact]
    public void UnseenEnemy_IsCleanedUpAfterStaleWindow()
    {
        double now = 0;
        var svc = new TimeToKillService(() => now);

        svc.UpdateInternal(One(1, 10000));
        now = 1; svc.UpdateInternal(One(1, 9000));

        // Enemy 1 last seen at t=1. Advancing past the stale window without it drops it.
        now = 5; svc.UpdateInternal(Enumerable.Empty<TimeToKillService.EnemyHpSnapshot>());

        Assert.Equal(float.MaxValue, svc.GetTtkSeconds(1UL));
    }

    [Fact]
    public void SubSecondUpdates_AreThrottled()
    {
        double now = 0;
        var svc = new TimeToKillService(() => now);

        svc.UpdateInternal(One(1, 10000));
        // These fall inside the 1 Hz throttle window and must not add samples.
        now = 0.2; svc.UpdateInternal(One(1, 9000));
        now = 0.4; svc.UpdateInternal(One(1, 8000));

        // Only one sample recorded -> insufficient data -> MaxValue.
        Assert.Equal(float.MaxValue, svc.GetTtkSeconds(1UL));
    }

    [Fact]
    public void DeadEnemy_IsNotSampled()
    {
        double now = 0;
        var svc = new TimeToKillService(() => now);

        svc.UpdateInternal(One(1, 10000, dead: true));
        now = 1; svc.UpdateInternal(One(1, 9000, dead: true));

        Assert.Equal(float.MaxValue, svc.GetTtkSeconds(1UL));
    }

    [Fact]
    public void NullBattleChara_ReturnsMaxValue()
    {
        var svc = new TimeToKillService(() => 0);
        Assert.Equal(float.MaxValue, svc.GetTtkSeconds((IBattleChara?)null!));
    }

    [Fact]
    public void Clear_ResetsState()
    {
        double now = 0;
        var svc = new TimeToKillService(() => now);

        svc.UpdateInternal(One(1, 10000));
        now = 1; svc.UpdateInternal(One(1, 9000));
        now = 2; svc.UpdateInternal(One(1, 8000));
        Assert.True(svc.GetTtkSeconds(1UL) < float.MaxValue);

        svc.Clear();

        Assert.Equal(float.MaxValue, svc.GetTtkSeconds(1UL));
        Assert.Equal(float.MaxValue, svc.AverageTtk);
    }
}
