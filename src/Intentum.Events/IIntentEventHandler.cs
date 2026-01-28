namespace Intentum.Events;

/// <summary>
/// Handles intent-related events (e.g. dispatch to webhook, event bus).
/// </summary>
public interface IIntentEventHandler
{
    /// <summary>
    /// Handles an intent event (e.g. IntentInferred, PolicyDecisionChanged).
    /// </summary>
    /// <param name="payload">Event payload (intent, decision, behavior space id).</param>
    /// <param name="eventType">Type of event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(
        IntentEventPayload payload,
        IntentumEventType eventType,
        CancellationToken cancellationToken = default);
}
