using UnityEngine;
using UnigramAds.Common;
using UnigramAds.Utils;

namespace UnigramAds.Core.Events
{
    public sealed class NativeAdEventListener : MonoBehaviour
    {
        private static readonly object _lock = new();

        private static NativeAdEventListener _instance;

        private static NativeAdEventListener _instanceReference
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<NativeAdEventListener>();
                    }
                }

                return _instance;
            }
        }

        private void Awake()
        {
            CreateInstance();
        }


        public void ReceiveEvent(string eventId)
        {
            var eventType = AdEvents.GetEventById(eventId);

            NativeEventBus.Invoke(eventType);
        }

        private void CreateInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = this;

                    DontDestroyOnLoad(gameObject);

                    return;
                }

                if (_instance != null)
                {
                    UnigramAdsLogger.LogWarning($"Another `EventListener` " +
                        $"instance is detected on the scene, running delete...");

                    Destroy(gameObject);
                }
            }
        }
    }
}