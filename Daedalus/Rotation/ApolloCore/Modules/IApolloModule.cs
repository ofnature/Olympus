using Daedalus.Rotation.ApolloCore.Context;
using Daedalus.Rotation.Common;

namespace Daedalus.Rotation.ApolloCore.Modules;

/// <summary>
/// Interface for Apollo (White Mage) rotation modules.
/// Inherits from IHealerRotationModule for consistent module patterns across healers.
/// </summary>
public interface IApolloModule : IHealerRotationModule<IApolloContext>
{
}
