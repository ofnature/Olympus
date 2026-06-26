using System;
using Dalamud.Bindings.ImGui;
using Daedalus.Config;
using Daedalus.Data;
using Daedalus.Localization;

namespace Daedalus.Windows.Config.Tanks;

internal static class TankAoEConfigHelper
{
    public static void DrawAoESettings(
        Configuration config,
        uint jobId,
        Func<int?> getOverride,
        Action<int?> setOverride,
        Action save)
    {
        ConfigUIHelpers.Toggle(
            Loc.T(LocalizedStrings.Tank.EnableAoEDamage, "Enable AoE Damage"),
            () => config.Tank.EnableAoEDamage,
            v => config.Tank.EnableAoEDamage = v,
            Loc.T(LocalizedStrings.Tank.EnableAoEDamageDesc, "Use AoE combos when enough enemies are in range."),
            save);

        if (!config.Tank.EnableAoEDamage)
            return;

        config.Tank.AoEMinTargets = ConfigUIHelpers.IntSlider(
            Loc.T(LocalizedStrings.Tank.AoEMinTargets, "Global AoE Min Targets"),
            config.Tank.AoEMinTargets, 2, 8,
            Loc.T(LocalizedStrings.Tank.AoEMinTargetsDesc, "Default minimum enemies for AoE rotation (all tanks)."),
            save, v => config.Tank.AoEMinTargets = v);

        ConfigUIHelpers.Spacing();

        ConfigUIHelpers.Toggle(
            Loc.T("config.job.tank.aoe_override", "Use custom min targets"),
            () => getOverride().HasValue,
            v =>
            {
                if (v)
                    setOverride(getOverride() ?? config.Tank.AoEMinTargets);
                else
                    setOverride(null);
                save();
            },
            Loc.T("config.job.tank.aoe_override_desc", "Override the global default for this job only."),
            save);

        if (getOverride() is int overrideValue)
        {
            ConfigUIHelpers.IntSlider(
                Loc.T(LocalizedStrings.Tank.AoEMinTargets, "AoE Min Targets"),
                overrideValue, 2, 8,
                Loc.T(LocalizedStrings.Tank.AoEMinTargetsDesc, "Minimum enemies for AoE rotation."),
                save, v => setOverride(v));
        }
        else
        {
            ImGui.TextDisabled(Loc.TFormat(
                "config.job.tank.inherit_global_aoe",
                "Inheriting global default ({0} targets).",
                config.Tank.AoEMinTargets));
        }

        ImGui.TextDisabled(Loc.TFormat(
            "config.job.tank.effective_min_targets",
            "Effective min targets: {0}",
            config.Tank.GetEffectiveAoEMinTargets(jobId)));
    }
}
