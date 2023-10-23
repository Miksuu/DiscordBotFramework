using Discord;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
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

    public ConcurrentBag<ScheduledEvent> GetEvents()
    {
        return ClassScheduledEvents;
    }

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
            Log.WriteLine(ex.Message, LogLevel.ERROR);
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
            Log.WriteLine(ex.Message, LogLevel.ERROR);
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
                Log.WriteLine("ClassScheduledEvents is null.", LogLevel.ERROR);
                throw new InvalidOperationException("ClassScheduledEvents is null.");
            }

            if (_eventType == null)
            {
                Log.WriteLine("eventType is null.", LogLevel.ERROR);
                throw new ArgumentNullException(nameof(_eventType));
            }

            if (!typeof(ScheduledEvent).IsAssignableFrom(_eventType))
            {
                Log.WriteLine("eventType is not a ScheduledEvent.", LogLevel.ERROR);
                throw new ArgumentException("eventType is not a ScheduledEvent.");
            }

            ScheduledEvent? eventOfType = ClassScheduledEvents.FirstOrDefault(e => e.GetType() == _eventType);
            if (eventOfType == null)
            {
                Log.WriteLine($"Event of type {_eventType.Name} does not exist in ClassScheduledEvents.", LogLevel.ERROR);
                throw new InvalidOperationException($"Event of type {_eventType.Name} does not exist in ClassScheduledEvents.");
            }

            Log.WriteLine("Found " + eventOfType + ", returning it", LogLevel.DEBUG);
            return eventOfType;
        }
        catch (Exception ex)
        {
            Log.WriteLine("Error in GetEventByType: " + ex.Message, LogLevel.ERROR);
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
                Log.WriteLine("ClassScheduledEvents is null.", LogLevel.ERROR);
                throw new InvalidOperationException("ClassScheduledEvents is null.");
            }

            if (_eventType == null)
            {
                Log.WriteLine("eventType is null.", LogLevel.ERROR);
                throw new ArgumentNullException(nameof(_eventType));
            }

            if (!typeof(ScheduledEvent).IsAssignableFrom(_eventType))
            {
                Log.WriteLine("eventType is not a ScheduledEvent.", LogLevel.ERROR);
                throw new ArgumentException("eventType is not a ScheduledEvent.");
            }

            List<ScheduledEvent> eventsOfType = ClassScheduledEvents.Where(e => e.GetType() == _eventType).ToList();

            if (eventsOfType.Count == 0)
            {
                Log.WriteLine($"No events of type {_eventType.Name} exist in ClassScheduledEvents.", LogLevel.ERROR);
                throw new InvalidOperationException($"No events of type {_eventType.Name} exist in ClassScheduledEvents.");
            }

            Log.WriteLine("Found " + eventsOfType.Count + " events, returning them", LogLevel.DEBUG);
            return eventsOfType;
        }
        catch (Exception ex)
        {
            Log.WriteLine("Error in GetEventsByType: " + ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }

    public void HandleEvents(ulong _currentUnixTime)
    {
        try
        {
            Log.WriteLine("Handling events with time: " + _currentUnixTime + " with " +
                nameof(ClassScheduledEvents) + "'s count: " + ClassScheduledEvents.Count, LogLevel.DEBUG);

            var additionalEvents = AdditionalEvents.GetAdditionalEvents();
            foreach (ScheduledEvent scheduledEvent in ClassScheduledEvents.Concat(additionalEvents))
            {
                // Perhaps temp, maybe move this to inside the class itself
                try
                {
                    Log.WriteLine("Loop on: " + scheduledEvent.EventId + scheduledEvent.GetType() +
                        " with time: " + scheduledEvent.TimeToExecuteTheEventOn);

                    if (!scheduledEvent.CheckIfTheEventCanBeExecuted(_currentUnixTime))
                    {
                        Log.WriteLine("Event: " + scheduledEvent.EventId + " can not be executed, continuing");
                        continue;
                    }

                    Log.WriteLine("Event: " + scheduledEvent.EventId + " can be executed, continuing");

                    // Event succesfully executed
                    var scheduledEventToRemove = ClassScheduledEvents.FirstOrDefault(e => e.EventId == scheduledEvent.EventId);
                    if (scheduledEventToRemove == null)
                    {
                        Log.WriteLine("scheduledEventToRemove is null.", LogLevel.ERROR);
                        throw new InvalidOperationException("scheduledEventToRemove is null.");
                    }

                    RemoveEventFromTheScheduledEventsBag(scheduledEventToRemove);

                    Log.WriteLine("Event: " + scheduledEvent.EventId + " executed", LogLevel.DEBUG);
                }
                catch (Exception ex)
                {
                    Log.WriteLine(ex.Message, LogLevel.ERROR);
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            Log.WriteLine("Error in HandleEvents: " + ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }

    public void RemoveEventFromTheScheduledEventsBag(ScheduledEvent scheduledEventToRemove)
    {
        try
        {
            var updatedScheduledEvents = new ConcurrentBag<ScheduledEvent>();

            Log.WriteLine("Removing event: " + scheduledEventToRemove.EventId + " looping though " +
                nameof(ClassScheduledEvents) + " with count: " + ClassScheduledEvents.Count(), LogLevel.DEBUG);

            foreach (ScheduledEvent scheduledEvent in ClassScheduledEvents)
            {
                try
                {
                    Log.WriteLine("Looping on item: " + scheduledEvent.EventId);
                    if (scheduledEvent != scheduledEventToRemove)
                    {
                        Log.WriteLine(scheduledEvent.EventId + " != " + scheduledEventToRemove.EventId + " adding");
                        updatedScheduledEvents.Add(scheduledEvent);
                        continue;
                    }

                    Log.WriteLine(scheduledEvent.EventId + " == " + scheduledEventToRemove.EventId + ", did not add");
                }
                catch (Exception ex)
                {
                    Log.WriteLine(ex.Message, LogLevel.ERROR);
                    throw new InvalidOperationException(ex.Message);
                }
            }

            ClassScheduledEvents = updatedScheduledEvents;

            if (!ClassScheduledEvents.Contains(scheduledEventToRemove))
            {
                Log.WriteLine("Event: " + scheduledEventToRemove.EventId + " removed", LogLevel.DEBUG);
            }
            else
            {
                Log.WriteLine("Event: " + scheduledEventToRemove.EventId + " failed to remove", LogLevel.ERROR);
            }

            Log.WriteLine("Removed event: " + scheduledEventToRemove.EventId, LogLevel.DEBUG);
        }
        catch (Exception ex)
        {
            Log.WriteLine(ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException(ex.Message);
        }
    }

    public void ClearCertainTypeOfEventsFromTheList(Type _type)
    {
        try
        {
            Log.WriteLine("Clearing events of type: " + _type, LogLevel.DEBUG);

            if (ClassScheduledEvents == null)
            {
                Log.WriteLine("ClassScheduledEvents is null.", LogLevel.ERROR);
                throw new InvalidOperationException("ClassScheduledEvents is null.");
            }

            List<ScheduledEvent> events = ClassScheduledEvents
                .Where(e => e.GetType() == _type)
                .ToList();

            Log.WriteLine("Found: " + events.Count + " events");

            foreach (ScheduledEvent loopEvent in events)
            {
                try
                {
                    Log.WriteLine("Looping on event: " + loopEvent.EventId + " with time: " + loopEvent.TimeToExecuteTheEventOn);

                    ScheduledEvent? eventToRemove = loopEvent;
                    ClassScheduledEvents.TryTake(out eventToRemove);
                    if (eventToRemove == null)
                    {
                        Log.WriteLine("eventToRemove is null.", LogLevel.ERROR);
                        throw new InvalidOperationException("eventToRemove is null.");
                    }

                    Log.WriteLine("Removed event: " + eventToRemove.EventId + " with time: " + eventToRemove.TimeToExecuteTheEventOn);
                }
                catch (Exception ex)
                {
                    Log.WriteLine("Error in ClearCertainTypeOfEventsFromTheList forloop: " + ex.Message, LogLevel.ERROR);
                    throw new InvalidOperationException("Error in ClearCertainTypeOfEventsFromTheList forloop: " + ex.Message);
                }
            }

            Log.WriteLine($"{events.Count} {_type.Name}(s) removed from ClassScheduledEvents.", LogLevel.DEBUG);
        }
        catch (Exception ex)
        {
            Log.WriteLine("Error in ClearCertainTypeOfEventsFromTheList: " + ex.Message, LogLevel.ERROR);
            throw new InvalidOperationException("Error in ClearCertainTypeOfEventsFromTheList: " + ex.Message);
        }
    }
}