using Daedalus.Rotation.Common;
using Daedalus.Services.Consumables;

namespace Daedalus.Rotation.Common.Modules;

/// <summary>
/// Pre-pull candidate that dispatches a combat tincture during the opener.
/// Delegates the gate evaluation and dispatch to <see cref="ITinctureDispatcher"/>.
/// </summary>
public sealed class TinctureCandidate : IPrePullCandidate
{
    private readonly ITinctureDispatcher _dispatcher;

    public TinctureCandidate(ITinctureDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public bool TryDispatch(uint jobId, IRotationContext context)
    {
        return _dispatcher.TryDispatch(jobId, context.InCombat, prePullPhase: true);
    }
}
