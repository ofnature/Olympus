namespace Olympus.Services.Action;

/// <summary>
/// Minimal last-action memory: the most recent successfully dispatched GCD and oGCD ids.
/// Solves oGCD sequencing cases like NIN Trick Attack after Mug. Intentionally NOT a full
/// action log or replay buffer — only the single last GCD and last oGCD are retained.
/// </summary>
internal sealed class ActionHistory
{
    public uint LastGcdId { get; private set; }
    public uint LastOgcdId { get; private set; }
    public uint LastActionId { get; private set; }

    public void RecordGcd(uint actionId)
    {
        LastGcdId = actionId;
        LastActionId = actionId;
    }

    public void RecordOgcd(uint actionId)
    {
        LastOgcdId = actionId;
        LastActionId = actionId;
    }

    public void RecordAction(uint actionId) => LastActionId = actionId;

    public bool WasLastGcd(uint actionId) => actionId != 0 && LastGcdId == actionId;
    public bool WasLastOgcd(uint actionId) => actionId != 0 && LastOgcdId == actionId;
    public bool WasLastAction(uint actionId) => actionId != 0 && LastActionId == actionId;
}
