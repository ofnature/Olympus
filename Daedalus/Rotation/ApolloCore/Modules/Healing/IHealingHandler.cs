using Daedalus.Rotation.ApolloCore.Context;
using Daedalus.Rotation.Common.Scheduling;

namespace Daedalus.Rotation.ApolloCore.Modules.Healing;

/// <summary>
/// Interface for healing sub-handlers within the HealingModule.
/// Each handler is responsible for one healing priority and pushes
/// scheduler candidates instead of dispatching directly.
/// </summary>
public interface IHealingHandler
{
    /// <summary>
    /// Internal priority within HealingModule (lower = higher priority).
    /// </summary>
    HealingPriority Priority { get; }

    /// <summary>
    /// Display name for debugging.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Pushes scheduler candidates for this handler's healing actions.
    /// Replaces the legacy TryExecute method — gating is local to the handler,
    /// dispatch is centralized in the scheduler.
    /// </summary>
    void CollectCandidates(IApolloContext context, RotationScheduler scheduler, bool isMoving);
}
