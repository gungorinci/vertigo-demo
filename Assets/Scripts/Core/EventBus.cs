using System;
using System.Collections.Generic;

namespace VertigoDemo.Core
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        #region IEventBus

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            List<Delegate> handlers;
            if (!_handlers.TryGetValue(typeof(T), out handlers))
            {
                handlers = new List<Delegate>();
                _handlers.Add(typeof(T), handlers);
            }
            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            List<Delegate> handlers;
            if (_handlers.TryGetValue(typeof(T), out handlers))
            {
                handlers.Remove(handler);
            }
        }

        public void Publish<T>(T gameEvent) where T : struct
        {
            List<Delegate> handlers;
            if (!_handlers.TryGetValue(typeof(T), out handlers))
            {
                return;
            }

            // A handler may subscribe or unsubscribe while the event is being delivered.
            Delegate[] snapshot = handlers.ToArray();
            foreach (Delegate handler in snapshot)
            {
                ((Action<T>)handler)(gameEvent);
            }
        }

        #endregion
    }
}
