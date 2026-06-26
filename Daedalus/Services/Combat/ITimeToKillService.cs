using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;

namespace Daedalus.Services.Combat;

/// <summary>
/// Estimates enemy time-to-kill (TTK) by sampling HP over time.
/// Mirrors RSR's ObjectHelper.GetTTK / DataCenter.AverageTTK: rotations use it to
/// avoid wasting DoTs / long resources on targets that will die imminently.
/// </summary>
public interface ITimeToKillService
{
    /// <summary>Samples enemy HP. Call once per frame; sampling is throttled internally to ~1 Hz.</summary>
    void Update(IEnumerable<IBattleChara> enemies);

    /// <summary>Estimated seconds until the enemy dies at the current HP-loss rate, or float.MaxValue if unknown / not declining.</summary>
    float GetTtkSeconds(IBattleChara enemy);

    /// <summary>TTK lookup by game object id (no need to resolve the IBattleChara).</summary>
    float GetTtkSeconds(ulong gameObjectId);

    /// <summary>Average TTK across all tracked enemies, or float.MaxValue if none.</summary>
    float AverageTtk { get; }

    /// <summary>Clears all samples (e.g. on zone change).</summary>
    void Clear();
}
