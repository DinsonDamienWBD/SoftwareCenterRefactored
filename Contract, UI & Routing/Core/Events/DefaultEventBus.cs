using SoftwareCenter.Core.Events;
using SoftwareCenter.Core.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftwareCenter.Kernel.Events;

/// <summary>
/// A thread-safe, in-memory implementation of the IEventBus interface.
/// </summary>
public class DefaultEventBus : IEventBus
{
    // Using ConcurrentDictionary for thread-safe additions and removals of event names.
    // The value is a list of handlers. Access to this list must be synchronized.
    private readonly ConcurrentDictionary<string, List<Func<IEvent, Task>>> _subscriptions = new();
    private IKernelLogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultEventBus"/> class.
    /// </summary>
    /// <param name="logger">The kernel logger for logging event activity. Can be null initially.</param>
    public DefaultEventBus(IKernelLogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Sets the logger instance after construction to resolve circular dependencies.
    /// </summary>
    /// <param name="logger">The kernel logger.</param>
    public void SetLogger(IKernelLogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishAsync(IEvent systemEvent)
    {
        if (systemEvent == null) return;

        _logger?.Log($"Publishing event: {systemEvent.Name}");

        if (_subscriptions.TryGetValue(systemEvent.Name, out var handlers))
        {
            List<Func<IEvent, Task>> handlersSnapshot;
            lock (handlers)
            {
                // Take a snapshot to avoid issues with collection modification during iteration.
                handlersSnapshot = handlers.ToList();
            }

            // Execute handlers outside the lock to prevent deadlocks.
            foreach (var handler in handlersSnapshot)
            {
                // We don't want one failing handler to stop others.
                try
                {
                    await handler(systemEvent);
                }
                catch (Exception ex)
                {
                    _logger?.Log($"Event handler for '{systemEvent.Name}' failed: {ex.Message}");
                }
            }
        }
    }

    /// <inheritdoc />
    public void Subscribe(string eventName, Func<IEvent, Task> handler)
    {
        var handlers = _subscriptions.GetOrAdd(eventName, _ => new List<Func<IEvent, Task>>());
        lock (handlers)
        {
            handlers.Add(handler);
        }
    }

    /// <inheritdoc />
    public void Unsubscribe(string eventName, Func<IEvent, Task> handler)
    {
        if (_subscriptions.TryGetValue(eventName, out var handlers))
        {
            lock (handlers)
            {
                handlers.Remove(handler);
            }
        }
    }
}