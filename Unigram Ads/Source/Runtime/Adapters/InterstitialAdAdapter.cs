using System;
using UnigramAds.Core.Bridge;
using UnigramAds.Core.Events;
using UnigramAds.Common;
using UnigramAds.Utils;

namespace UnigramAds.Core.Adapters
{
    public sealed class InterstitialAdAdapter : IVideoAd, IDisposable
    {
        private readonly UnigramAdsSDK _unigramSDK;

        private AdNetworkTypes _currentNetwork => _unigramSDK.CurrentNetwork;

        private bool _isDisposed;

        public event Action OnLoaded;
        public event Action OnClosed;
        public event Action OnShown;

        public event Action<string> OnShowFailed;

        public InterstitialAdAdapter()
        {
            if (UnigramAdsSDK.Instance == null)
            {
                UnigramAdsLogger.LogWarning("Sdk is not initialized");

                return;
            }

            _unigramSDK = UnigramAdsSDK.Instance;

            NativeEventBus.Subscribe(AdEventsTypes.Started, AdLoaded);
            NativeEventBus.Subscribe(AdEventsTypes.Closed, AdClosed);
            NativeEventBus.Subscribe(AdEventsTypes.Shown, AdShown);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            NativeEventBus.Unsubscribe(AdEventsTypes.Started, AdLoaded);
            NativeEventBus.Unsubscribe(AdEventsTypes.Closed, AdClosed);
            NativeEventBus.Unsubscribe(AdEventsTypes.Shown, AdShown);

            _isDisposed = true;
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
                    interstitialAdUnit, AdShown, AdShowFailed);
            }

            if (_unigramSDK.IsAvailableAdsGram)
            {
                AdsGramBridge.ShowNativeAd(interstitialAdUnit,
                    _unigramSDK.IsTestMode, AdShown, AdShowFailed);
            }
        }

        private bool IsAvailableAdUnit()
        {
            return !string.IsNullOrEmpty(_unigramSDK.InterstitialAdUnit);
        }

        private void AdLoaded()
        {
            OnLoaded?.Invoke();

            UnigramAdsLogger.Log($"Interstitial ad successfully " +
                $"loaded by network: {_currentNetwork}");
        }

        private void AdClosed()
        {
            OnClosed?.Invoke();

            UnigramAdsLogger.Log($"Interstitial ad closed " +
                $"by network: {_currentNetwork}");
        }

        private void AdShown()
        {
            OnShown?.Invoke();

            UnigramAdsLogger.Log($"Interstitial ad successfully " +
                $"shown by network: {_currentNetwork}");
        }

        private void AdShowFailed(string errorMessage)
        {
            OnShowFailed?.Invoke(errorMessage);

            UnigramAdsLogger.LogWarning("Failed to show rewarded ad " +
                $"by network {_currentNetwork}, reason: {errorMessage}");
        }
    }
}