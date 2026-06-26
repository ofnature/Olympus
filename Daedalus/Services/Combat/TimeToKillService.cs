using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;

namespace Daedalus.Services.Combat;

/// <summary>
/// HP-sampling TTK estimator. Records each enemy's HP at ~1 Hz over a rolling
/// window and derives a linear HP-loss rate. Self-contained (no combat-event
/// dependency), matching RSR's periodic-HP-sample approach.
/// </summary>
public sealed class TimeToKillService : ITimeToKillService
{
    /// <summary>Minimal HP snapshot used by the internal sampling seam (test-friendly, no Dalamud types).</summary>
    internal readonly struct EnemyHpSnapshot
    {
        public readonly ulong Id;
        public readonly uint Hp;
        public readonly bool Dead;
        public EnemyHpSnapshot(ulong id, uint hp, bool dead = false) { Id = id; Hp = hp; Dead = dead; }
    }

    private readonly struct HpSample
    {
        public readonly float Time;
        public readonly uint Hp;
        public HpSample(float time, uint hp) { Time = time; Hp = hp; }
    }

    private const float SampleIntervalSeconds = 1f; // RSR samples HP at ~1 Hz
    private const float WindowSeconds = 6f;          // rolling window for the rate estimate
    private const int MaxSamplesPerEnemy = 8;
    private const float StaleAfterSeconds = 3f;      // drop enemies not seen recently

    private readonly Dictionary<ulong, List<HpSample>> _samples = new();
    private readonly Dictionary<ulong, float> _lastSeen = new();
    private readonly List<ulong> _cleanupBuffer = new();
    private readonly Func<double> _nowSeconds;

    private float _now;
    private float _lastSampleTime = -1f;

    public float AverageTtk { get; private set; } = float.MaxValue;

    /// <summary>
    /// Creates the service. <paramref name="nowSeconds"/> supplies a monotonically
    /// increasing time source in seconds; defaults to a process Stopwatch. Tests
    /// inject a controllable clock for deterministic sampling.
    /// </summary>
    public TimeToKillService(Func<double>? nowSeconds = null)
    {
        if (nowSeconds is not null)
        {
            _nowSeconds = nowSeconds;
        }
        else
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _nowSeconds = () => sw.Elapsed.TotalSeconds;
        }
    }

    public void Update(IEnumerable<IBattleChara> enemies)
        => UpdateInternal(ProjectSnapshots(enemies));

    private static IEnumerable<EnemyHpSnapshot> ProjectSnapshots(IEnumerable<IBattleChara> enemies)
    {
        foreach (var e in enemies)
        {
            if (e is not null)
                yield return new EnemyHpSnapshot(e.GameObjectId, e.CurrentHp, e.IsDead);
        }
    }

    internal void UpdateInternal(IEnumerable<EnemyHpSnapshot> enemies)
    {
        _now = (float)_nowSeconds();

        // Throttle to ~1 Hz. Enumeration is lazy at the call site, so returning
        // here avoids iterating the object table on non-sample frames.
        if (_lastSampleTime >= 0f && _now - _lastSampleTime < SampleIntervalSeconds)
            return;
        _lastSampleTime = _now;

        var ttkSum = 0f;
        var ttkCount = 0;

        foreach (var enemy in enemies)
        {
            if (enemy.Dead || enemy.Hp == 0) continue;
            var id = enemy.Id;
            _lastSeen[id] = _now;

            if (!_samples.TryGetValue(id, out var list))
            {
                list = new List<HpSample>(MaxSamplesPerEnemy);
                _samples[id] = list;
            }

            list.Add(new HpSample(_now, enemy.Hp));
            while (list.Count > MaxSamplesPerEnemy ||
                   (list.Count > 1 && _now - list[0].Time > WindowSeconds))
                list.RemoveAt(0);

            var ttk = ComputeTtk(list, enemy.Hp);
            if (ttk < float.MaxValue) { ttkSum += ttk; ttkCount++; }
        }

        AverageTtk = ttkCount > 0 ? ttkSum / ttkCount : float.MaxValue;
        CleanupStale();
    }

    public float GetTtkSeconds(IBattleChara enemy)
        => enemy is null ? float.MaxValue : GetTtkSeconds(enemy.GameObjectId);

    public float GetTtkSeconds(ulong gameObjectId)
        => _samples.TryGetValue(gameObjectId, out var list) && list.Count > 0
            ? ComputeTtk(list, list[^1].Hp)
            : float.MaxValue;

    private static float ComputeTtk(List<HpSample> list, uint currentHp)
    {
        if (list.Count < 2 || currentHp == 0) return float.MaxValue;
        var oldest = list[0];
        var newest = list[^1];
        var dt = newest.Time - oldest.Time;
        if (dt <= 0f) return float.MaxValue;
        var lost = (float)oldest.Hp - newest.Hp;
        if (lost <= 0f) return float.MaxValue; // stable or healing -> not dying
        return currentHp / (lost / dt);
    }

    private void CleanupStale()
    {
        _cleanupBuffer.Clear();
        foreach (var kvp in _lastSeen)
            if (_now - kvp.Value > StaleAfterSeconds)
                _cleanupBuffer.Add(kvp.Key);
        foreach (var id in _cleanupBuffer)
        {
            _lastSeen.Remove(id);
            _samples.Remove(id);
        }
    }

    public void Clear()
    {
        _samples.Clear();
        _lastSeen.Clear();
        AverageTtk = float.MaxValue;
        _lastSampleTime = -1f;
    }
}
