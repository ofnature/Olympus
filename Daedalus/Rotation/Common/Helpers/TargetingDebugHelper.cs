using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Daedalus.Services.Targeting;

namespace Daedalus.Rotation.Common.Helpers;

/// <summary>
/// Shared formatting for combat target display in the debug window.
/// </summary>
public static class TargetingDebugHelper
{
    /// <summary>
    /// Builds the Overview tab <c>TargetInfo</c> string from the rotation context target
    /// and/or the player's selected enemy.
    /// </summary>
    public static string FormatTargetInfo(IBattleChara? currentTarget, ITargetingService targetingService)
        => currentTarget?.Name?.TextValue
           ?? targetingService.GetUserEnemyTarget()?.Name?.TextValue
           ?? "None";
}
