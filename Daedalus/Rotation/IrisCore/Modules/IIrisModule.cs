using Daedalus.Rotation.Common;
using Daedalus.Rotation.IrisCore.Context;

namespace Daedalus.Rotation.IrisCore.Modules;

/// <summary>
/// Interface for Iris rotation modules.
/// Extends the base rotation module with Pictomancer-specific context.
/// </summary>
public interface IIrisModule : IRotationModule<IIrisContext>
{
}
