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
                AdsGramBridge.DestroyNativeAd();

                UnigramAdsLogger.Log($"Rewarded ad unit " +
                    $"{rewardAdUnit} from AdsGram removed!");
            }

            if (_unigramSDK.IsAvailableAdSonar)
            {
                AdSonarBridge.RemoveAdUnit(rewardAdUnit, () =>
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
                OnShowFinished = adShown;
            }

            if (_unigramSDK.IsAvailableAdSonar)
            {
                AdSonarBridge.ShowRewardedAdByUnit(
                    rewardAdUnit, OnAdShown, OnAdShowFailed);
            }

            if (_unigramSDK.IsAvailableAdsGram)
            {
                AdsGramBridge.ShowNativeAd(rewardAdUnit, 
                    _unigramSDK.IsTestMode, OnAdShown, OnAdShowFailed);
            }
        }

        private void OnAdShown()
        {
            OnShowFinished?.Invoke();

            UnigramAdsLogger.Log($"Reward ad successfully shown " +
                $"by network {_unigramSDK.CurrentNetwork}");
        }

        private void OnAdShowFailed(string errorMessage)
        {
            OnShowFailed?.Invoke(errorMessage);

            UnigramAdsLogger.LogWarning("Failed to show rewarded ad by " +
                $"network {_unigramSDK.CurrentNetwork}, reason: {errorMessage}");
        }

        private bool IsAvailableAdUnit()
        {
            return !string.IsNullOrEmpty(_unigramSDK.RewardedAdUnit);
        }
    }
}