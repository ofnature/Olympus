using Dalamud.Game.ClientState.Objects.Types;
using Daedalus.Data;
using Daedalus.Rotation.Common.Helpers;

namespace Daedalus.Rotation.HermesCore.Helpers;

/// <summary>
/// Helper class for checking Ninja status effects.
/// </summary>
public sealed class HermesStatusHelper : BaseStatusHelper
{
    #region Ninjutsu Buffs

    /// <summary>
    /// Checks if the player has Shadow Walker (Dawntrail Suiton/Huton buff that enables Kunai's Bane).
    /// </summary>
    public bool HasSuiton(IBattleChara player)
    {
        return HasShadowWalker(player);
    }

    /// <summary>
    /// Dawntrail Shadow Walker buff — replaces the legacy Suiton (507) status.
    /// </summary>
    public bool HasShadowWalker(IBattleChara player)
    {
        return HasStatus(player, NINActions.StatusIds.ShadowWalker)
               || HasStatus(player, NINActions.StatusIds.Suiton);
    }

    /// <summary>
    /// Gets the remaining duration of Shadow Walker (shown as Suiton in debug for burst prep).
    /// </summary>
    public float GetSuitonRemaining(IBattleChara player)
    {
        if (HasStatus(player, NINActions.StatusIds.ShadowWalker, out var remaining))
            return remaining;

        return GetStatusRemaining(player, NINActions.StatusIds.Suiton);
    }

    /// <summary>
    /// Checks if the player has Kassatsu active.
    /// </summary>
    public bool HasKassatsu(IBattleChara player)
    {
        return HasStatus(player, NINActions.StatusIds.Kassatsu);
    }

    /// <summary>
    /// Checks if the player has Ten Chi Jin active.
    /// </summary>
    public bool HasTenChiJin(IBattleChara player)
    {
        return HasStatus(player, NINActions.StatusIds.TenChiJin);
    }

    /// <summary>
    /// Gets the remaining stacks of Ten Chi Jin.
    /// </summary>
    public int GetTenChiJinStacks(IBattleChara player)
    {
        return GetStatusStacks(player, NINActions.StatusIds.TenChiJin);
    }

    /// <summary>
    /// Checks if a mudra is currently active (mid-sequence).
    /// </summary>
    public bool IsMudraActive(IBattleChara player)
    {
        return HasStatus(player, NINActions.StatusIds.Mudra);
    }

    #endregion

    #region Combat Buffs

    /// <summary>
    /// Checks if the player has Bunshin active.
    /// </summary>
    public bool HasBunshin(IBattleChara player)
    {
        return HasStatus(player, NINActions.StatusIds.Bunshin);
    }

    /// <summary>
    /// Gets the remaining stacks of Bunshin.
    /// </summary>
    public int GetBunshinStacks(IBattleChara player)
    {
        return GetStatusStacks(player, NINActions.StatusIds.Bunshin);
    }

    /// <summary>
    /// Checks if Phantom Kamaitachi is ready.
    /// </summary>
    public bool HasPhantomKamaitachiReady(IBattleChara player)
    {
        return HasStatus(player, NINActions.StatusIds.PhantomKamaitachiReady);
    }

    /// <summary>
    /// Checks if Raiju is ready.
    /// </summary>
    public bool HasRaijuReady(IBattleChara player)
    {
        return HasStatus(player, NINActions.StatusIds.RaijuReady);
    }

    /// <summary>
    /// Gets the number of Raiju stacks available.
    /// </summary>
    public int GetRaijuStacks(IBattleChara player)
    {
        return GetStatusStacks(player, NINActions.StatusIds.RaijuReady);
    }

    /// <summary>
    /// Checks if Meisui is active.
    /// </summary>
    public bool HasMeisui(IBattleChara player)
    {
        return HasStatus(player, NINActions.StatusIds.Meisui);
    }

    /// <summary>
    /// Checks if Tenri Jindo is ready.
    /// </summary>
    public bool HasTenriJindoReady(IBattleChara player)
    {
        return HasStatus(player, NINActions.StatusIds.TenriJindoReady);
    }

    #endregion

    #region Debuff Tracking

    /// <summary>
    /// Checks if Kunai's Bane is on the target.
    /// </summary>
    public bool HasKunaisBane(IBattleChara target, uint playerId)
    {
        if (target.StatusList == null)
            return false;

        foreach (var status in target.StatusList)
        {
            if (status.StatusId == NINActions.StatusIds.KunaisBane && status.SourceId == playerId)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the remaining duration of Kunai's Bane on target.
    /// </summary>
    public float GetKunaisBaneRemaining(IBattleChara target, uint playerId)
    {
        if (target.StatusList == null)
            return 0f;

        foreach (var status in target.StatusList)
        {
            if (status.StatusId == NINActions.StatusIds.KunaisBane && status.SourceId == playerId)
                return status.RemainingTime;
        }
        return 0f;
    }

    /// <summary>
    /// Checks if Dokumori is on the target.
    /// </summary>
    public bool HasDokumori(IBattleChara target, uint playerId)
    {
        if (target.StatusList == null)
            return false;

        foreach (var status in target.StatusList)
        {
            if (status.StatusId == NINActions.StatusIds.Dokumori && status.SourceId == playerId)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the remaining duration of Dokumori on target.
    /// </summary>
    public float GetDokumoriRemaining(IBattleChara target, uint playerId)
    {
        if (target.StatusList == null)
            return 0f;

        foreach (var status in target.StatusList)
        {
            if (status.StatusId == NINActions.StatusIds.Dokumori && status.SourceId == playerId)
                return status.RemainingTime;
        }
        return 0f;
    }

    /// <summary>
    /// Checks if Vulnerability Up (Trick Attack) is on the target.
    /// </summary>
    public bool HasVulnerabilityUp(IBattleChara target, uint playerId)
    {
        if (target.StatusList == null)
            return false;

        foreach (var status in target.StatusList)
        {
            if (status.StatusId == NINActions.StatusIds.VulnerabilityUp && status.SourceId == playerId)
                return true;
        }
        return false;
    }

    #endregion

    #region Role Buffs

    /// <summary>
    /// Checks if True North is active.
    /// </summary>
    public bool HasTrueNorth(IBattleChara player)
    {
        return HasStatus(player, NINActions.StatusIds.TrueNorth);
    }

    #endregion

    #region Defensive Buffs

    /// <summary>
    /// Checks if Shade Shift is active.
    /// </summary>
    public bool HasShadeShift(IBattleChara player)
    {
        return HasStatus(player, NINActions.StatusIds.ShadeShift);
    }

    #endregion

    #region Kazematoi (Aeolian Edge buff)

    /// <summary>
    /// Kazematoi is job-gauge only — use <see cref="IHermesContext.Kazematoi"/> for stack count.
    /// </summary>
    public bool HasKazematoi(IBattleChara player) => false;

    /// <summary>
    /// Kazematoi is job-gauge only — use <see cref="IHermesContext.Kazematoi"/> for stack count.
    /// </summary>
    public int GetKazematoiStacks(IBattleChara player) => 0;

    #endregion

    // Core status methods (HasStatus, GetStatusRemaining, GetStatusStacks) inherited from BaseStatusHelper
}
