using System;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using Olympus.Data;
using Olympus.Rotation.Common.Helpers;
using Olympus.Rotation.HermesCore.Context;
using Olympus.Services;
using Olympus.Services.Training;

namespace Olympus.Rotation.HermesCore.Helpers;

/// <summary>
/// RSR DoTenChiJin — Track 1 only (Phase R1).
/// </summary>
public sealed class HermesNinjutsuExecutor : IHermesNinjutsuExecutor
{
    public bool IsTcjStepPending { get; private set; }

    public void ResetTcjTrack()
    {
        IsTcjStepPending = false;
    }

    public unsafe bool TryExecuteTenChiJin(IHermesContext context, IBattleChara? target, int enemyCount)
    {
        IsTcjStepPending = false;

        if (!context.Configuration.Ninja.EnableTenChiJin || !context.HasTenChiJin)
        {
            ClearTcjDebugProbe(context);
            return false;
        }

        var actionManager = SafeGameAccess.GetActionManager(null);
        uint tenAdj;
        uint chiAdj;
        uint jinAdj;
        if (actionManager != null)
        {
            tenAdj = actionManager->GetAdjustedActionId(NINActions.Ten.ActionId);
            chiAdj = actionManager->GetAdjustedActionId(NINActions.Chi.ActionId);
            jinAdj = actionManager->GetAdjustedActionId(NINActions.Jin.ActionId);
        }
        else
        {
            tenAdj = context.ActionService.GetAdjustedActionId(NINActions.Ten.ActionId);
            chiAdj = context.ActionService.GetAdjustedActionId(NINActions.Chi.ActionId);
            jinAdj = context.ActionService.GetAdjustedActionId(NINActions.Jin.ActionId);
        }
        var hasDotonActive = BaseStatusHelper.HasStatus(context.Player, NINActions.StatusIds.Doton);
        var wasLastAction = (uint id) => context.ActionService.WasLastAction(id);

        UpdateTcjDebugProbe(
            context, tenAdj, chiAdj, jinAdj, enemyCount, hasDotonActive, wasLastAction);

        if (!HermesTenChiJinHelper.TryGetNextStep(
                tenAdj, chiAdj, jinAdj,
                enemyCount, context.Configuration.Ninja.AoEMinTargets,
                hasDotonActive, wasLastAction, out var step))
        {
            context.Debug.NinjutsuState = "TCJ: waiting for slot advance";
            return false;
        }

        if (!context.ActionService.CanExecuteActionId(step.AdjustedNinjutsuId))
        {
            IsTcjStepPending = true;
            context.Debug.NinjutsuState = $"TCJ: Waiting for {step.DisplayAction.Name}";
            return false;
        }

        var targetId = target?.GameObjectId ?? context.Player.GameObjectId;
        if (actionManager == null)
            return false;

        if (!actionManager->UseAction(ActionType.Action, step.AdjustedNinjutsuId, targetId))
        {
            IsTcjStepPending = true;
            context.Debug.NinjutsuState = $"TCJ: Waiting for {step.DisplayAction.Name}";
            return false;
        }

        context.ActionService.NotifyActionExecuted(step.DisplayAction, step.AdjustedNinjutsuId);
        context.Debug.PlannedAction = step.DisplayAction.Name;
        context.Debug.NinjutsuState = step.DebugName;

        var tcjConceptId = step.AdjustedNinjutsuId switch
        {
            NINActions.TenChiJinAdjusted.Suiton => NinConcepts.Suiton,
            NINActions.TenChiJinAdjusted.Doton => NinConcepts.AoeNinjutsu,
            _ => NinConcepts.TcjOptimization,
        };
        TrainingHelper.Decision(context.TrainingService)
            .Action(step.DisplayAction.ActionId, step.DisplayAction.Name)
            .AsMeleeBurst()
            .Target(target?.Name?.TextValue ?? "Target")
            .Reason($"TCJ: {step.DisplayAction.Name}",
                "Ten Chi Jin lets you use three Ninjutsu instantly. Standard sequence: Fuma Shuriken (Ten) → " +
                "Raiton/Katon (Chi) → Suiton/Doton (Jin).")
            .Factors(new[] { "TCJ active", step.DebugName, $"{enemyCount} enemies nearby" })
            .Alternatives(new[] { "Cannot deviate — TCJ sequences are locked in order" })
            .Tip("Complete all three steps within the 6s buff window.")
            .Concept(tcjConceptId)
            .Record();
        context.TrainingService?.RecordConceptApplication(tcjConceptId, true, step.DebugName);
        return true;
    }

    internal static void ClearTcjDebugProbe(IHermesContext context)
    {
        context.Debug.TcjTenAdjustedId = 0;
        context.Debug.TcjChiAdjustedId = 0;
        context.Debug.TcjJinAdjustedId = 0;
        context.Debug.TcjTryGetNextStepResult = "";
    }

    private static void UpdateTcjDebugProbe(
        IHermesContext context,
        uint tenAdj, uint chiAdj, uint jinAdj,
        int enemyCount, bool hasDotonActive,
        Func<uint, bool> wasLastAction)
    {
        context.Debug.TcjTenAdjustedId = tenAdj;
        context.Debug.TcjChiAdjustedId = chiAdj;
        context.Debug.TcjJinAdjustedId = jinAdj;

        if (HermesTenChiJinHelper.TryGetNextStep(
                tenAdj, chiAdj, jinAdj,
                enemyCount, context.Configuration.Ninja.AoEMinTargets,
                hasDotonActive, wasLastAction, out var step))
        {
            context.Debug.TcjTryGetNextStepResult =
                $"true → {step.DebugName} (adj {step.AdjustedNinjutsuId})";
        }
        else
        {
            context.Debug.TcjTryGetNextStepResult = "false (waiting for slot advance)";
        }
    }
}
