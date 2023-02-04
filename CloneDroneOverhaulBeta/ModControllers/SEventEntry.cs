﻿using System;

namespace CDOverhaul
{
    public class EventEntry
    {
        public string EventName { get; set; }
        public bool WithPrefix { get; set; }
        public Action EventAction { get; set; }
        public object EventActionWithArgument { get; set; }

        public EventEntry(in string eventName, in Action @delegate, in bool withPrefix)
        {
            EventName = eventName;
            EventAction = @delegate;
            EventActionWithArgument = null;
            WithPrefix = withPrefix;
        }

        public void SetArgument<T>(Action<T> @delegate)
        {
            EventAction = null;
            EventActionWithArgument = @delegate;
        }

        public Action<T> GetActionWithArgument<T>()
        {
            return EventActionWithArgument as Action<T>;
        }

        public bool Equals(in EventEntry a, in EventEntry b)
        {
            return a.EventName == b.EventName && a.EventAction == b.EventAction && a.EventActionWithArgument == b.EventActionWithArgument && a.WithPrefix == b.WithPrefix;
        }
    }
}
