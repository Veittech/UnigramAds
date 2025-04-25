using System;
using System.Collections.Generic;
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
        private static extern void InitAdsGram(Action<int> sdkInitialized);

        [DllImport("__Internal")]
        private static extern void ShowAd(string adUnit,
            bool isTestMode, Action adShown, Action<string> showAdFailed);

        [DllImport("__Internal")]
        private static extern void DestroyAd();

        [DllImport("__Internal")]
        private static extern void AddListener(
            string eventId, Action actionSubsribed);

        [DllImport("__Internal")]
        private static extern void RemoveListener(
            string eventId, Action actionUnsubsribed);
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

        [MonoPInvokeCallback(typeof(Action))]
        private static void OnEventListenerInvoke()
        {
            OnEventListenerInvoked?.Invoke();
        }

        [MonoPInvokeCallback(typeof(Action))]
        private static void OnEventListenerRemove()
        {
            OnEventListenerRemoved?.Invoke();
        }
        #endregion

        private static event Action<bool> OnInitialized;

        private static event Action OnRewardAdShown;
        private static event Action<string> OnRewardAdShowFailed;

        private static event Action OnEventListenerInvoked;
        private static event Action OnEventListenerRemoved;

        internal static void Subscribe(
            AdEventsTypes eventType, Action eventInvoked)
        {
            var eventId = AdEvents.GetEventByType(eventType);

            OnEventListenerInvoked = eventInvoked;

            AddListener(eventId, OnEventListenerInvoke);
        }

        internal static void Subscribe(
            Dictionary<AdEventsTypes, Action> eventsMap)
        {
            foreach (var map in eventsMap)
            {
                Subscribe(map.Key, map.Value);
            }
        }

        internal static void UnSubscribe(
            AdEventsTypes eventType, Action eventUnsubscribed)
        {
            var eventId = AdEvents.GetEventByType(eventType);

            OnEventListenerRemoved = eventUnsubscribed;

            RemoveListener(eventId, OnEventListenerRemove);
        }

        internal static void UnSubscribe(
            Dictionary<AdEventsTypes, Action> eventsMap)
        {
            foreach (var map in eventsMap)
            {
                UnSubscribe(map.Key, map.Value);
            }
        }

        internal static void Init(Action<bool> sdkInitialized)
        {
            OnInitialized = sdkInitialized;

            InitAdsGram(OnInitialize);
        }

        internal static void ShowNativeAd(string adUnitId, bool isTestMode,
            Action adShown, Action<string> adShowFailed)
        {
            OnRewardAdShown = adShown;
            OnRewardAdShowFailed = adShowFailed;

            ShowAd(adUnitId, isTestMode, OnRewardAdShow, OnRewardAdShowFail);
        }

        internal static void DestroyNativeAd()
        {
            DestroyAd();
        }
    }
}