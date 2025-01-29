using System;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;
using UnigramAds.Utils;

namespace UnigramAds.Core.Bridge
{
    internal sealed class AdSonarBridge
    {
#region NATIVE_METHODS
        [DllImport("__Internal")]
        private static extern void InitAdSonar(Action<int> sdkinitialized);

        [DllImport("__Internal")]
        private static extern void ShowAd(string adUnit,
            Action adShown, Action<string> showAdFailed);

        [DllImport("__Internal")]
        private static extern void RemoveAd(string adUnit,
            Action adUnitRemoved, Action<string> failedRemoveAdUnit);
#endregion

#region NATIVE_EVENTS
        [MonoPInvokeCallback(typeof(Action<int>))]
        private static void OnInitialize(int statusCode)
        {
            var isSuccess = UnigramUtils.IsSuccess(statusCode);

            if (isSuccess)
            {
                OnInitialized?.Invoke(isSuccess);

                Debug.Log("Ad Sonar sdk successfully initialized");

                return;
            }

            Debug.LogError("Failed to initialize Ad Sonar sdk");

            OnInitialized?.Invoke(isSuccess);
        }

        [MonoPInvokeCallback(typeof(Action))]
        private static void OnAdShow()
        {
            Debug.Log("Ad sonar ad successfully shown");

            OnAdShown?.Invoke();
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnAdShowFail(string errorMessage)
        {
            Debug.LogWarning($"Failed to shown Ad Sonar ad, " +
                $"reason: {errorMessage}");

            OnAdShowFailed?.Invoke(errorMessage);
        }

        [MonoPInvokeCallback(typeof(Action))]
        private static void OnAdUnitRemove()
        {
            Debug.Log("Ad sonar ad unit removed");

            OnAdUnitRemoved?.Invoke();
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnAdUnitRemoveFail(string errorMessage)
        {
            Debug.LogWarning($"Failed to remove ad sonar ad unit, " +
                $"reason: {errorMessage}");

            OnAdUnitRemoveFailed?.Invoke(errorMessage);
        }
#endregion

        private static event Action<bool> OnInitialized;

        private static event Action OnAdShown;
        private static event Action<string> OnAdShowFailed;

        private static event Action OnAdUnitRemoved;
        private static event Action<string> OnAdUnitRemoveFailed;

        internal static void Init(Action<bool> sdkInitialized)
        {
            OnInitialized = sdkInitialized;

            InitAdSonar(OnInitialize);
        }

        internal static void ShowRewardAd(string adUnit,
            Action adShown, Action<string> adShowFailed)
        {
            OnAdShown = adShown;
            OnAdShowFailed = adShowFailed;

            ShowAd(adUnit, OnAdShow, OnAdShowFail);
        }

        internal static void RemoveAdUnit(string adUnit,
            Action adUnitRemoved, Action<string> adUnitRemoveFailed)
        {
            OnAdUnitRemoved = adUnitRemoved;
            OnAdUnitRemoveFailed = adUnitRemoveFailed;

            RemoveAd(adUnit, OnAdUnitRemove, OnAdUnitRemoveFail);
        }
    }
}