using Daedalus.Rotation.AthenaCore.Context;
using Daedalus.Rotation.Common.Scheduling;

namespace Daedalus.Rotation.AthenaCore.Modules.Healing;

public interface IHealingHandler
{
    int Priority { get; }
    string Name { get; }
    void CollectCandidates(IAthenaContext context, RotationScheduler scheduler, bool isMoving);
}
