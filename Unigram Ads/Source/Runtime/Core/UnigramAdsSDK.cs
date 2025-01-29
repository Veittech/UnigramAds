using System;
using System.Collections.Generic;
using System.Linq;
using UnigramAds.Common;
using UnigramAds.Core.Bridge;
using UnigramAds.Utils;
using UnityEngine;

namespace UnigramAds.Core
{
    public sealed class UnigramAdsSDK
    {
        private static UnigramAdsSDK _instance;

        public static UnigramAdsSDK Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new NullReferenceException($"{UnigramAdsLogger.PREFIX} Sdk is not ready for use");
                }

                return _instance;
            }
        }

        public string AppId { get; private set; }

        public string InterstitialAdUnit { get; private set; }
        public string RewardedAdUnit { get; private set; }

        public bool IsInitialized { get; private set; }
        public bool IsTestMode { get; private set; }

        public IReadOnlyList<AdNetworkTypes> ActiveAdNetworks { get; private set; }

        public AdTypes DebugAdType { get; private set; }

        public bool IsAvailableAdSonar => ActiveAdNetworks.Contains(AdNetworkTypes.AdSonar);
        public bool IsAvailableAdsGram => ActiveAdNetworks.Contains(AdNetworkTypes.AdsGram);

        private UnigramAdsSDK(Builder builder)
        {
            this.IsInitialized = builder.IsInitialized;
            this.IsTestMode = builder.IsTestMode;

            this.AppId = builder.AppId;
            this.InterstitialAdUnit = builder.InterstitialAdUnit;
            this.RewardedAdUnit = builder.RewardedAdUnit;
        }

        public sealed class Builder
        {
            internal string AppId { get; private set; }

            internal string InterstitialAdUnit { get; private set; }
            internal string RewardedAdUnit { get; private set; }

            internal bool IsInitialized { get; private set; }
            internal bool IsTestMode { get; private set; }

            internal AdTypes DebugAdType { get; private set; }

            internal List<AdNetworkTypes> ActiveAdNetworks { get; private set; }

            /// <summary>
            /// Init AdSonar Bridge
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="interAdUnit"></param>
            /// <param name="rewardAdUnit"></param>
            public Builder(string appId, 
                string interAdUnit, string rewardAdUnit)
            {
                this.AppId = appId;

                this.InterstitialAdUnit = interAdUnit;
                this.RewardedAdUnit = rewardAdUnit;

                this.ActiveAdNetworks = new();
            }

            /// <summary>
            /// Init AdSonar Bridge
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="interAdUnit"></param>
            /// <param name="rewardAdUnit"></param>
            public Builder(string appId,
                AdTypes adType, string targetAdUnit)
            {
                this.AppId = appId;

                this.ActiveAdNetworks = new();

                if (adType == AdTypes.FullscreenStatic)
                {
                    this.InterstitialAdUnit = targetAdUnit;

                    return;
                }

                this.RewardedAdUnit = targetAdUnit;
            }

            /// <summary>
            /// Init AdsGram Bridge
            /// </summary>
            /// <param name="appId"></param>
            public Builder(string appId)
            {
                this.AppId = appId;

                this.ActiveAdNetworks = new();
            }

            public Builder WithTestMode()
            {
                this.IsTestMode = true;

                UnigramAdsLogger.Enabled();

                UnigramAdsLogger.Log($"Enabled test mode");

                return this;
            }

            public Builder WithTestMode(AdTypes adType)
            {
                if (adType == AdTypes.None)
                {
                    Debug.Log($"{UnigramAdsLogger.PREFIX} " +
                        $"Unsuppored ad type, test mode disabled");

                    return this;
                }

                this.DebugAdType = adType;

                WithTestMode();

                UnigramAdsLogger.Log($"Enabled test mode for ad type: {adType}");

                return this;
            }

            public Builder WithAdNetwork(AdNetworkTypes adNetwork)
            {
                var foundedNetwork = this.ActiveAdNetworks.Find(network => network == adNetwork);

                UnigramAdsLogger.Log($"Available networks: {JsonUtility.ToJson(ActiveAdNetworks)}");
                UnigramAdsLogger.Log($"Founded network: {foundedNetwork}, current: {adNetwork}");

                if (foundedNetwork == adNetwork)
                {
                    Debug.LogWarning($"Ad network {adNetwork} already exist");

                    //return this;
                }

                this.ActiveAdNetworks.Add(adNetwork);

                return this;
            }

            public UnigramAdsSDK Build(Action<bool,
                AdNetworkTypes> initializationFinished)
            {
                if (!UnigramUtils.IsSupporedPlatform())
                {
                    return null;
                }

                if (_instance == null)
                {
                    _instance = new UnigramAdsSDK(this);
                }

                var isActiveAdSonar = this.ActiveAdNetworks.Contains(AdNetworkTypes.AdSonar);
                var isActiveAdsGram = this.ActiveAdNetworks.Contains(AdNetworkTypes.AdsGram);

                Debug.Log($"AdSonar status: {isActiveAdSonar}, " +
                    $"AdsGram status: {isActiveAdsGram}");

                if (isActiveAdSonar)
                {
                    AdSonarBridge.Init(this.AppId, 
                        this.IsTestMode, (isSuccess) =>
                    {
                        initializationFinished?.Invoke(isSuccess,
                            AdNetworkTypes.AdSonar);

                        IsInitialized = isSuccess;

                        if (IsInitialized)
                        {
                            UnigramAdsLogger.Log("AdSonar bridge initialized");
                        }
                    });
                }

                if (isActiveAdsGram)
                {
                    AdsGramBridge.Init(this.AppId, (isSuccess) =>
                    {
                        this.InterstitialAdUnit = this.AppId;
                        this.RewardedAdUnit = this.AppId;

                        initializationFinished?.Invoke(isSuccess,
                            AdNetworkTypes.AdsGram);

                        IsInitialized = isSuccess;

                        if (IsInitialized)
                        {
                            UnigramAdsLogger.Log("AdsGram bridge initialized");
                        }
                    });
                }

                return _instance;
            }
        }
    }
}