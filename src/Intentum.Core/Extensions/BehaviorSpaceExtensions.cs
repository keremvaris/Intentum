using Intentum.Core.Behavior;

namespace Intentum.Core.Extensions;

public static class BehaviorSpaceExtensions
{
    public static void EnsureNotEmpty(this BehaviorSpace space)
    {
        ArgumentNullException.ThrowIfNull(space);

        if (space.Events.Count == 0)
            throw new ArgumentException("BehaviorSpace must contain at least one event for inference.", nameof(space));
    }
}
