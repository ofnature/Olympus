using Daedalus.Data;
using Daedalus.Rotation.HermesCore.Helpers;
using Daedalus.Services.Positional.Navigation;

namespace Daedalus.Services.Positional;

/// <summary>
/// NIN finisher anticipation after Spinning Edge / Gust Slash (~one GCD ahead of Aeolian/Armor Crush).
/// </summary>
public sealed class NinjaPositionalAnticipationProvider : IPositionalAnticipationProvider
{
    public PositionalAnticipation? GetAnticipatedPositional(in PositionalAnticipationContext context)
    {
        if (context.TargetHasPositionalImmunity || context.HasTrueNorth)
            return null;

        var last = context.LastComboAction;
        if (last != NINActions.GustSlash.ActionId && last != NINActions.SpinningEdge.ActionId)
            return null;

        if (context.PlayerLevel < NINActions.AeolianEdge.MinLevel)
            return null;

        var positional = HermesKazematoiRules.GetFinisherPositional(context.Kazematoi);
        if (positional is null)
            return null;

        var finisherId = positional == PositionalType.Flank
            ? NINActions.ArmorCrush.ActionId
            : NINActions.AeolianEdge.ActionId;

        return new PositionalAnticipation(
            positional.Value,
            finisherId,
            PositionalAnticipationReason.ComboSetup);
    }
}
