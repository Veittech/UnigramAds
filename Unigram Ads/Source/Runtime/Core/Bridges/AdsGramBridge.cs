using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnigramAds.Utils;
using UnigramAds.Common;

namespace UnigramAds.Core.Bridge
{
    internal static class AdsGramBridge
    {
#region NATIVE_METHODS
        [DllImport("__Internal")]
        private static extern void InitAdsGram(Action<int> sdkInitialized);

        [DllImport("__Internal")]
        private static extern void ShowAd(string adType, string adUnit,
            bool isTestMode, Action adShown, Action<string> showAdFailed);

        [DllImport("__Internal")]
        private static extern void DestroyAd();
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
        private static void OnRewardAdShow()
        {
            UnigramAdsLogger.Log("Reward ad successfully shown");

            OnRewardAdShown?.Invoke();
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void OnRewardAdShowFail(string errorMessage)
        {
            UnigramAdsLogger.LogWarning($"Failed to show " +
                $"reward ad, reason: {errorMessage}");

            OnRewardAdShowFailed?.Invoke(errorMessage);
        }

        #endregion

        private static event Action<bool> OnInitialized;

        private static event Action OnRewardAdShown;
        private static event Action<string> OnRewardAdShowFailed;

        internal static void Init(Action<bool> sdkInitialized)
        {
            OnInitialized = sdkInitialized;

            InitAdsGram(OnInitialize);
        }

        internal static void Show(NativeAdTypes adType, string adUnitId, 
            bool isTestMode, Action adShown, Action<string> adShowFailed)
        {
            OnRewardAdShown = adShown;
            OnRewardAdShowFailed = adShowFailed;

            ShowAd(adType.ToString(), adUnitId, 
                isTestMode, OnRewardAdShow, OnRewardAdShowFail);
        }

        internal static void Destroy()
        {
            DestroyAd();
        }
    }
}