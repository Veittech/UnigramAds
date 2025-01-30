using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnigramAds.Common;
using UnigramAds.Core.Bridge;
using UnigramAds.Utils;

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

        public AdNetworkTypes CurrentNetwork { get; private set; }

        public AdTypes DebugAdType { get; private set; }

        public bool IsAvailableAdSonar => this.ActiveAdNetworks.Contains(AdNetworkTypes.AdSonar);
        public bool IsAvailableAdsGram => this.ActiveAdNetworks.Contains(AdNetworkTypes.AdsGram);

        private UnigramAdsSDK(Builder builder)
        {
            this.IsInitialized = builder.IsInitialized;
            this.IsTestMode = builder.IsTestMode;

            this.DebugAdType = builder.DebugAdType;

            this.AppId = builder.AppId;

            this.InterstitialAdUnit = builder.InterstitialAdUnit;
            this.RewardedAdUnit = builder.RewardedAdUnit;

            this.ActiveAdNetworks = builder.ActiveAdNetworks;
            this.CurrentNetwork = builder.CurrentNetwork;
        }

        public sealed class Builder
        {
            internal string AppId { get; private set; }

            internal string InterstitialAdUnit { get; private set; }
            internal string RewardedAdUnit { get; private set; }

            internal bool IsInitialized { get; private set; }
            internal bool IsTestMode { get; private set; }

            internal AdTypes DebugAdType { get; private set; }

            internal AdNetworkTypes CurrentNetwork { get; private set; }

            internal List<AdNetworkTypes> ActiveAdNetworks { get; private set; }

            public Builder(string interstitialAdUnit, 
                string rewardedAdUnit)
            {
                this.InterstitialAdUnit = interstitialAdUnit;
                this.RewardedAdUnit = rewardedAdUnit;

                this.ActiveAdNetworks = new();
            }

            public Builder(AdTypes adType, string targetAdUnit)
            {
                this.ActiveAdNetworks = new();

                if (adType == AdTypes.FullscreenStatic)
                {
                    this.InterstitialAdUnit = targetAdUnit;

                    return;
                }

                this.RewardedAdUnit = targetAdUnit;
            }

            public Builder(string appId, 
                string interstitialAdUnit, string rewardAdUnit)
            {
                this.AppId = appId;

                this.InterstitialAdUnit = interstitialAdUnit;
                this.RewardedAdUnit = rewardAdUnit;

                this.ActiveAdNetworks = new();
            }

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

                if (isActiveAdSonar)
                {
                    AdSonarBridge.Init((isSuccess) =>
                    {
                        initializationFinished?.Invoke(isSuccess,
                            AdNetworkTypes.AdSonar);

                        this.IsInitialized = isSuccess;

                        this.CurrentNetwork = AdNetworkTypes.AdSonar;

                        if (this.IsInitialized)
                        {
                            UnigramAdsLogger.Log("AdSonar bridge initialized");
                        }
                    });
                }

                if (isActiveAdsGram)
                {
                    AdsGramBridge.Init((isSuccess) =>
                    {
                        initializationFinished?.Invoke(isSuccess,
                            AdNetworkTypes.AdsGram);

                        this.IsInitialized = isSuccess;

                        this.CurrentNetwork = AdNetworkTypes.AdsGram;

                        if (this.IsInitialized)
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