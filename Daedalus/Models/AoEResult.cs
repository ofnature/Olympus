using Dalamud.Game.ClientState.Objects.Types;

namespace Daedalus.Models;

public enum AoEShape { Circle, Cone, Line }

/// <summary>
/// Result of an optimal AoE target/angle computation.
/// </summary>
public readonly record struct AoEResult(
    IBattleNpc? Target,
    int HitCount,
    float OptimalAngle,
    AoEShape Shape);
