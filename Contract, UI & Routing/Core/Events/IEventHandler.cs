namespace SoftwareCenter.Core.Events;

/// <summary>
/// Defines a handler for a specific event.
/// </summary>
/// <typeparam name="TEvent">The type of event to be handled.</typeparam>
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Handles the event.
    /// </summary>
    Task Handle(TEvent @event, CancellationToken cancellationToken);
}