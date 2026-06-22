using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Olympus.Config;
using Olympus.Data;
using Olympus.Localization;
using Olympus.Rotation;
using Olympus.Windows.Config;

namespace Olympus.Windows;

/// <summary>
/// Quick-access control panel (styled like the main window) for toggling auto movement on the fly.
/// Exposes the global master kill-switch plus contextual per-job movement toggles for the active job.
/// </summary>
public sealed class ControlWindow : Window
{
    private readonly Configuration configuration;
    private readonly Action saveConfiguration;
    private readonly RotationManager rotationManager;
    private readonly ITextureProvider textureProvider;

    public ControlWindow(
        Configuration configuration,
        Action saveConfiguration,
        RotationManager rotationManager,
        ITextureProvider textureProvider)
        : base("Olympus Control", ImGuiWindowFlags.NoCollapse)
    {
        this.configuration = configuration;
        this.saveConfiguration = saveConfiguration;
        this.rotationManager = rotationManager;
        this.textureProvider = textureProvider;

        Size = new Vector2(280, 220);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        DrawActiveRotationHeader();

        ImGui.Separator();

        ImGui.TextDisabled("Auto Movement");

        var autoMovement = configuration.EnableAutoMovement;
        if (ConfigUIHelpers.ToggleCheckbox(
                "Enable Auto Movement",
                ref autoMovement,
                "Master switch for all vNav movement (flank/rear repositioning, burst approach). "
                + "Movement is always off when solo (no party).",
                saveConfiguration))
        {
            configuration.EnableAutoMovement = autoMovement;
        }

        ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1f), "Solo (no party): auto movement is always disabled.");

        ImGui.Separator();

        DrawJobMovementToggles();
    }

    private void DrawActiveRotationHeader()
    {
        var statusColor = configuration.Enabled
            ? new Vector4(0.0f, 1.0f, 0.0f, 1.0f)
            : new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        var statusText = configuration.Enabled
            ? Loc.T(LocalizedStrings.Main.Active, "ACTIVE")
            : Loc.T(LocalizedStrings.Main.Inactive, "INACTIVE");

        ImGui.TextColored(statusColor, statusText);

        var activeRotation = rotationManager.ActiveRotation;
        if (activeRotation != null)
        {
            var activeJobId = activeRotation.SupportedJobIds[0];
            var jobName = JobRegistry.GetJobName(activeJobId);

            var iconId = JobRegistry.GetJobIconId(activeJobId);
            if (iconId != 0)
            {
                var wrap = textureProvider.GetFromGameIcon(new GameIconLookup(iconId)).GetWrapOrEmpty();
                ImGui.Image(wrap.Handle, new Vector2(20, 20));
                ImGui.SameLine();
            }

            ImGui.TextColored(ConfigUIHelpers.AccentBlue, $"{activeRotation.Name} ({jobName})");
        }
        else
        {
            ImGui.TextDisabled(Loc.T(LocalizedStrings.Main.SwitchToSupported, "No rotation \u2014 switch to a supported job"));
        }
    }

    private void DrawJobMovementToggles()
    {
        var activeRotation = rotationManager.ActiveRotation;
        if (activeRotation == null)
        {
            ImGui.TextDisabled("No active job.");
            return;
        }

        var jobId = activeRotation.SupportedJobIds[0];
        var jobName = JobRegistry.GetJobName(jobId);

        ImGui.TextDisabled($"{jobName} Movement");

        // Per-job toggles only make sense while the master switch is on.
        ImGui.BeginDisabled(!configuration.EnableAutoMovement);

        switch (jobId)
        {
            case JobRegistry.Ninja:
            case JobRegistry.Rogue:
            {
                var positional = configuration.Ninja.EnablePositionalMovement;
                if (ConfigUIHelpers.ToggleCheckbox(
                        "Positional Reposition",
                        ref positional,
                        "Use vNav to move to the flank/rear before Aeolian Edge / Armor Crush.",
                        saveConfiguration))
                {
                    configuration.Ninja.EnablePositionalMovement = positional;
                }

                var approach = configuration.Ninja.EnableBurstMeleeApproach;
                if (ConfigUIHelpers.ToggleCheckbox(
                        "Burst Melee Approach",
                        ref approach,
                        "Move into melee range during burst prep (Suiton + Kunai's Bane ready).",
                        saveConfiguration))
                {
                    configuration.Ninja.EnableBurstMeleeApproach = approach;
                }

                break;
            }

            case JobRegistry.Samurai:
            {
                var positional = configuration.Samurai.EnablePositionalMovement;
                if (ConfigUIHelpers.ToggleCheckbox(
                        "Positional Reposition",
                        ref positional,
                        "Use vNav to move to the flank/rear before Gekko / Kasha.",
                        saveConfiguration))
                {
                    configuration.Samurai.EnablePositionalMovement = positional;
                }

                break;
            }

            default:
                ImGui.TextDisabled("No quick movement options for this job.");
                break;
        }

        ImGui.EndDisabled();
    }
}
