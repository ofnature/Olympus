using Daedalus.Rotation.AthenaCore.Context;
using Daedalus.Rotation.Common;

namespace Daedalus.Rotation.AthenaCore.Modules;

/// <summary>
/// Interface for Athena (Scholar) rotation modules.
/// Inherits from IHealerRotationModule for consistent module patterns across healers.
/// </summary>
public interface IAthenaModule : IHealerRotationModule<IAthenaContext>
{
}
