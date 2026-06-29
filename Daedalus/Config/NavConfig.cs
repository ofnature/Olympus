using System;

namespace Daedalus.Config;

/// <summary>
/// Global navigation / vNav tuning, surfaced in the Nav Control window. Applies across all jobs
/// (not per-job) and persists with the rest of the plugin configuration.
/// </summary>
public sealed class NavConfig
{
    private float _vNavFlex = 0.5f;

    /// <summary>
    /// "Grace" dead-band (yalms) around the max-melee stand distance before vNav is called to correct
    /// position. The character only repaths when its distance to the target leaves the
    /// <c>[standDistance − VNavFlex, standDistance + VNavFlex]</c> band — inside it the vNav call is
    /// suppressed entirely, which is what stops the move-in/move-out twitching. Range 0.0–2.0, default 0.5.
    /// </summary>
    public float VNavFlex
    {
        get => _vNavFlex;
        set => _vNavFlex = Math.Clamp(value, 0.0f, 2.0f);
    }

    /// <summary>
    /// Disable max-melee positioning while solo (in a solo duty or with no party members present).
    /// Default false.
    /// </summary>
    public bool SoloPositionLock { get; set; } = false;

    /// <summary>
    /// Draw the max-melee debug rings (enemy hitbox / combined / max-melee + grace band) around the
    /// current target. Default false.
    /// </summary>
    public bool MaxMeleeDebugRings { get; set; } = false;

    /// <summary>
    /// Tank feature: ranged-pull adds to the camp. Behavior is stubbed for now; this only persists the
    /// toggle. Default false.
    /// </summary>
    public bool AddPull { get; set; } = false;

    /// <summary>
    /// Tank feature: toggle between boss-anchor mode and add-puller mode. Behavior is stubbed for now;
    /// this only persists the toggle. Default false.
    /// </summary>
    public bool TankMode { get; set; } = false;

    private float _bmrRangedStandDistance = 15f;

    /// <summary>
    /// Auto-manage BossMod Reborn's AI movement config by role (for group content, where AutoDuty isn't
    /// running its own BMR management). When on and BMR is loaded, Daedalus feeds BMR a role-based stand
    /// distance + the live next-GCD positional and puts BMR in movement-only mode so it positions while
    /// Daedalus keeps the rotation. You still enable BMR AI yourself (<c>/bmrai</c>). Default OFF.
    /// </summary>
    public bool AutoManageBmrAi { get; set; } = false;

    /// <summary>
    /// Distance (yalms) backline jobs (healers/ranged/casters) stand from the target when auto-managing
    /// BMR AI. 15y sits inside the 25y cast range but out of most melee/AoE. Range 8–24, default 15.
    /// </summary>
    public float BmrRangedStandDistance
    {
        get => _bmrRangedStandDistance;
        set => _bmrRangedStandDistance = Math.Clamp(value, 8f, 24f);
    }
}
