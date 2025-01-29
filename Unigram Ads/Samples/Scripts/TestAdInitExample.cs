using UnityEngine;
using UnityEngine.UI;
using UnigramAds.Core;
using UnigramAds.Core.Adapters;
using UnigramAds.Common;
using UnigramAds.Utils;

namespace UnigramAds.Demo
{
    public sealed class TestAdInitExample : MonoBehaviour
    {
        [SerializeField, Space] private Button _watchAdButton;
        [SerializeField] private Button _watchRewaardAdButton;
        [SerializeField, Space] private Text _fakeBalanceBar;

        private UnigramAdsSDK _unigramAds;

        private IVideoAd _interstitialAd;
        private IRewardVideoAd _rewardAd;

        private readonly string AD_SONAR_APP_ID = "app_aaa2d5da";

        private readonly string ADS_GRAM_INTER_ID = "";
        private readonly string ADS_GRAM_REWARD_AD_ID = "";

        private readonly int REWARD_AMOUNT = 15;
        private readonly string FAKE_BALANCE_SAVE_KEY = UnigramUtils.FAKE_BALANCE_SAVE_KEY;

        private void OnDestroy()
        {
            _watchAdButton.onClick.RemoveListener(WatchAd);
            _watchRewaardAdButton.onClick.RemoveListener(WatchRewardAd);
        }

        private void Start()
        {
            if (PlayerPrefs.HasKey(FAKE_BALANCE_SAVE_KEY))
            {
                var loadedBalance = PlayerPrefs.GetInt(FAKE_BALANCE_SAVE_KEY);

                Debug.Log($"Loaded balance: {loadedBalance}");

                SetBalance(loadedBalance);
            }

            _watchAdButton.onClick.AddListener(WatchAd);
            _watchRewaardAdButton.onClick.AddListener(WatchRewardAd);

            if (!UnigramUtils.IsSupporedPlatform())
            {
                return;
            }

            _unigramAds = new UnigramAdsSDK.Builder(AD_SONAR_APP_ID,
                "interstitial_placement", "rewarded_placement")
                .WithTestMode()
                .WithAdNetwork(AdNetworkTypes.AdSonar)
                .Build((isSuccess, network) =>
                {
                    Debug.Log($"Sdk initialized with status: {isSuccess}, network: {network}");
                });

            _interstitialAd = new InterstitialAdAdapter();
            _rewardAd = new RewardAdAdapter();
        }

        private void WatchAd()
        {
            if (_interstitialAd == null)
            {
                Debug.LogWarning("Interstitial ad adapter is not exist");

                return;
            }

            _interstitialAd.Show();
        }

        private void WatchRewardAd()
        {
            if (_rewardAd == null)
            {
                Debug.LogWarning("Reward ad adapter is not exist");

                return;
            }

            _rewardAd.Show(OnRewardAdFinished);
        }

        private void SetBalance(int amount)
        {
            _fakeBalanceBar.text = $"{amount} Coins";
        }

        private void SaveBalance(int amount)
        {
            PlayerPrefs.SetInt(FAKE_BALANCE_SAVE_KEY, amount);
        }

        private void OnRewardAdFinished()
        {
            Debug.Log("Ad watched, start fetching reward");

            var currentBalance = int.Parse(_fakeBalanceBar.text);

            Debug.Log($"Current balance: {currentBalance}");

            currentBalance += REWARD_AMOUNT;

            Debug.Log($"Updated balance: {currentBalance}");

            SetBalance(currentBalance);
            SaveBalance(currentBalance);
        }
    }
}