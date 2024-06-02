namespace Content.Shared.Interaction.Events;

/// <summary>
/// Event raised on an entity to determine if it can use a specified item for generic interactions.
/// </summary>
public sealed class UseAttemptEvent(EntityUid uid, EntityUid used) : CancellableEntityEventArgs
{
    public EntityUid Uid { get; } = uid;

    public EntityUid Used = used;
}

/// <summary>
/// Event raised on an item to determine if it can be used by a specified entity for generic interactions.
/// </summary>
public sealed class CanBeUsedAttemptEvent(EntityUid user, EntityUid used) : CancellableEntityEventArgs
{
    public EntityUid User { get; } = user;

    public EntityUid Used = used;
}

