using Dalamud.Game.ClientState.Objects.Types;
using Daedalus.Data;
using Daedalus.Rotation.Common.Helpers;

namespace Daedalus.Rotation.ThemisCore.Helpers;

/// <summary>
/// Helper class for checking Paladin status effects.
/// </summary>
public sealed class ThemisStatusHelper : BaseStatusHelper
{
    /// <summary>
    /// Checks if the player has Iron Will (tank stance) active.
    /// </summary>
    public bool HasIronWill(IBattleChara player)
    {
        return HasStatus(player, PLDActions.StatusIds.IronWill);
    }

    /// <summary>
    /// Checks if the player has Fight or Flight active.
    /// </summary>
    public bool HasFightOrFlight(IBattleChara player)
    {
        return HasStatus(player, PLDActions.StatusIds.FightOrFlight);
    }

    /// <summary>
    /// Checks if the player has Goring Blade Ready (granted by Fight or Flight).
    /// In Dawntrail, Goring Blade is a proc weaponskill gated by this status — it is no
    /// longer a DoT/combo action.
    /// </summary>
    public bool HasGoringBladeReady(IBattleChara player)
    {
        return HasStatus(player, PLDActions.StatusIds.GoringBladeReady);
    }

    /// <summary>
    /// Gets the remaining duration of Fight or Flight.
    /// </summary>
    public float GetFightOrFlightRemaining(IBattleChara player)
    {
        return GetStatusRemaining(player, PLDActions.StatusIds.FightOrFlight);
    }

    /// <summary>
    /// Checks if the player has Requiescat active.
    /// </summary>
    public bool HasRequiescat(IBattleChara player)
    {
        return HasStatus(player, PLDActions.StatusIds.Requiescat);
    }

    /// <summary>
    /// Gets the number of Requiescat stacks remaining.
    /// </summary>
    public int GetRequiescatStacks(IBattleChara player)
    {
        return GetStatusStacks(player, PLDActions.StatusIds.Requiescat);
    }

    /// <summary>
    /// Checks if the player has Divine Might active (granted by Royal Authority).
    /// Makes the next Holy Spirit cast instant.
    /// </summary>
    public bool HasDivineMight(IBattleChara player)
    {
        return HasStatus(player, PLDActions.StatusIds.DivineMight);
    }

    /// <summary>
    /// Checks if the player has Atonement Ready (granted by Royal Authority).
    /// Dawntrail: status 1902 is a single proc, not a stack count.
    /// </summary>
    public bool HasAtonementReady(IBattleChara player)
    {
        return HasStatus(player, PLDActions.StatusIds.AtonementReady);
    }

    /// <summary>
    /// Checks if the player has Sword Oath / Atonement Ready active.
    /// </summary>
    public bool HasSwordOath(IBattleChara player) => HasAtonementReady(player);

    /// <summary>
    /// Legacy name — Dawntrail no longer uses stack count on this buff. Returns 1 when ready, else 0.
    /// </summary>
    public int GetSwordOathStacks(IBattleChara player)
    {
        return HasAtonementReady(player) ? 1 : 0;
    }

    /// <summary>
    /// Checks if the player has Sheltron or Holy Sheltron active.
    /// </summary>
    public bool HasSheltron(IBattleChara player)
    {
        return HasStatus(player, PLDActions.StatusIds.Sheltron) ||
               HasStatus(player, PLDActions.StatusIds.HolySheltron);
    }

    /// <summary>
    /// Checks if the player has Rampart active.
    /// </summary>
    public bool HasRampart(IBattleChara player)
    {
        return HasStatus(player, PLDActions.StatusIds.Rampart);
    }

    /// <summary>
    /// Checks if the player has Sentinel/Guardian active.
    /// </summary>
    public bool HasSentinel(IBattleChara player)
    {
        return HasStatus(player, PLDActions.StatusIds.Sentinel) ||
               HasStatus(player, PLDActions.StatusIds.Guardian);
    }

    /// <summary>
    /// Checks if the player has Hallowed Ground active.
    /// </summary>
    public bool HasHallowedGround(IBattleChara player)
    {
        return HasStatus(player, PLDActions.StatusIds.HallowedGround);
    }

    /// <summary>
    /// Checks if the player has Bulwark active.
    /// </summary>
    public bool HasBulwark(IBattleChara player)
    {
        return HasStatus(player, PLDActions.StatusIds.Bulwark);
    }

    /// <summary>
    /// Checks if the player has Arm's Length active.
    /// </summary>
    public bool HasArmsLength(IBattleChara player)
    {
        return HasStatus(player, PLDActions.StatusIds.ArmsLength);
    }

    /// <summary>
    /// Checks if any defensive cooldown is active.
    /// </summary>
    public bool HasActiveMitigation(IBattleChara player)
    {
        return HasSheltron(player) ||
               HasRampart(player) ||
               HasSentinel(player) ||
               HasHallowedGround(player) ||
               HasBulwark(player) ||
               HasArmsLength(player);
    }

    /// <summary>
    /// Gets a string listing all active mitigations for debug display.
    /// </summary>
    public string GetActiveMitigations(IBattleChara player)
    {
        var active = new System.Collections.Generic.List<string>();

        if (HasHallowedGround(player)) active.Add("Hallowed");
        if (HasSentinel(player)) active.Add("Sentinel");
        if (HasRampart(player)) active.Add("Rampart");
        if (HasSheltron(player)) active.Add("Sheltron");
        if (HasBulwark(player)) active.Add("Bulwark");
        if (HasArmsLength(player)) active.Add("Arm's Length");

        return FormatActiveList(active);
    }

    /// <summary>
    /// Gets the remaining duration of Goring Blade DoT on a target.
    /// </summary>
    public float GetGoringBladeRemaining(IBattleChara? target, uint playerId)
    {
        if (target == null) return 0f;
        return GetStatusRemainingFromSource(target, PLDActions.StatusIds.GoringBladeDot, playerId);
    }

    // Core status methods (HasStatus, GetStatusRemaining, GetStatusStacks, GetStatusRemainingFromSource) inherited from BaseStatusHelper
}
