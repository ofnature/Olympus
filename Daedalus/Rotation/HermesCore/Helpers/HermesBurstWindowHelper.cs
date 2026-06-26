using Daedalus.Data;
using Daedalus.Services.Action;

namespace Daedalus.Rotation.HermesCore.Helpers;

/// <summary>
/// RSR InMug / InTrickAttack window mirrors for Ninki spender gating in Hermes DamageModule.
/// </summary>
internal static class HermesBurstWindowHelper
{
    /// <summary>RSR TrickAttack/KunaisBane burst debuff window (~17s).</summary>
    public const float TrickAttackWindowSeconds = 17f;

    /// <summary>RSR Mug/Dokumori debuff window (~19s).</summary>
    public const float MugWindowSeconds = 19f;

    public static bool IsInTrickAttackWindow(IActionService actionService, byte level)
    {
        var kunaiInWindow = level >= NINActions.KunaisBane.MinLevel
            && IsWithinWindow(actionService, NINActions.KunaisBane.ActionId, TrickAttackWindowSeconds);
        var trickInWindow = IsWithinWindow(actionService, NINActions.TrickAttack.ActionId, TrickAttackWindowSeconds);
        return kunaiInWindow || trickInWindow;
    }

    public static bool IsInMugWindow(IActionService actionService, byte level)
    {
        var mug = NINActions.GetMugAction(level, actionService);
        return IsWithinWindow(actionService, mug.ActionId, MugWindowSeconds);
    }

    internal static bool IsWithinWindow(IActionService actionService, uint actionId, float windowSeconds)
    {
        var elapsed = actionService.GetRecastTimeElapsed(actionId);
        return elapsed > 0f && elapsed < windowSeconds;
    }
}
