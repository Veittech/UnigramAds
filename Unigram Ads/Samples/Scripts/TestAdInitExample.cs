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
        [SerializeField, Space] private Button _watchInterstitialAd;
        [SerializeField] private Button _watchRewardAd;
        [SerializeField, Space] private Text _fakeBalanceBar;

        private UnigramAdsSDK _unigramAds;

        private IVideoAd _interstitialAd;
        private IRewardVideoAd _rewardAd;

        private readonly string AD_SONAR_APP_ID = "app_aaa2d5da";

        private readonly int REWARD_AMOUNT = 15;
        private readonly string FAKE_BALANCE_SAVE_KEY = UnigramUtils.FAKE_BALANCE_SAVE_KEY;

        private void OnDestroy()
        {
            _watchInterstitialAd.onClick.RemoveListener(WatchAd);
            _watchRewardAd.onClick.RemoveListener(WatchRewardAd);
        }

        private void Start()
        {
            if (PlayerPrefs.HasKey(FAKE_BALANCE_SAVE_KEY))
            {
                var loadedBalance = PlayerPrefs.GetInt(FAKE_BALANCE_SAVE_KEY);

                Debug.Log($"Loaded balance: {loadedBalance}");

                SetBalance(loadedBalance);
            }

            _watchInterstitialAd.onClick.AddListener(WatchAd);
            _watchRewardAd.onClick.AddListener(WatchRewardAd);

            if (!UnigramUtils.IsSupporedPlatform())
            {
                return;
            }

            _unigramAds = new UnigramAdsSDK.Builder(AD_SONAR_APP_ID,
                "interstitial_placement", "rewarded_placement")
                .WithTestMode()
                .WithAdNetwork(AdNetworkTypes.AdSonar)
                .Build(OnInitialized);
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

        private void OnInitialized(bool isSuccess,
            AdNetworkTypes currentNetwork)
        {
            _interstitialAd = new InterstitialAdAdapter();
            _rewardAd = new RewardAdAdapter();

            Debug.Log($"Sdk initialized with status: " +
                $"{isSuccess}, network: {currentNetwork}");
        }
    }
}