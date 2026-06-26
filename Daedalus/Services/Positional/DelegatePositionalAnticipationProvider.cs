using System;
using Daedalus.Services.Positional.Navigation;

namespace Daedalus.Services.Positional;

/// <summary>
/// Generic positional anticipation provider that delegates to a callback.
/// Used by melee jobs (MNK, DRG, VPR, RPR) whose <c>GetNextRequiredPositional</c>
/// already predicts the upcoming positional from game state.
/// </summary>
public sealed class DelegatePositionalAnticipationProvider : IPositionalAnticipationProvider
{
    private readonly Func<PositionalType?> _getNextPositional;

    public DelegatePositionalAnticipationProvider(Func<PositionalType?> getNextPositional)
    {
        _getNextPositional = getNextPositional;
    }

    public PositionalAnticipation? GetAnticipatedPositional(in PositionalAnticipationContext context)
    {
        if (context.TargetHasPositionalImmunity || context.HasTrueNorth)
            return null;

        var positional = _getNextPositional();
        if (positional is null)
            return null;

        return new PositionalAnticipation(
            positional.Value,
            0,
            PositionalAnticipationReason.ComboSetup);
    }
}
