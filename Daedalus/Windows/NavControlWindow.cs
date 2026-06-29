using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Daedalus.Windows.Config;

namespace Daedalus.Windows;

/// <summary>
/// Global navigation control panel. All vNav / max-melee tuning lives here in one place (not per-job):
/// the vNav Flex grace band, solo position lock, debug rings, and the tank-feature stubs.
/// </summary>
public sealed class NavControlWindow : Window
{
    private readonly Configuration configuration;
    private readonly Action saveConfiguration;

    public NavControlWindow(Configuration configuration, Action saveConfiguration)
        : base("Nav Control", ImGuiWindowFlags.NoCollapse)
    {
        this.configuration = configuration;
        this.saveConfiguration = saveConfiguration;

        Size = new Vector2(340, 280);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var nav = configuration.Nav;

        ImGui.TextDisabled("Max Melee Positioning");
        ImGui.Separator();

        nav.VNavFlex = ConfigUIHelpers.FloatSlider(
            "vNav Flex (yalms)",
            nav.VNavFlex,
            0.0f,
            2.0f,
            "%.1f",
            "Grace dead-band around max melee before vNav is called to reposition. Larger = fewer, lazier "
            + "moves (less twitching); smaller = tighter range-keeping.",
            saveConfiguration);

        var soloLock = nav.SoloPositionLock;
        if (ConfigUIHelpers.ToggleCheckbox(
                "Solo Position Lock",
                ref soloLock,
                "Disable max-melee positioning when solo (in a solo duty or with no party members).",
                saveConfiguration))
        {
            nav.SoloPositionLock = soloLock;
        }

        var rings = nav.MaxMeleeDebugRings;
        if (ConfigUIHelpers.ToggleCheckbox(
                "Max Melee Debug Rings",
                ref rings,
                "Draw the enemy-hitbox / combined / max-melee rings (and the vNav Flex grace band) around "
                + "the current target.",
                saveConfiguration))
        {
            nav.MaxMeleeDebugRings = rings;
        }

        ImGui.Spacing();
        ImGui.TextDisabled("Auto-Manage BossMod AI (groups — experimental)");
        ImGui.Separator();

        var autoBmr = nav.AutoManageBmrAi;
        if (ConfigUIHelpers.ToggleCheckbox(
                "Auto-Manage BMR AI by role",
                ref autoBmr,
                "For group content (not Trust): feeds BossMod Reborn's AI a role-based stand distance "
                + "(healers/ranged hold at range, melee hug) and the live next-GCD positional, in movement-only "
                + "mode so BMR positions while Daedalus keeps the rotation. You still enable BMR AI yourself "
                + "(/bmrai). Does nothing if BossMod Reborn isn't loaded. Off by default.",
                saveConfiguration))
        {
            nav.AutoManageBmrAi = autoBmr;
        }

        if (nav.AutoManageBmrAi)
        {
            nav.BmrRangedStandDistance = ConfigUIHelpers.FloatSlider(
                "Ranged Stand Distance (yalms)",
                nav.BmrRangedStandDistance,
                8f,
                24f,
                "%.0f",
                "How far healers/ranged/casters stand from the target. 15y is inside cast range but out of "
                + "most melee/AoE. Melee always hug (2.6y).",
                saveConfiguration);
        }

        ImGui.Spacing();
        ImGui.TextDisabled("Tank (experimental)");
        ImGui.Separator();

        var addPull = nav.AddPull;
        if (ConfigUIHelpers.ToggleCheckbox(
                "Add Pull",
                ref addPull,
                "Ranged-pull adds to the camp. Stub \u2014 the toggle is saved but the behavior is not wired yet.",
                saveConfiguration))
        {
            nav.AddPull = addPull;
        }

        var tankMode = nav.TankMode;
        if (ConfigUIHelpers.ToggleCheckbox(
                "Tank Mode",
                ref tankMode,
                "Toggle boss-anchor vs add-puller mode. Stub \u2014 the toggle is saved but the behavior is not "
                + "wired yet.",
                saveConfiguration))
        {
            nav.TankMode = tankMode;
        }
    }
}
