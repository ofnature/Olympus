using Olympus.Data;
using Olympus.Services;
using Olympus.Services.Action;

namespace Olympus.Rotation.HermesCore.Helpers;

/// <summary>
/// RSR AdjustId(Ninjutsu) slot probes — progress is game slot state, not MudraHelper counters.
/// </summary>
internal static class HermesNinjutsuSlotProbe
{
    public static uint GetSlotAdjustedId(IActionService actionService)
        => actionService.GetAdjustedActionId(NINActions.Ninjutsu.ActionId);

    public static bool IsNoActiveNinjutsu(uint slotId) => slotId == NINActions.Ninjutsu.ActionId;

    public static bool IsFumaShurikenCurrent(uint slotId) => slotId == NINActions.FumaShuriken.ActionId;

    public static bool IsKatonCurrent(uint slotId) => slotId == NINActions.Katon.ActionId;

    public static bool IsRaitonCurrent(uint slotId) => slotId == NINActions.Raiton.ActionId;

    public static bool IsHyotonCurrent(uint slotId) => slotId == NINActions.Hyoton.ActionId;

    public static bool IsHutonCurrent(uint slotId) => slotId == NINActions.Huton.ActionId;

    public static bool IsDotonCurrent(uint slotId) => slotId == NINActions.Doton.ActionId;

    public static bool IsSuitonCurrent(uint slotId) => slotId == NINActions.Suiton.ActionId;

    public static bool IsGokaMekkyakuCurrent(uint slotId) => slotId == NINActions.GokaMekkyaku.ActionId;

    public static bool IsHyoshoRanryuCurrent(uint slotId) => slotId == NINActions.HyoshoRanryu.ActionId;

    public static bool IsRabbitMediumCurrent(uint slotId) => slotId == NINActions.RabbitMedium.ActionId;

    public static string DescribeSlot(uint slotId)
    {
        if (IsNoActiveNinjutsu(slotId)) return "NoActive";
        if (IsFumaShurikenCurrent(slotId)) return "FumaShuriken";
        if (IsKatonCurrent(slotId)) return "Katon";
        if (IsRaitonCurrent(slotId)) return "Raiton";
        if (IsHyotonCurrent(slotId)) return "Hyoton";
        if (IsHutonCurrent(slotId)) return "Huton";
        if (IsDotonCurrent(slotId)) return "Doton";
        if (IsSuitonCurrent(slotId)) return "Suiton";
        if (IsGokaMekkyakuCurrent(slotId)) return "GokaMekkyaku";
        if (IsHyoshoRanryuCurrent(slotId)) return "HyoshoRanryu";
        if (IsRabbitMediumCurrent(slotId)) return "RabbitMedium";
        return $"Unknown({slotId})";
    }

    /// <summary>RSR branch label for Suiton-path slot probes (NoActive/Fuma/Raiton/Suiton/etc.).</summary>
    public static string GetMatchedSlotBranch(uint slotId)
    {
        if (IsNoActiveNinjutsu(slotId)) return "NoActive";
        if (IsFumaShurikenCurrent(slotId)) return "Fuma";
        if (IsKatonCurrent(slotId)) return "Katon";
        if (IsRaitonCurrent(slotId)) return "Raiton";
        if (IsHyotonCurrent(slotId)) return "Hyoton";
        if (IsHutonCurrent(slotId)) return "Huton";
        if (IsDotonCurrent(slotId)) return "Doton";
        if (IsSuitonCurrent(slotId)) return "Suiton";
        if (IsGokaMekkyakuCurrent(slotId)) return "GokaMekkyaku";
        if (IsHyoshoRanryuCurrent(slotId)) return "HyoshoRanryu";
        if (IsRabbitMediumCurrent(slotId)) return "RabbitMedium";
        return "Unknown";
    }

    public static uint GetSlotFromActionManager()
    {
        unsafe
        {
            var actionManager = SafeGameAccess.GetActionManager(null);
            if (actionManager == null)
                return 0;
            return actionManager->GetAdjustedActionId(NINActions.Ninjutsu.ActionId);
        }
    }

    /// <summary>Prefer ActionManager slot when present — matches mudra executor probes.</summary>
    public static uint GetEffectiveSlotId(IActionService actionService)
    {
        var fromManager = GetSlotFromActionManager();
        return fromManager != 0 ? fromManager : GetSlotAdjustedId(actionService);
    }

    public static bool IsRabbitFailureSlot(IActionService actionService)
        => IsRabbitMediumCurrent(GetEffectiveSlotId(actionService));
}
