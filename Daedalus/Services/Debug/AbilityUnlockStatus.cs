namespace Daedalus.Services.Debug;

/// <summary>
/// Unlock status of one ability the current job's rotation expects at the current level.
/// <see cref="Learned"/> == false means the action is level-met but not actually usable — almost
/// always an uncompleted job quest (common when leveling via AutoDuty).
/// </summary>
public readonly record struct AbilityUnlockStatus(string Name, byte MinLevel, uint ActionId, bool Learned);
