using System;
using System.Collections.Generic;
using UnigramAds.Common;

namespace UnigramAds.Core.Events
{
    public static class NativeEventBus
    {
        private readonly static Dictionary<AdEventsTypes, Action> _events = new();

        public static void Subscribe(
            AdEventsTypes type, Action callback)
        {
            if (_events.ContainsKey(type))
            {
                _events[type] += callback;

                return;
            }

            _events[type] = callback;
        }

        public static void Unsubscribe(
            AdEventsTypes type, Action callback)
        {
            if (_events.ContainsKey(type))
            {
                _events[type] -= callback;

                if (_events[type] == null)
                {
                    _events.Remove(type);
                }
            }
        }

        public static void Invoke(AdEventsTypes type)
        {
            if (_events.TryGetValue(type, out var callback))
            {
                callback?.Invoke();
            }
        }
    }
}