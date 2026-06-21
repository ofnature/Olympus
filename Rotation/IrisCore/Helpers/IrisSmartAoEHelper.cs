using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Olympus.Data;
using Olympus.Rotation.IrisCore.Context;
using Olympus.Services;
using Olympus.Services.Targeting;
namespace Olympus.Rotation.IrisCore.Helpers;

/// <summary>
/// Smart AoE target resolution for scheduler-driven PCT GCD pushes.
/// </summary>
public static class IrisSmartAoEHelper
{
    public static ulong ResolveGcdTargetId(
        ISmartAoEService? smartAoEService,
        IIrisContext context,
        IBattleChara defaultTarget,
        uint actionId,
        bool useAoeBranch)
    {
        if (!useAoeBranch || smartAoEService is null)
            return defaultTarget.GameObjectId;

        var minTargets = context.Configuration.Pictomancer.AoEMinTargets;
        if (context.NearbyEnemyCount < minTargets)
            return defaultTarget.GameObjectId;

        var result = smartAoEService.FindBestAoETarget(
            actionId,
            FFXIVConstants.CasterTargetingRange,
            context.Player,
            recordPrediction: true);

        return result.Target?.GameObjectId ?? defaultTarget.GameObjectId;
    }

    /// <summary>
    /// Refines raw enemy count using Smart AoE hit prediction for the primary AoE filler.
    /// Never promotes single-target packs into AoE mode on smart prediction alone.
    /// </summary>
    public static int RefineEnemyCountForAoE(
        ISmartAoEService? smartAoEService,
        IIrisContext context,
        int rawEnemyCount)
        => RefineEnemyCountForAoE(smartAoEService, context.Player, context.Configuration, rawEnemyCount);

    public static int RefineEnemyCountForAoE(
        ISmartAoEService? smartAoEService,
        IPlayerCharacter player,
        Configuration configuration,
        int rawEnemyCount)
    {
        if (smartAoEService is null || !configuration.Pictomancer.EnableAoERotation)
            return rawEnemyCount;

        if (player.Level < PCTActions.Fire2InRed.MinLevel)
            return rawEnemyCount;

        // Only consult smart prediction when the raw pack is near breakeven.
        // Avoids promoting a lone target into AoE when smart geometry over-counts.
        var minTargets = configuration.Pictomancer.AoEMinTargets;
        if (rawEnemyCount + 1 < minTargets)
            return rawEnemyCount;

        var smart = smartAoEService.FindBestAoETarget(
            PCTActions.Fire2InRed.ActionId,
            FFXIVConstants.CasterTargetingRange,
            player,
            recordPrediction: false);

        return smart.HitCount > rawEnemyCount ? smart.HitCount : rawEnemyCount;
    }
}
