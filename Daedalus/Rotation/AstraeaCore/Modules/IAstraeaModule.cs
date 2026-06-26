using Daedalus.Rotation.AstraeaCore.Context;
using Daedalus.Rotation.Common;

namespace Daedalus.Rotation.AstraeaCore.Modules;

/// <summary>
/// Interface for Astraea (Astrologian) rotation modules.
/// Inherits from IHealerRotationModule for consistent module patterns across healers.
/// </summary>
public interface IAstraeaModule : IHealerRotationModule<IAstraeaContext>
{
}
