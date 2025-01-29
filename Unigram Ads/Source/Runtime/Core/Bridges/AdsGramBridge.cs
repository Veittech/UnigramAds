using System;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;
using UnigramAds.Common;
using UnigramAds.Utils;

namespace UnigramAds.Core.Bridge
{
    internal static class AdsGramBridge
    {
#region NATIVE_METHODS
        [DllImport("__Internal")]
        private static extern void InitAdsGram(string appId, 
            bool isTesting, string testingType, Action<int> sdkInitialized);

        [DllImport("__Internal")]
        private static extern void ShowAd(Action adShown,
            Action<string> showAdFailed);

        [DllImport("__Internal")]
        private static extern void DestroyAd();

        [DllImport("__Internal")]
        private static extern void AddListener(string eventId,
            Action<string> actionSubsribed);

        [DllImport("__Internal")]
        private static extern void RemoveListener(string eventId, 
            Action<string> actionUnsubsribed);
#endregion

#region NATIVE_EVENTS
        [MonoPInvokeCallback(typeof(Action<int>))]
        private static void OnInitialize(int statusCode)
        {
            var isSuccess = UnigramUtils.IsSuccess(statusCode);

            if (isSuccess)
            {
                OnInitialized?.Invoke(isSuccess);

                Debug.Log("Sdk successfully initialized");

                return;
            }

            Debug.LogError("Failed to initialize sdk");

            OnInitialized?.Invoke(isSuccess);
        }

        [MonoPInvokeCallback(typeof(Action))]
        private static void OnRewardAdShow()
        {
            Debug.Log("Reward ad successfully shown");

            OnRewardAdShown?.Invoke();
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnRewardAdShowFail(string errorMessage)
        {
            Debug.LogWarning($"Failed to show reward ad, reason: {errorMessage}");

            OnRewardAdShowFailed?.Invoke(errorMessage);
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnEventListenerInvoke(string eventId)
        {
            Debug.Log($"Invoked action by event: {eventId}");

            OnEventListenerInvoked?.Invoke(AdEvents.GetEventById(eventId));
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnEventListenerRemove(string eventId)
        {
            Debug.Log($"Action unsubscribed by event: {eventId}");

            OnEventListenerRemoved?.Invoke(AdEvents.GetEventById(eventId));
        }
        #endregion

        private static event Action<bool> OnInitialized;

        private static event Action OnRewardAdShown;
        private static event Action<string> OnRewardAdShowFailed;

        private static event Action<AdEventsTypes> OnEventListenerInvoked;
        private static event Action<AdEventsTypes> OnEventListenerRemoved;

        internal static void SubscribeToEvent(AdEventsTypes eventType, 
            Action<AdEventsTypes> eventInvoked)
        {
            OnEventListenerInvoked = eventInvoked;

            AddListener(AdEvents.GetEventByType(
                eventType), OnEventListenerInvoke);
        }

        internal static void UnSubscribeFromEvent(AdEventsTypes eventType,
            Action<AdEventsTypes> eventUnsubscribed)
        {
            OnEventListenerRemoved = eventUnsubscribed;

            RemoveListener(AdEvents.GetEventByType(
                eventType), OnEventListenerRemove);
        }

        internal static void Init(string adUnitId,
            bool isTesting, AdTypes testingType, Action<bool> sdkInitialized)
        {
            OnInitialized = sdkInitialized;

            var targetType = AvailableAdTypes.GetAdByType(testingType);

            InitAdsGram(adUnitId, isTesting, targetType, OnInitialize);
        }

        internal static void Init(string adUnitId,
            Action<bool> sdkInitialized)
        {
            OnInitialized = sdkInitialized;

            var targetType = AvailableAdTypes.GetAdByType(AdTypes.FullscreenStatic);

            InitAdsGram(adUnitId, false, targetType, OnInitialize);
        }

        internal static void ShowNativeAd(Action adShown,
            Action<string> adShowFailed)
        {
            OnRewardAdShown = adShown;
            OnRewardAdShowFailed = adShowFailed;

            ShowAd(OnRewardAdShow, OnRewardAdShowFail);
        }

        internal static void DestroyNativeAd()
        {
            DestroyAd();
        }
    }
}