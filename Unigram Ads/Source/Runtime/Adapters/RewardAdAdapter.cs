using System;
using UnigramAds.Core.Bridge;
using UnigramAds.Utils;

namespace UnigramAds.Core.Adapters
{
    public sealed class RewardAdAdapter : IRewardVideoAd
    {
        private readonly UnigramAdsSDK _unigramSDK;

        public event Action OnShowFinished;
        public event Action<string> OnShowFailed;

        public RewardAdAdapter()
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
            ShowAdWithCallback(() =>
            {
                OnShowFinished?.Invoke();
            });
        }

        public void Show(Action adFinished)
        {
            ShowAdWithCallback(adFinished);
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
                UnigramAdsLogger.LogWarning("Reward ad unit is not available");

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
                UnigramAdsLogger.LogWarning("Reward ad unit is not available");

                return;
            }

            var rewardAdUnit = _unigramSDK.RewardedAdUnit;

            AdSonarBridge.RemoveAdUnit(rewardAdUnit, () =>
            {
                UnigramAdsLogger.Log($"Rewarded unit {rewardAdUnit} from AdsSonar removed");
            },
            (errorMessage) =>
            {
                UnigramAdsLogger.LogWarning($"Failed to remove rewardedad unit {rewardAdUnit} from AdsSonar");
            });
        }

        private void ShowAdWithCallback(Action adShown)
        {
            if (!UnigramUtils.IsSupporedPlatform())
            {
                return;
            }

            var rewardAdUnit = _unigramSDK.RewardedAdUnit;

            UnigramAdsLogger.Log($"Rewarded ad unit: {rewardAdUnit}, " +
                $"app id: {_unigramSDK.AppId}");

            if (!IsAvailableAdUnit())
            {
                UnigramAdsLogger.LogWarning("Reward ad unit is not available");

                return;
            }

            if (_unigramSDK.IsAvailableAdSonar)
            {
                AdSonarBridge.ShowRewardedAdByUnit(rewardAdUnit, () =>
                {
                    UnigramAdsLogger.Log("Rewarded ad successfully shown");

                    adShown?.Invoke();
                },
                (errorMessage) =>
                {
                    UnigramAdsLogger.LogWarning($"Failed to show rewarded " +
                        $"ad, reason: {errorMessage}");

                    OnShowFailed?.Invoke(errorMessage);
                });

                return;
            }

            if (_unigramSDK.IsAvailableAdsGram)
            {
                AdsGramBridge.ShowNativeAd(() =>
                {
                    UnigramAdsLogger.Log("Reward ad successfully shown");

                    adShown?.Invoke();
                },
                (errorMessage) =>
                {
                    UnigramAdsLogger.LogWarning("Failed to show "+
                        $"rewarded ad, reason: {errorMessage}");

                    OnShowFailed?.Invoke(errorMessage);
                });
            }
        }

        private bool IsAvailableAdUnit()
        {
            return !string.IsNullOrEmpty(_unigramSDK.RewardedAdUnit);
        }
    }
}