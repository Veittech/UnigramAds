using System;
using UnigramAds.Core.Bridge;
using UnigramAds.Utils;

namespace UnigramAds.Core.Adapters
{
    public sealed class RewardAdAdapter : IRewardVideoAd
    {
        public event Action OnShowFinished;
        public event Action<string> OnShowFailed;

        public void Show()
        {
            if (!UnigramUtils.IsSupporedPlatform())
            {
                return;
            }

            Show(() => { OnShowFinished?.Invoke(); });
        }

        public void Show(Action adFinished)
        {
            var rewardAdUnit = UnigramAdsSDK.Instance.RewardedAdUnit;

            AdSonarBridge.ShowRewardAd(rewardAdUnit, () =>
            {
                UnigramAdsLogger.Log("Current ad shown");

                adFinished?.Invoke();

                OnShowFinished?.Invoke();
            },
            (errorMessage) =>
            {
                UnigramAdsLogger.LogWarning("Failed to show current ad");

                OnShowFailed?.Invoke(errorMessage);
            });
        }

        public void Destroy()
        {
            //AdsGramBridge.DestroyAd();
        }

        public void Destroy(string adUnit)
        {
            var rewardAdUnit = UnigramAdsSDK.Instance.RewardedAdUnit;

            AdSonarBridge.RemoveAdUnit(rewardAdUnit, () =>
            {
                UnigramAdsLogger.Log($"Ad unit {rewardAdUnit} removed");
            },
            (errorMessage) =>
            {
                UnigramAdsLogger.LogWarning($"Failed to remove ad unit {rewardAdUnit}");
            });
        }
    }
}