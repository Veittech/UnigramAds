using System;
using System.Collections.Generic;
using UnigramAds.Core.Bridge;
using UnigramAds.Core.Events;
using UnigramAds.Common;
using UnigramAds.Utils;

namespace UnigramAds.Core.Adapters
{
    public sealed class RewardAdAdapter : IRewardVideoAd
    {
        private readonly UnigramAdsSDK _unigramSDK;

        private readonly Dictionary<AdEventsTypes, Action> _callbacksMap;

        private AdNetworkTypes _currentNetwork => _unigramSDK.CurrentNetwork;

        private bool _isDisposed;

        public event Action OnLoaded;
        public event Action OnClosed;
        public event Action OnShown;
        public event Action OnRewarded;
        public event Action OnTryNonStopWatch;

        public event Action<string> OnShowFailed;
        public event Action OnShowExpired;
        public event Action OnLoadFailed;

        public RewardAdAdapter()
        {
            if (UnigramAdsSDK.Instance == null)
            {
                UnigramAdsLogger.LogWarning("Sdk is not initialized");

                return;
            }

            _unigramSDK = UnigramAdsSDK.Instance;

            _callbacksMap = new()
            {
                { AdEventsTypes.Started, AdLoaded },
                { AdEventsTypes.Skipped, AdClosed },
                { AdEventsTypes.Completed, AdShown },
                { AdEventsTypes.Rewarded, AdRewarded },
                { AdEventsTypes.NotAvailable, AdLoadFailed },
                { AdEventsTypes.TooLongSession, AdShowExpired },
                { AdEventsTypes.TryNonStopWatch, AdNonStopWatch },
            };

            NativeEventBus.Subscribe(NativeAdTypes.rewarded, _callbacksMap);
        }

        public void Show()
        {
            ShowAdWithCallback();
        }

        public void Show(Action adFinished)
        {
            ShowAdWithCallback(adFinished);
        }

        public void Destroy()
        {
            if (!UnigramUtils.IsSupporedPlatform() ||
                !IsAvailableAdUnit())
            {
                UnigramAdsLogger.LogWarning("Reward ad unit is not available");

                return;
            }

            var rewardAdUnit = _unigramSDK.RewardedAdUnit;

            if (_unigramSDK.IsAvailableAdsGram)
            {
                AdsGramBridge.Destroy();

                UnigramAdsLogger.Log($"Rewarded ad unit " +
                    $"{rewardAdUnit} from AdsGram removed!");
            }

            if (_unigramSDK.IsAvailableAdSonar)
            {
                AdSonarBridge.Destroy(rewardAdUnit, () =>
                {
                    UnigramAdsLogger.Log($"Rewarded ad unit " +
                        $"{rewardAdUnit} from AdsSonar removed!");
                },
                (errorMessage) =>
                {
                    UnigramAdsLogger.LogWarning($"Failed to remove " +
                        $"rewarded ad unit {rewardAdUnit} from AdsSonar");
                });
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            NativeEventBus.Unsubscribe(NativeAdTypes.rewarded, _callbacksMap);

            _isDisposed = true;
        }

        private void ShowAdWithCallback(Action adShown = null)
        {
            if (!UnigramUtils.IsSupporedPlatform())
            {
                return;
            }

            if (!IsAvailableAdUnit())
            {
                UnigramAdsLogger.LogWarning("Reward ad unit is not available");

                return;
            }

            var rewardAdUnit = _unigramSDK.RewardedAdUnit;

            if (adShown != null)
            {
                OnRewarded = adShown;
            }

            if (_unigramSDK.IsAvailableAdSonar)
            {
                AdSonarBridge.ShowRewardedAdByUnit(
                    rewardAdUnit, () =>
                {
                    adShown?.Invoke();
                },
                AdShowFailed);
            }

            if (_unigramSDK.IsAvailableAdsGram)
            {
                AdsGramBridge.Show(NativeAdTypes.rewarded,
                    rewardAdUnit, _unigramSDK.IsTestMode, () =>
                {
                    adShown?.Invoke();
                },
                AdShowFailed);
            }
        }

        private bool IsAvailableAdUnit()
        {
            return !string.IsNullOrEmpty(_unigramSDK.RewardedAdUnit);
        }

        private void AdRewarded()
        {
            OnRewarded?.Invoke();

            UnigramAdsLogger.Log($"Rewarded ad successfully " +
                $"received reward by network {_currentNetwork}");
        }

        private void AdLoaded()
        {
            OnLoaded?.Invoke();

            UnigramAdsLogger.Log($"Rewarded ad loaded by network: {_currentNetwork}");
        }

        private void AdClosed()
        {
            OnClosed?.Invoke();

            UnigramAdsLogger.Log($"Rewarded ad closed by network: {_currentNetwork}");
        }

        private void AdShown()
        {
            OnShown?.Invoke();

            UnigramAdsLogger.Log($"Rewarded ad shown by network: {_currentNetwork}");
        }

        private void AdLoadFailed()
        {
            OnLoadFailed?.Invoke();

            UnigramAdsLogger.LogWarning($"Failed to load " +
                $"rewarded ad by network {_currentNetwork}");
        }

        private void AdShowFailed(string errorMessage)
        {
            OnShowFailed?.Invoke(errorMessage);

            UnigramAdsLogger.LogError("Failed to show rewarded ad by " +
                $"network {_unigramSDK.CurrentNetwork}, reason: {errorMessage}");
        }

        private void AdNonStopWatch()
        {
            OnTryNonStopWatch?.Invoke();

            UnigramAdsLogger.LogWarning($"Failed to show rewarded ad " +
                $"by network {_currentNetwork}, reason: " +
                $"try non stop watch before load new ad...");
        }

        private void AdShowExpired()
        {
            OnShowExpired?.Invoke();

            UnigramAdsLogger.LogError($"Failed to show rewarded ad " +
                $"by network {_currentNetwork}, reason: " +
                $"available ad amount per user expired, please restart app or try again later...");
        }
    }
}