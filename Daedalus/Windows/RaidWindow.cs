using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Daedalus.Config;
using Daedalus.Services.Content;
using Daedalus.Services.Targeting;
using Daedalus.Windows.Config;

namespace Daedalus.Windows;

/// <summary>
/// Per-fight strategy panel. Lets the user override targeting behavior for the specific duty they're
/// in (keyed by territory), so e.g. a split-boss fight can switch off unreachable targets while the
/// rest of the game uses the global settings. Overrides are applied non-destructively onto the
/// rotation's effective config (see DutyConfigurationService) — the global config is never mutated.
/// MVP scope: targeting only.
/// </summary>
public sealed class RaidWindow : Window
{
    private static readonly string[] StrategyNames =
        ["Lowest HP", "Highest HP", "Nearest", "Tank Assist", "Current Target", "Focus Target"];

    private static readonly string[] StrategyDescriptions =
    [
        "Target the enemy with the lowest HP (finish off weak enemies).",
        "Target the enemy with the highest HP (for cleave/AoE).",
        "Target the closest enemy.",
        "Attack what the party tank is targeting.",
        "Use your current hard target if valid.",
        "Use your focus target if valid.",
    ];

    private readonly Configuration configuration;
    private readonly Action saveConfiguration;
    private readonly IDutyContentService dutyContentService;

    public RaidWindow(Configuration configuration, Action saveConfiguration, IDutyContentService dutyContentService)
        : base("Raid", ImGuiWindowFlags.NoCollapse)
    {
        this.configuration = configuration;
        this.saveConfiguration = saveConfiguration;
        this.dutyContentService = dutyContentService;

        Size = new Vector2(360, 360);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var territory = dutyContentService.CurrentTerritoryType;

        if (territory == 0)
        {
            ImGui.TextDisabled("Not in a duty.");
            ImGui.TextWrapped(
                "Per-fight strategies apply inside instanced duties (dungeons, trials, raids). "
                + "Enter a duty to set a strategy for it.");
        }
        else
        {
            DrawCurrentFight(territory);
        }

        ImGui.Spacing();
        DrawSavedList();
    }

    private void DrawCurrentFight(uint territory)
    {
        var name = string.IsNullOrEmpty(dutyContentService.CurrentDutyName)
            ? $"Territory {territory}"
            : dutyContentService.CurrentDutyName;

        ImGui.Text("Current fight:");
        ImGui.SameLine();
        ImGui.TextColored(ConfigUIHelpers.AccentBlue, name);
        ImGui.TextDisabled(dutyContentService.DutyLabel);
        ImGui.Separator();

        var existing = configuration.Raid.GetTargeting(territory);
        var enabled = existing is { Enabled: true };
        if (ImGui.Checkbox("Use a custom strategy for this fight", ref enabled))
        {
            if (enabled)
            {
                var strat = existing ?? RaidTargetingStrategy.FromGlobal(configuration.Targeting);
                strat.Enabled = true;
                strat.DisplayName = name;
                configuration.Raid.TargetingByTerritory[territory] = strat;
            }
            else if (existing != null)
            {
                existing.Enabled = false;
            }

            saveConfiguration();
        }
        ImGui.TextDisabled(
            "Overrides the global targeting settings while you're in this duty.\nYour global settings are untouched.");

        if (!enabled)
            return;

        var strategy = configuration.Raid.TargetingByTerritory[territory];

        ConfigUIHelpers.BeginIndent();

        var strategyIndex = (int)strategy.EnemyStrategy;
        ImGui.SetNextItemWidth(200);
        if (ImGui.Combo("Enemy Strategy", ref strategyIndex, StrategyNames, StrategyNames.Length))
        {
            strategy.EnemyStrategy = (EnemyTargetingStrategy)strategyIndex;
            saveConfiguration();
        }
        ImGui.TextDisabled(StrategyDescriptions[strategyIndex]);

        ConfigUIHelpers.Spacing();

        ConfigUIHelpers.Toggle(
            "Switch off unreachable targets",
            () => strategy.RetargetUnreachableTarget,
            v => strategy.RetargetUnreachableTarget = v,
            "If your followed target is alive but out of reach (e.g. a boss split into an elevated "
            + "'upper' part melee can't hit and a grounded 'lower' part) and another enemy is in range, "
            + "switch to the reachable one instead of standing idle.",
            saveConfiguration);

        ConfigUIHelpers.Toggle(
            "Strict explicit-target mode",
            () => strategy.StrictCurrentTargetStrategy,
            v => strategy.StrictCurrentTargetStrategy = v,
            "When using Current Target or Focus Target strategy, never fall back to another enemy if "
            + "yours is gone.",
            saveConfiguration);

        ConfigUIHelpers.Toggle(
            "Skip invulnerable enemies",
            () => strategy.EnableInvulnerabilityFiltering,
            v => strategy.EnableInvulnerabilityFiltering = v,
            "Auto-targeting ignores enemies with invulnerability effects (phase transitions, immune "
            + "adds). Prevents wasting actions on targets that take no damage.",
            saveConfiguration);

        ConfigUIHelpers.EndIndent();
    }

    private void DrawSavedList()
    {
        var saved = configuration.Raid.TargetingByTerritory;
        if (saved.Count == 0)
            return;

        ImGui.Separator();
        ImGui.TextDisabled("Saved fight strategies");

        uint? toRemove = null;
        foreach (var (territory, strategy) in saved.OrderBy(kvp => kvp.Value.DisplayName, StringComparer.OrdinalIgnoreCase))
        {
            var label = string.IsNullOrEmpty(strategy.DisplayName) ? $"Territory {territory}" : strategy.DisplayName;
            var status = strategy.Enabled ? "on" : "off";

            if (ImGui.SmallButton($"X##raid{territory}"))
                toRemove = territory;

            ImGui.SameLine();
            ImGui.TextUnformatted($"{label}  ({strategy.EnemyStrategy}, {status})");
        }

        if (toRemove.HasValue)
        {
            saved.Remove(toRemove.Value);
            saveConfiguration();
        }
    }
}
