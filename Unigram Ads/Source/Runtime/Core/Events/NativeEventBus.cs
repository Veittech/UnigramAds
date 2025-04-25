using System;
using System.Collections.Generic;
using UnigramAds.Common;

namespace UnigramAds.Core.Events
{
    public static class NativeEventBus
    {
        private readonly static Dictionary<NativeAdTypes, 
            Dictionary<AdEventsTypes, Action>> _eventMap = new();

        public static void Subscribe(NativeAdTypes adType,
            AdEventsTypes eventType, Action callback)
        {
            if (!_eventMap.ContainsKey(adType))
            {
                _eventMap[adType] = new Dictionary<AdEventsTypes, Action>();
            }

            if (_eventMap[adType].ContainsKey(eventType))
            {
                _eventMap[adType][eventType] += callback;

                return;
            }

            _eventMap[adType][eventType] = callback;
        }

        public static void Subscribe(NativeAdTypes adType,
            Dictionary<AdEventsTypes, Action> handlers)
        {
            foreach (var handler in handlers)
            {
                Subscribe(adType, handler.Key, handler.Value);
            }
        }

        public static void Unsubscribe(NativeAdTypes adType,
            AdEventsTypes eventType, Action callback)
        {
            if (!_eventMap.ContainsKey(adType))
            {
                return;
            }

            if (_eventMap[adType].ContainsKey(eventType))
            {
                _eventMap[adType][eventType] -= callback;

                if (_eventMap[adType][eventType] == null)
                {
                    _eventMap[adType].Remove(eventType);
                }
            }

            if (_eventMap[adType].Count == 0)
            {
                _eventMap.Remove(adType);
            }
        }

        public static void Unsubscribe(NativeAdTypes adType,
            Dictionary<AdEventsTypes, Action> handlers)
        {
            foreach (var handler in handlers)
            {
                Unsubscribe(adType, handler.Key, handler.Value);
            }
        }

        public static void Invoke(NativeAdTypes adType, AdEventsTypes eventType)
        {
            if (_eventMap.TryGetValue(adType, out var adTypeMap) &&
                adTypeMap.TryGetValue(eventType, out var callback))
            {
                callback?.Invoke();
            }
        }
    }
}