using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnigramAds.Utils;

namespace UnigramAds.Core.Bridge
{
    internal sealed class AdSonarBridge
    {
#region NATIVE_METHODS
        [DllImport("__Internal")]
        private static extern void InitAdSonar(string appId,
            bool isTesting, Action<int> sdkinitialized);

        [DllImport("__Internal")]
        private static extern void ShowInterstitialAd(string adUnit,
            Action adShown, Action<string> showAdFailed);

        [DllImport("__Internal")]
        private static extern void ShowRewardedAd(string adUnit,
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

                Debug.Log($"{UnigramAdsLogger.PREFIX} Sdk successfully initialized");

                return;
            }

            OnInitialized?.Invoke(isSuccess);

            Debug.LogError($"{UnigramAdsLogger.PREFIX} Failed to initialize sdk");
        }

        [MonoPInvokeCallback(typeof(Action))]
        private static void OnAdShow()
        {
            OnAdShown?.Invoke();

            UnigramAdsLogger.Log("Ad sonar ad successfully shown");

            OnAdShown = null;
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnAdShowFail(string errorMessage)
        {
            OnAdShowFailed?.Invoke(errorMessage);

            UnigramAdsLogger.LogWarning($"Failed to shown " +
                $"Ad Sonar ad, reason: {errorMessage}");

            OnAdShowFailed = null;
        }

        [MonoPInvokeCallback(typeof(Action))]
        private static void OnAdUnitRemove()
        {
            OnAdUnitRemoved?.Invoke();

            UnigramAdsLogger.Log("Ad sonar ad unit removed");
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnAdUnitRemoveFail(string errorMessage)
        {
            OnAdUnitRemoveFailed?.Invoke(errorMessage);

            UnigramAdsLogger.LogWarning($"Failed to remove ad sonar " +
                $"ad unit, reason: {errorMessage}");
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

            InitAdSonar(string.Empty, false, OnInitialize);
        }

        internal static void ShowRewardedAdByUnit(string adUnit,
            Action adShown, Action<string> adShowFailed)
        {
            OnAdShown = adShown;
            OnAdShowFailed = adShowFailed;

            ShowRewardedAd(adUnit, OnAdShow, OnAdShowFail);
        }

        internal static void ShowInterstitialAdByUnit(string adUnit,
            Action adShown, Action<string> adShowFailed)
        {
            OnAdShown = adShown;
            OnAdShowFailed = adShowFailed;

            ShowInterstitialAd(adUnit, OnAdShow, OnAdShowFail);
        }

        internal static void Destroy(string adUnit,
            Action adUnitRemoved, Action<string> adUnitRemoveFailed)
        {
            OnAdUnitRemoved = adUnitRemoved;
            OnAdUnitRemoveFailed = adUnitRemoveFailed;

            RemoveAd(adUnit, OnAdUnitRemove, OnAdUnitRemoveFail);
        }
    }
}