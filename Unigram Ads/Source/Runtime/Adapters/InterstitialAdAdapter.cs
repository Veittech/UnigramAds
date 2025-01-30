using System;
using System.Diagnostics;
using UnigramAds.Core.Bridge;
using UnigramAds.Utils;

namespace UnigramAds.Core.Adapters
{
    public sealed class InterstitialAdAdapter : IVideoAd
    {
        private readonly UnigramAdsSDK _unigramSDK;

        public event Action OnShowFinished;
        public event Action<string> OnShowFailed;

        public InterstitialAdAdapter()
        {
            if (UnigramAdsSDK.Instance == null)
            {
                UnigramAdsLogger.LogWarning("Sdk is not initialized");

                return;
            }

            _unigramSDK = UnigramAdsSDK.Instance;
        }

        public void Show()
        {
            ShowAdWithCallback(() => { OnShowFinished?.Invoke(); });
        }

        public void Destroy()
        {
            if (!_unigramSDK.IsAvailableAdsGram)
            {
                UnigramAdsLogger.LogWarning("AdsGram ad units is not available");

                return;
            }

            if (!UnigramUtils.IsSupporedPlatform() ||
                !IsAvailableAdUnit())
            {
                UnigramAdsLogger.LogWarning("Ad unit is not available");

                return;
            }

            AdsGramBridge.DestroyNativeAd();
        }

        public void Destroy(string adUnit)
        {
            if (!_unigramSDK.IsAvailableAdSonar)
            {
                UnigramAdsLogger.LogWarning("AdSonar ad units is not available");

                return;
            }

            if (!UnigramUtils.IsSupporedPlatform() ||
                !IsAvailableAdUnit())
            {
                UnigramAdsLogger.LogWarning("Ad unit is not available");

                return;
            }

            var rewardAdUnit = _unigramSDK.InterstitialAdUnit;

            AdSonarBridge.RemoveAdUnit(rewardAdUnit, () =>
            {
                UnigramAdsLogger.Log($"Interstitial ad unit "+
                    $"{rewardAdUnit} from AdsSonar removed");
            },
            (errorMessage) =>
            {
                UnigramAdsLogger.LogWarning($"Failed to remove "+
                    $"interstitial ad unit {rewardAdUnit} from AdsSonar");
            });
        }

        private void ShowAdWithCallback(Action adShown)
        {
            if (!UnigramUtils.IsSupporedPlatform())
            {
                return;
            }

            if (!IsAvailableAdUnit())
            {
                UnigramAdsLogger.LogWarning("Ad unit is not available");

                return;
            }

            var interstitialAdUnit = _unigramSDK.InterstitialAdUnit;

            if (_unigramSDK.IsAvailableAdSonar)
            {
                AdSonarBridge.ShowInterstitialAdByUnit(
                    interstitialAdUnit, OnAdShown, OnAdShowFailed);

                return;
            }

            if (_unigramSDK.IsAvailableAdsGram)
            {
                AdsGramBridge.ShowNativeAd(
                    interstitialAdUnit, OnAdShown, OnAdShowFailed);
            }
        }

        private void OnAdShown()
        {
            OnShowFinished?.Invoke();

            UnigramAdsLogger.Log("Interstitial ad successfully shown");
        }

        private void OnAdShowFailed(string errorMessage)
        {
            OnShowFailed?.Invoke(errorMessage);

            UnigramAdsLogger.LogWarning("Failed to show " +
                $"rewarded ad, reason: {errorMessage}");
        }

        private bool IsAvailableAdUnit()
        {
            return !string.IsNullOrEmpty(_unigramSDK.InterstitialAdUnit);
        }
    }
}