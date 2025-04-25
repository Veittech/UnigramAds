using UnityEngine;
using UnigramAds.Common;
using UnigramAds.Utils;
using UnigramAds.Data;

namespace UnigramAds.Core.Events
{
    public sealed class NativeAdEventListener : MonoBehaviour
    {
        private static readonly object _lock = new();

        private static NativeAdEventListener _instance;

        private void Awake()
        {
            CreateInstance();

            UnigramAdsLogger.Log($"Native events listener created");
        }

        public void ReceiveEvent(string eventPayload)
        {
            var parsedPayload = JsonUtility.FromJson<NativeEventPayloadData>(eventPayload);

            var eventType = AdEvents.GetEventById(parsedPayload.EventId);
            var adType = parsedPayload.AdType;

            NativeEventBus.Invoke(adType, eventType);
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