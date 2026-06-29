using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Daedalus.Data;
using Daedalus.Services.Debug;

namespace Daedalus.Windows;

/// <summary>
/// "Missing" window — scans the current job's expected abilities and flags any that are level-met but
/// not actually usable (uncompleted job quests). Auto-updates with whatever job you're on. Catches the
/// case where the rotation silently can't cast something (e.g. Reaper Enshroud before the Lv80 quest).
/// </summary>
public sealed class MissingWindow : Window
{
    private static readonly Vector4 _red = new(1.0f, 0.4f, 0.4f, 1.0f);
    private static readonly Vector4 _green = new(0.4f, 1.0f, 0.4f, 1.0f);
    private static readonly Vector4 _dim = new(0.6f, 0.6f, 0.6f, 1.0f);

    private readonly DebugService debugService;

    public MissingWindow(DebugService debugService)
        : base("Missing", ImGuiWindowFlags.NoCollapse)
    {
        this.debugService = debugService;
        Size = new Vector2(360, 420);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var jobId = debugService.GetJobId();
        var level = debugService.GetPlayerLevel();

        if (jobId == 0)
        {
            ImGui.TextColored(_dim, "Not logged in.");
            return;
        }

        var jobName = JobRegistry.GetJobName(jobId);
        ImGui.Text($"Job: {jobName} (Lv.{level})");
        ImGui.Separator();

        var statuses = debugService.GetAbilityUnlockStatus();
        if (statuses.Count == 0)
        {
            ImGui.TextColored(_dim, "No ability list for this job.");
            return;
        }

        var missing = statuses.Where(s => !s.Learned).ToList();

        // Missing first — the actionable part.
        if (missing.Count == 0)
        {
            ImGui.TextColored(_green, $"All {statuses.Count} abilities unlocked for Lv.{level}.");
        }
        else
        {
            ImGui.TextColored(_red, $"{missing.Count} ability(ies) level-met but NOT unlocked:");
            ImGui.TextWrapped("These are almost always uncompleted job quests — finish the job quest to enable them.");
            ImGui.Spacing();
            foreach (var s in missing.OrderBy(s => s.MinLevel))
            {
                ImGui.TextColored(_red, "✗");
                ImGui.SameLine();
                ImGui.Text($"{s.Name}  (Lv.{s.MinLevel})");
            }
        }

        ImGui.Spacing();
        ImGui.Separator();

        // Full list (collapsed by default) for reference.
        if (ImGui.CollapsingHeader($"All expected abilities ({statuses.Count})"))
        {
            foreach (var s in statuses.OrderBy(s => s.MinLevel))
            {
                if (s.Learned)
                {
                    ImGui.TextColored(_green, "✔");
                    ImGui.SameLine();
                    ImGui.TextColored(_dim, $"{s.Name}  (Lv.{s.MinLevel})");
                }
                else
                {
                    ImGui.TextColored(_red, "✗");
                    ImGui.SameLine();
                    ImGui.Text($"{s.Name}  (Lv.{s.MinLevel})");
                }
            }
        }
    }
}
