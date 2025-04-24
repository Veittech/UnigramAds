using System;
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
            ShowAdWithCallback();
        }

        public void Destroy()
        {
            if (!UnigramUtils.IsSupporedPlatform() ||
                !IsAvailableAdUnit())
            {
                UnigramAdsLogger.LogWarning("Ad unit is not available");

                return;
            }

            var interstitialAdUnit = _unigramSDK.InterstitialAdUnit;

            if (_unigramSDK.IsAvailableAdsGram)
            {
                AdsGramBridge.DestroyNativeAd();

                UnigramAdsLogger.Log($"Interstitial ad unit " +
                    $"{interstitialAdUnit} from AdsGram removed!");
            }

            if (_unigramSDK.IsAvailableAdSonar)
            {
                AdSonarBridge.RemoveAdUnit(interstitialAdUnit, () =>
                {
                    UnigramAdsLogger.Log($"Interstitial ad unit " +
                        $"{interstitialAdUnit} from AdsSonar removed!");
                },
                (errorMessage) =>
                {
                    UnigramAdsLogger.LogWarning($"Failed to remove " +
                        $"interstitial ad unit {interstitialAdUnit} from AdsSonar");
                });
            }
        }

        private void ShowAdWithCallback()
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
            }

            if (_unigramSDK.IsAvailableAdsGram)
            {
                AdsGramBridge.ShowNativeAd(interstitialAdUnit,
                    _unigramSDK.IsTestMode, OnAdShown, OnAdShowFailed);
            }
        }

        private void OnAdShown()
        {
            OnShowFinished?.Invoke();

            UnigramAdsLogger.Log($"Interstitial ad successfully " +
                $"shown by network: {_unigramSDK.CurrentNetwork}");
        }

        private void OnAdShowFailed(string errorMessage)
        {
            OnShowFailed?.Invoke(errorMessage);

            UnigramAdsLogger.LogWarning("Failed to show " +
                $"rewarded ad by network {_unigramSDK.CurrentNetwork}, reason: {errorMessage}");
        }

        private bool IsAvailableAdUnit()
        {
            return !string.IsNullOrEmpty(_unigramSDK.InterstitialAdUnit);
        }
    }
}