namespace Intentum.Core.Behavior;

/// <summary>
/// Fluent builder for creating BehaviorSpace instances with a more readable API.
/// </summary>
public sealed class BehaviorSpaceBuilder
{
    private readonly BehaviorSpace _space = new();
    private string? _currentActor;

    /// <summary>
    /// Sets the current actor for subsequent actions.
    /// </summary>
    public BehaviorSpaceBuilder WithActor(string actor)
    {
        _currentActor = actor;
        return this;
    }

    /// <summary>
    /// Adds an action for the current actor.
    /// </summary>
    public BehaviorSpaceBuilder Action(string action)
    {
        if (_currentActor == null)
            throw new InvalidOperationException("Must call WithActor before Action");

        _space.Observe(new BehaviorEvent(
            _currentActor,
            action,
            DateTimeOffset.UtcNow));

        return this;
    }

    /// <summary>
    /// Adds an action for the current actor with a specific timestamp.
    /// </summary>
    public BehaviorSpaceBuilder Action(string action, DateTimeOffset occurredAt)
    {
        if (_currentActor == null)
            throw new InvalidOperationException("Must call WithActor before Action");

        _space.Observe(new BehaviorEvent(
            _currentActor,
            action,
            occurredAt));

        return this;
    }

    /// <summary>
    /// Adds an action for the current actor with metadata.
    /// </summary>
    public BehaviorSpaceBuilder Action(string action, IReadOnlyDictionary<string, object>? metadata)
    {
        if (_currentActor == null)
            throw new InvalidOperationException("Must call WithActor before Action");

        _space.Observe(new BehaviorEvent(
            _currentActor,
            action,
            DateTimeOffset.UtcNow,
            metadata));

        return this;
    }

    /// <summary>
    /// Adds an action for the current actor with timestamp and metadata.
    /// </summary>
    public BehaviorSpaceBuilder Action(string action, DateTimeOffset occurredAt, IReadOnlyDictionary<string, object>? metadata)
    {
        if (_currentActor == null)
            throw new InvalidOperationException("Must call WithActor before Action");

        _space.Observe(new BehaviorEvent(
            _currentActor,
            action,
            occurredAt,
            metadata));

        return this;
    }

    /// <summary>
    /// Adds a complete behavior event directly.
    /// </summary>
    public BehaviorSpaceBuilder Observe(BehaviorEvent behaviorEvent)
    {
        _space.Observe(behaviorEvent);
        return this;
    }

    /// <summary>
    /// Builds and returns the BehaviorSpace instance.
    /// </summary>
    public BehaviorSpace Build()
    {
        return _space;
    }
}
