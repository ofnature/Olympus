using Dalamud.Game.ClientState.Objects.SubKinds;
using Daedalus.Rotation.Common;

namespace Daedalus.Rotation;

/// <summary>
/// Base interface for all rotation implementations.
/// Each job module (Apollo/WHM, Athena/SCH, etc.) implements this interface.
/// </summary>
public interface IRotation
{
    /// <summary>
    /// Display name for this rotation (e.g., "Apollo" for WHM).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Job IDs supported by this rotation.
    /// For example, Apollo supports both WhiteMage (24) and Conjurer (6).
    /// </summary>
    uint[] SupportedJobIds { get; }

    /// <summary>
    /// Main execution loop - called every frame when the rotation is active.
    /// </summary>
    /// <param name="player">The local player character.</param>
    void Execute(IPlayerCharacter player);

    /// <summary>
    /// Called when the player changes territory (duty start/exit, zone change).
    /// Rotations use this to reset per-instance state so they re-establish buffs
    /// (e.g. Kardia) cleanly at duty start. Default implementation is a no-op.
    /// </summary>
    /// <param name="territoryType">The new territory type id.</param>
    void OnTerritoryChanged(ushort territoryType) { }

    /// <summary>
    /// Debug state for this rotation, used by the debug window.
    /// </summary>
    DebugState DebugState { get; }
}
