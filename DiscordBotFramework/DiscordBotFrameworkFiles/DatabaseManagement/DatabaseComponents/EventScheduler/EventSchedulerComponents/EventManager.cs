using System.Collections.Concurrent;
using System.Runtime.Serialization;

[DataContract]
public class EventManager
{
    public ConcurrentBag<ScheduledEvent> ClassScheduledEvents
    {
        get => classScheduledEvents.GetValue();
        set => classScheduledEvents.SetValue(value);
    }

    public EventManager() { }

    [DataMember] private logConcurrentBag<ScheduledEvent> classScheduledEvents = new logConcurrentBag<ScheduledEvent>();

    public ulong GetTimeOfEventOfType(Type _eventType)
    {
        try
        {
            Log.WriteLine("Getting time on: " + _eventType, LogLevel.DEBUG);
            var time = GetEventByType(_eventType).TimeToExecuteTheEventOn;
            Log.WriteLine("Got time on: " + _eventType + ", " + time + " returning it", LogLevel.DEBUG);
            return time;
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
            throw new InvalidOperationException(ex.Message);
        }
    }

    public ulong GetTimeUntilEventOfType(Type _eventType)
    {
        try
        {
            Log.WriteLine("Getting time on: " + _eventType, LogLevel.DEBUG);
            var timeUntil = GetEventByType(_eventType).TimeToExecuteTheEventOn - TimeService.GetCurrentUnixTime();
            Log.WriteLine("Got time on: " + _eventType + ", " + timeUntil + " returning it", LogLevel.DEBUG);
            return timeUntil;
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
            throw new InvalidOperationException(ex.Message);
        }
    }

    public ScheduledEvent GetEventByType(Type _eventType)
    {
        try
        {
            Log.WriteLine("Getting event by type: " + _eventType, LogLevel.DEBUG);

            if (ClassScheduledEvents == null)
            {
                Log.WriteLine("ClassScheduledEvents is null.", LogLevel.CRITICAL);
                throw new InvalidOperationException("ClassScheduledEvents is null.");
            }

            if (_eventType == null)
            {
                Log.WriteLine("eventType is null.", LogLevel.CRITICAL);
                throw new ArgumentNullException(nameof(_eventType));
            }

            if (!typeof(ScheduledEvent).IsAssignableFrom(_eventType))
            {
                Log.WriteLine("eventType is not a ScheduledEvent.", LogLevel.CRITICAL);
                throw new ArgumentException("eventType is not a ScheduledEvent.");
            }

            ScheduledEvent? eventOfType = ClassScheduledEvents.FirstOrDefault(e => e.GetType() == _eventType);
            if (eventOfType == null)
            {
                Log.WriteLine($"Event of type {_eventType.Name} does not exist in ClassScheduledEvents.", LogLevel.CRITICAL);
                throw new InvalidOperationException($"Event of type {_eventType.Name} does not exist in ClassScheduledEvents.");
            }

            Log.WriteLine("Found " + eventOfType + ", returning it", LogLevel.DEBUG);
            return eventOfType;
        }
        catch (Exception ex)
        {
            Log.WriteLine("Error in GetEventByType: " + ex.Message, LogLevel.CRITICAL);
            throw new InvalidOperationException(ex.Message);
        }
    }

    public List<ScheduledEvent> GetListOfEventsByType(Type _eventType)
    {
        try
        {
            Log.WriteLine("Getting list of events by type: " + _eventType, LogLevel.DEBUG);
            if (ClassScheduledEvents == null)
            {
                Log.WriteLine("ClassScheduledEvents is null.", LogLevel.CRITICAL);
                throw new InvalidOperationException("ClassScheduledEvents is null.");
            }

            if (_eventType == null)
            {
                Log.WriteLine("eventType is null.", LogLevel.CRITICAL);
                throw new ArgumentNullException(nameof(_eventType));
            }

            if (!typeof(ScheduledEvent).IsAssignableFrom(_eventType))
            {
                Log.WriteLine("eventType is not a ScheduledEvent.", LogLevel.CRITICAL);
                throw new ArgumentException("eventType is not a ScheduledEvent.");
            }

            List<ScheduledEvent> eventsOfType = ClassScheduledEvents.Where(e => e.GetType() == _eventType).ToList();

            if (eventsOfType.Count == 0)
            {
                Log.WriteLine($"No events of type {_eventType.Name} exist in ClassScheduledEvents.", LogLevel.CRITICAL);
                throw new InvalidOperationException($"No events of type {_eventType.Name} exist in ClassScheduledEvents.");
            }

            Log.WriteLine("Found " + eventsOfType.Count + " events, returning them", LogLevel.DEBUG);
            return eventsOfType;
        }
        catch (Exception ex)
        {
            Log.WriteLine("Error in GetEventsByType: " + ex.Message, LogLevel.CRITICAL);
            throw new InvalidOperationException(ex.Message);
        }
    }

    public void HandleEvents(ulong _currentUnixTime)
    {
        try
        {
            Log.WriteLine("Handling events with time: " + _currentUnixTime, LogLevel.DEBUG);

            foreach (ScheduledEvent scheduledEvent in ClassScheduledEvents)
            {
                // Perhaps temp, maybe move this to inside the class itself
                try
                {
                    Log.WriteLine("Loop on: " + scheduledEvent.EventId + scheduledEvent.GetType() +
                        " with time: " + scheduledEvent.TimeToExecuteTheEventOn);

                    if (!scheduledEvent.CheckIfTheEventCanBeExecuted(_currentUnixTime))
                    {
                        Log.WriteLine("Event: " + scheduledEvent.EventId + scheduledEvent.GetType() +
                            " with time: " + scheduledEvent.TimeToExecuteTheEventOn + " can not be executed, continuing");
                        continue;
                    }

                    Log.WriteLine("Event: " + scheduledEvent.EventId + scheduledEvent.GetType() +
                        " with time: " + scheduledEvent.TimeToExecuteTheEventOn + " can be executed, continuing");

                    // Event succesfully executed
                    var scheduledEventsToRemove = ClassScheduledEvents.Where(e => e.EventId == scheduledEvent.EventId).ToList();
                    foreach (var item in scheduledEventsToRemove)
                    {
                        Log.WriteLine("event: " + item.EventId + " scheduledEventsToRemove: " + item.EventId);
                    }

                    RemoveEventsFromTheScheduledEventsBag(scheduledEventsToRemove);

                    Log.WriteLine("Event: " + scheduledEvent.EventId + scheduledEvent.GetType() +
                        " with time: " + scheduledEvent.TimeToExecuteTheEventOn + "executed", LogLevel.DEBUG);
                }
                catch (Exception ex)
                {
                    Log.WriteLine(ex.Message, LogLevel.CRITICAL);
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            Log.WriteLine("Error in HandleEvents: " + ex.Message, LogLevel.CRITICAL);
            throw new InvalidOperationException(ex.Message);
        }
    }

    // Refactor this to take in single event id instead of list
    public void RemoveEventsFromTheScheduledEventsBag(List<ScheduledEvent> _scheduledEventsToRemove)
    {
        var updatedScheduledEvents = new ConcurrentBag<ScheduledEvent>();

        foreach (var item in ClassScheduledEvents)
        {
            if (!_scheduledEventsToRemove.Contains(item))
            {
                updatedScheduledEvents.Add(item);
            }
        }

        ClassScheduledEvents = updatedScheduledEvents;

        foreach (var item in _scheduledEventsToRemove)
        {
            if (!ClassScheduledEvents.Contains(item))
            {
                Log.WriteLine("event: " + item.EventId + " removed", LogLevel.DEBUG);
            }
            else
            {
                Log.WriteLine("event: " + item.EventId + ", failed to remove", LogLevel.ERROR);
            }
        }
    }

    public void ClearCertainTypeOfEventsFromTheList(Type _type)
    {
        try
        {
            if (ClassScheduledEvents == null)
            {
                Log.WriteLine("ClassScheduledEvents is null.", LogLevel.CRITICAL);
                throw new InvalidOperationException("ClassScheduledEvents is null.");
            }

            List<ScheduledEvent> events = ClassScheduledEvents
                .Where(e => e.GetType() == _type)
                .ToList();

            foreach (ScheduledEvent @event in events)
            {
                ScheduledEvent eventToRemove = @event; // Create a temporary variable
                ClassScheduledEvents.TryTake(out eventToRemove);
            }

            Log.WriteLine($"{events.Count} {_type.Name}(s) removed from ClassScheduledEvents.");
        }
        catch (Exception ex)
        {
            Log.WriteLine("Error in ClearCertainTypeOfEventsFromTheList: " + ex.Message, LogLevel.ERROR);
            throw; // Re-throw the exception to preserve the original exception stack trace
        }
    }
}