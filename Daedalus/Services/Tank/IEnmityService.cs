using Dalamud.Game.ClientState.Objects.Types;

namespace Daedalus.Services.Tank;

/// <summary>
/// Service for tracking enmity (aggro) state.
/// </summary>
public interface IEnmityService
{
    /// <summary>
    /// Gets the player's enmity position on the specified target (1 = highest).
    /// Returns 0 if the target has no enmity list or player is not on it.
    /// </summary>
    int GetEnmityPosition(IBattleChara target, uint playerEntityId);

    /// <summary>
    /// Returns true if the player has the highest enmity on the specified target.
    /// </summary>
    bool IsMainTankOn(IBattleChara target, uint playerEntityId);

    /// <summary>
    /// Returns true if another tank in the party has higher enmity than the player.
    /// Used for tank swap detection.
    /// </summary>
    bool HasCoTankAggro(IBattleChara target, uint playerEntityId);

    /// <summary>
    /// Gets the entity ID of the party member with highest enmity on the target.
    /// Returns 0 if no one has enmity.
    /// </summary>
    uint GetMainTankId(IBattleChara target);

    /// <summary>
    /// Checks if the player is about to lose aggro (enmity is close to being overtaken).
    /// Used for preemptive Provoke suggestions.
    /// </summary>
    bool IsLosingAggro(IBattleChara target, uint playerEntityId, float threshold = 0.9f);

    /// <summary>
    /// Returns true when the target is actively focused on another entity (not the player and not
    /// nobody) — i.e. we have genuinely lost it to someone else, as opposed to an un-aggroed idle mob.
    /// Used to decide not to chase a slipped mob (Provoke reclaims it instead). Distinct from a fresh
    /// pull where the target has no current focus.
    /// </summary>
    bool HasLostAggroToOther(IBattleChara target, uint playerEntityId);
}
