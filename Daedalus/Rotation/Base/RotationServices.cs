using Dalamud.Plugin.Services;

namespace Daedalus.Rotation.Base;

/// <summary>
/// Static service references set by Plugin on init, available to all rotations without DI.
/// </summary>
public static class RotationServices
{
    public static ICondition? Condition { get; set; }
}
