using Daedalus.Rotation.Common;
using Daedalus.Rotation.HecateCore.Context;

namespace Daedalus.Rotation.HecateCore.Modules;

/// <summary>
/// Interface for Black Mage rotation modules.
/// Each module handles a specific aspect of the rotation (buffs, damage, etc.).
/// </summary>
public interface IHecateModule : IRotationModule<IHecateContext>
{
    // Inherits from IRotationModule<IHecateContext>:
    // - int Priority { get; }
    // - string Name { get; }
    // - bool TryExecute(IHecateContext context, bool isMoving);
    // - void UpdateDebugState(IHecateContext context);
}
