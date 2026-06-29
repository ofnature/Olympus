using System.Collections.Generic;
using System.Globalization;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;

namespace Daedalus.Services.Positional.Navigation;

/// <summary>
/// Auto-manages BossMod Reborn's AI movement config by role — for group content, where AutoDuty (which
/// only runs in Trust) isn't there to do it. When enabled, it pushes role-based <c>MaxDistanceToTarget</c>
/// and a live <c>DesiredPositional</c> into BMR via the <c>BossMod.Configuration</c> IPC, and puts BMR in
/// movement-only mode (<c>ForbidActions</c>/<c>ManualTarget</c>) so BMR positions while Daedalus keeps the
/// rotation + targeting. You still enable BMR AI yourself (<c>/bmrai</c>); this just feeds it the numbers.
///
/// Conservative: changes are transient (save=false, reset when BMR reloads), only pushed on change, and
/// fail-open — if BMR isn't loaded nothing happens. Does NOT enable the AI (no IPC for that).
/// </summary>
public sealed class BmrAiConfigService
{
    private readonly IDalamudPluginInterface _pi;
    private readonly IBossModSafetyService _bmr;
    private readonly IPluginLog? _log;

    private ICallGateSubscriber<List<string>, bool, List<string>>? _configIpc;

    // Throttle: only push a field when its value changes, and never faster than this (belt-and-suspenders
    // against a value oscillating per-frame). 0.25s is well inside a GCD, so no responsiveness loss.
    private const double MinPushIntervalSeconds = 0.25;
    private float? _lastDistance;
    private string? _lastPositional;
    private System.DateTime _lastPushUtc = System.DateTime.MinValue;
    private bool _movementOnlyApplied;
    private bool _wasEnabled;
    private string? _savedAiPreset;

    private ICallGateSubscriber<string>? _getPresetIpc;
    private ICallGateSubscriber<string, object>? _setPresetIpc;

    public BmrAiConfigService(IDalamudPluginInterface pi, IBossModSafetyService bmr, IPluginLog? log = null)
    {
        _pi = pi;
        _bmr = bmr;
        _log = log;
    }

    public readonly record struct Request(
        bool Enabled,
        uint JobId,
        PositionalType? RequiredPositional,
        float RangedStandDistance);

    public void Update(in Request req)
    {
        if (!req.Enabled)
        {
            if (_wasEnabled)
                RestoreAndReset();
            return;
        }

        if (!_bmr.IsAvailable)
            return;

        EnsureSubscriber();
        _wasEnabled = true;

        // One-time setup per enable session. BMR's simple follow-target movement (which honors our
        // MaxDistanceToTarget/DesiredPositional) ONLY runs when no AI preset is loaded — a loaded preset
        // does its own positioning and ignores our config. So clear the AI preset (saving it to restore on
        // disable) and put BMR in movement-only mode: it positions, Daedalus fights + targets.
        if (!_movementOnlyApplied)
        {
            _savedAiPreset = TryGetPreset();
            TrySetPreset(string.Empty); // unknown name → SetAIPreset(null) → AIPreset == null
            PushConfig("ForbidActions", "true");
            PushConfig("ManualTarget", "true");
            PushConfig("FollowTarget", "true");
            _movementOnlyApplied = true;
        }

        // Rate cap: nothing changes value faster than a GCD, so a sub-0.25s push means something is
        // oscillating — skip this frame (the still-changed value pushes on the next eligible frame).
        var now = System.DateTime.UtcNow;
        if ((now - _lastPushUtc).TotalSeconds < MinPushIntervalSeconds)
            return;

        var pushed = false;

        var distance = BmrAiConfigPolicy.ResolveMaxDistance(req.JobId, req.RangedStandDistance);
        if (_lastDistance != distance)
        {
            PushConfig("MaxDistanceToTarget", distance.ToString("0.0", CultureInfo.InvariantCulture));
            _lastDistance = distance;
            pushed = true;
        }

        var positional = BmrAiConfigPolicy.ResolveDesiredPositional(req.JobId, req.RequiredPositional);
        if (_lastPositional != positional)
        {
            PushConfig("DesiredPositional", positional);
            _lastPositional = positional;
            pushed = true;
        }

        if (pushed)
            _lastPushUtc = now;
    }

    /// <summary>On disable, hand control back to BMR (drop movement-only) and clear the throttle cache.</summary>
    private void RestoreAndReset()
    {
        if (_bmr.IsAvailable && _movementOnlyApplied)
        {
            PushConfig("ForbidActions", "false");
            PushConfig("ManualTarget", "false");
            // Restore the AI preset we cleared on enable (non-destructive).
            if (!string.IsNullOrEmpty(_savedAiPreset))
                TrySetPreset(_savedAiPreset);
        }
        _savedAiPreset = null;
        _lastDistance = null;
        _lastPositional = null;
        _lastPushUtc = System.DateTime.MinValue;
        _movementOnlyApplied = false;
        _wasEnabled = false;
    }

    private void PushConfig(string field, string value)
    {
        try
        {
            _configIpc?.InvokeFunc(new List<string> { "AIConfig", field, value }, false);
        }
        catch (System.Exception ex)
        {
            _log?.Debug(ex, "[BmrAiConfigService] Failed to push AIConfig.{Field}={Value}", field, value);
        }
    }

    private string? TryGetPreset()
    {
        try { return _getPresetIpc?.InvokeFunc(); }
        catch (System.Exception ex) { _log?.Debug(ex, "[BmrAiConfigService] AI.GetPreset failed"); return null; }
    }

    private void TrySetPreset(string name)
    {
        try { _setPresetIpc?.InvokeAction(name); }
        catch (System.Exception ex) { _log?.Debug(ex, "[BmrAiConfigService] AI.SetPreset('{Name}') failed", name); }
    }

    private void EnsureSubscriber()
    {
        _configIpc ??= _pi.GetIpcSubscriber<List<string>, bool, List<string>>("BossMod.Configuration");
        _getPresetIpc ??= _pi.GetIpcSubscriber<string>("BossMod.AI.GetPreset");
        _setPresetIpc ??= _pi.GetIpcSubscriber<string, object>("BossMod.AI.SetPreset");
    }
}
