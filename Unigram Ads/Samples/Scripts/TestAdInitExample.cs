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

        private IVideoAd _interstitialAd;
        private IRewardVideoAd _rewardAd;

        private int _coinBalance;

        private readonly string AD_SONAR_INTER = "interstitial_placement";
        private readonly string AD_SONAR_REWARD = "rewarded_placement";

        private readonly int REWARD_AMOUNT = 15;
        private readonly string FAKE_BALANCE_SAVE_KEY = UnigramUtils.FAKE_BALANCE_SAVE_KEY;

        private void OnDestroy()
        {
            _watchInterstitialAd.onClick.RemoveListener(WatchAd);
            _watchRewardAd.onClick.RemoveListener(WatchRewardAd);

            if (_interstitialAd == null)
            {
                return;
            }

            _interstitialAd.OnLoaded -= InterstitialAdLoaded;
            _interstitialAd.OnClosed -= InterstitialAdClosed;
            _interstitialAd.OnShown -= InterstitialAdShown;
            _interstitialAd.OnLoadFailed -= InterstitialAdLoadFailed;
            _interstitialAd.OnShowFailed -= InterstitialAdShowFailed;

            _interstitialAd.Dispose();

            if (_rewardAd == null)
            {
                return;
            }

            _rewardAd.OnLoaded -= RewardedAdLoaded;
            _rewardAd.OnClosed -= RewardedAdClosed;
            _rewardAd.OnShown -= RewardedAdShown;
            _rewardAd.OnRewarded -= RewardedAdReceivedReward;
            _rewardAd.OnLoadFailed -= RewardedAdLoadFailed;
            _rewardAd.OnShowFailed -= RewardedAdShowFailed;

            _interstitialAd.Dispose();
        }

        private void Start()
        {
            if (PlayerPrefs.HasKey(FAKE_BALANCE_SAVE_KEY))
            {
                _coinBalance = PlayerPrefs.GetInt(FAKE_BALANCE_SAVE_KEY);

                Debug.Log($"Loaded balance: {_coinBalance}");

                SetBalance(_coinBalance);
            }

            _watchInterstitialAd.onClick.AddListener(WatchAd);
            _watchRewardAd.onClick.AddListener(WatchRewardAd);

            if (!UnigramUtils.IsSupporedPlatform())
            {
                return;
            }

            var unigramInstance = new UnigramAdsSDK.Builder(
                AD_SONAR_INTER, AD_SONAR_REWARD)
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
            Debug.Log($"Current balance: {_coinBalance}");

            _coinBalance += REWARD_AMOUNT;

            Debug.Log($"Updated balance: {_coinBalance}");

            SetBalance(_coinBalance);

            SaveBalance(_coinBalance);
        }

        private void OnInitialized(bool isSuccess,
            AdNetworkTypes currentNetwork)
        {
            _interstitialAd = new InterstitialAdAdapter();
            _rewardAd = new RewardAdAdapter();

            _interstitialAd.OnLoaded += InterstitialAdLoaded;
            _interstitialAd.OnClosed += InterstitialAdClosed;
            _interstitialAd.OnShown += InterstitialAdShown;
            _interstitialAd.OnLoadFailed += InterstitialAdLoadFailed;
            _interstitialAd.OnShowFailed += InterstitialAdShowFailed;

            _rewardAd.OnLoaded += RewardedAdLoaded;
            _rewardAd.OnClosed += RewardedAdClosed;
            _rewardAd.OnShown += RewardedAdShown;
            _rewardAd.OnRewarded += RewardedAdReceivedReward;
            _rewardAd.OnLoadFailed += RewardedAdLoadFailed;
            _rewardAd.OnShowFailed += RewardedAdShowFailed;

            Debug.Log($"Sdk initialized with status: " +
                $"{isSuccess}, network: {currentNetwork}");
        }

        private void InterstitialAdLoaded()
        {
            Debug.Log("Interstitial ad loaded");
        }

        private void InterstitialAdClosed()
        {
            Debug.Log("Interstitial ad closed");
        }

        private void InterstitialAdShown()
        {
            Debug.Log("Interstitial ad shown");
        }

        private void InterstitialAdLoadFailed()
        {
            Debug.LogWarning($"Failed to load interstitial ad");
        }

        private void InterstitialAdShowFailed(string error)
        {
            Debug.LogWarning($"Failed to show interstitial ad, reason: {error}");
        }

        private void RewardedAdLoaded()
        {
            Debug.Log("Rewarded ad successfully loaded");
        }

        private void RewardedAdClosed()
        {
            Debug.Log("Rewarded ad successfully closed");
        }

        private void RewardedAdShown()
        {
            Debug.Log("Rewarded ad successfully shown");
        }

        private void RewardedAdReceivedReward()
        {
            Debug.Log("Rewarded ad shown and award claimed");
        }

        private void RewardedAdLoadFailed()
        {
            Debug.LogWarning($"Failed to load rewarded ad");
        }

        private void RewardedAdShowFailed(string error)
        {
            Debug.LogWarning($"Failed to show rewarded ad, reason: {error}");
        }
    }
}