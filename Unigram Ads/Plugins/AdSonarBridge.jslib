const adSonarBridge = {
    $adSonar: {
        AdsSonarController: null,

        validateAllocString: function(data)
        {
            let ptr;

            if (typeof allocate === 'undefined')
            {
                console.log(`[Unigram Ads] Detected Unity version 2023+`);

                const length = lengthBytesUTF8(data) + 1;

                ptr = _malloc(length);

                stringToUTF8(data, ptr, length);

                return ptr;
            }

            return allocate(intArrayFromString(data), 'i8', ALLOC_NORMAL);
        },

        isAvailableAdsSonar: function()
        {
            return !!this.AdsSonarController;
        },

        initAdSonar: function(appId, isTestMode, callback)
        {
            this.AdsSonarController = window.Sonar;

            if (!this.isAvailableAdsSonar())
            {
                console.warn(`[Unigram Ads] Failed to initialize Ad Sonar bridge`);

                dynCall('vi', callback, [0]);

                return;
            }

            console.log(`[Unigram Ads] Ads Sonar bridge initialized`);

            dynCall('vi', callback, [1]);
        },

        showRewardedAd: function(adUnit, 
            successCallback, errorCallback)
        {
            if (!this.isAvailableAdsSonar())
            {
                console.warn('[Unigram Ads] Ad Sonar sdk is not initialized');

                const errorPtr = adSonar.validateAllocString("NOT_INITIALIZED");

                dynCall('vi', errorCallback, [errorPtr]);

                _free(errorPtr);

                return;
            }

            const adPlacement = UTF8ToString(adUnit);

            let adEventId;

            this.AdsSonarController.show({ adUnit: adPlacement, loader: true, 
                onStart: () =>
                {
                    adEventId = "onStart";

                    console.log('[Unigram Ads] Rewarded ad started loading');

                    SendMessage("NativeAdEventListener", "ReceiveEvent", adEventId);

                    console.log(`[Unigram Ads] Native event '${adEventId}' pushed at listener`);
                
                },
                onShow: () =>
                {
                    adEventId = "onShow";

                    console.log('[Unigram Ads] Rewarded ad start showing');

                    SendMessage("NativeAdEventListener", "ReceiveEvent", adEventId);

                    console.log(`[Unigram Ads] Native event '${adEventId}' pushed at listener`);
                },
                onReward: () =>
                {
                    adEventId = "onReward";

                    console.log('[Unigram Ads] Rewarded ad finished and reward claimed');

                    SendMessage("NativeAdEventListener", "ReceiveEvent", adEventId);

                    console.log(`[Unigram Ads] Native event '${adEventId}' pushed at listener`);
                },
                onError: () =>
                {
                    adEventId = "onError";

                    console.warn('[Unigram Ads] Rewarded ad load/show failed');

                    SendMessage("NativeAdEventListener", "ReceiveEvent", adEventId);

                    console.log(`[Unigram Ads] Native event '${adEventId}' pushed at listener`);
                },
                onClose: () =>
                {
                    adEventId = "onClose";

                    console.log('[Unigram Ads] Rewarded ad closed');
                    
                    SendMessage("NativeAdEventListener", "ReceiveEvent", adEventId);

                    console.log(`[Unigram Ads] Native event '${adEventId}' pushed at listener`);
                },
            }).then((result) =>
            {
                console.log(`[Unigram Ads] Ad Sonar ad status: ${result.status}`);

                if (result.status === 'error')
                {
                    console.error(`[Unigram Ads] Failed to show rewarded ad, claimed status: `+
                        `${result.status}, reason: ${result.message}`);
                        
                    const errorPtr = adSonar.validateAllocString(result.message);

                    dynCall('vi', errorCallback, [errorPtr]);

                    _free(errorPtr);

                    return;
                }

                console.log(`[Unigram Ads] Finished show Ad Sonar `+
                    `rewarded ad by event id: ${adEventId}`);

                dynCall('v', successCallback);

                console.log(`[Unigram Ads] Rewarded Ad successfully shown`);
            });
        },

        showInterstitialAd: function(adUnit,
            successCallback, errorCallback)
        {
            if (!this.isAvailableAdsSonar())
            {
                console.warn('[Unigram Ads] Ad Sonar sdk is not initialized');

                const errorPtr = adSonar.validateAllocString("NOT_INITIALIZED");

                dynCall('vi', errorCallback, [errorPtr]);

                _free(errorPtr);

                return;
            }

            const adPlacement = UTF8ToString(adUnit);

            let adEventId;

            this.AdsSonarController.show({ adUnit: adPlacement, loader: true, 
                onStart: () =>
                {
                    adEventId = "onStart";

                    console.log('[Unigram Ads] Interstitial ad started loading');

                    SendMessage("NativeAdEventListener", "ReceiveEvent", adEventId);

                    console.log(`[Unigram Ads] Native event '${adEventId}' pushed at listener`);
                },
                onShow: () =>
                {
                    adEventId = "onShow";

                    console.log('[Unigram Ads] Interstitial ad start showing');

                    SendMessage("NativeAdEventListener", "ReceiveEvent", adEventId);

                    console.log(`[Unigram Ads] Native event '${adEventId}' pushed at listener`);
                },
                onError: () =>
                {
                    adEventId = "onError";

                    console.warn('[Unigram Ads] Interstitial ad show failed');

                    SendMessage("NativeAdEventListener", "ReceiveEvent", adEventId);

                    console.log(`[Unigram Ads] Native event '${adEventId}' pushed at listener`);
                },
                onClose: () =>
                {
                    adEventId = "onClose";

                    console.log('[Unigram Ads] Interstitial ad closed');

                    SendMessage("NativeAdEventListener", "ReceiveEvent", adEventId);

                    console.log(`[Unigram Ads] Native event '${adEventId}' pushed at listener`);
                },
            }).then((result) =>
            {
                console.log(`[Unigram Ads] Ad sonar ad status: ${result.status}`);

                if (result.status === 'error')
                {
                    console.error(`[Unigram Ads] Failed to show interstitial ad, `+
                        `claimed status: ${result.status}, reason: ${result.message}`);
                        
                    const errorPtr = adSonar.validateAllocString(result.message);

                    dynCall('vi', errorCallback, [errorPtr]);

                    _free(errorPtr);

                    return;
                }

                console.log(`[Unigram Ads] Interstitial Ad successfully shown`);

                console.log(`[Unigram Ads] Finished show Ad Sonar `+
                    `interstitial ad by event id: ${adEventId}`);

                dynCall('v', successCallback);

                return;
            });
        },

        removeAd: function(adUnit,
            successCallback, errorCallback)
        {
            if (!this.isAvailableAdsSonar())
            {
                console.warn(`[Unigram Ads] Ad Sonar sdk is not initialized`);

                return;
            } 

            const adPlacement = UTF8ToString(adUnit);

            this.AdsSonarController.remove({ adUnit: adUnit }).then((result) =>
            {   
                if (result.status === 'error')
                {
                    console.error(`[Unigram Ads] Failed to remove ad unit, reasoN: ${result.message}`);
                        
                    const errorPtr = adSonar.validateAllocString(result.message);

                    dynCall('vi', errorCallback, [errorPtr]);

                    _free(errorPtr);

                    return;
                }

                console.log(`[Unigram Ads] Ad unit successfully removed, status: ${result.status}`);

                dynCall('v', successCallback);
            });
        }
    },

    InitAdSonar: function(appId, isTestMode, callback)
    {
        adSonar.initAdSonar(appId, isTestMode, callback);
    },

    ShowRewardedAd: function(adUnit, adShown, adShowFailed)
    {
        adSonar.showRewardedAd(adUnit, adShown, adShowFailed);
    },

    ShowInterstitialAd: function(adUnit, adShown, adShowFailed)
    {
        adSonar.showInterstitialAd(adUnit, adShown, adShowFailed);
    },

    RemoveAd: function(adUnit, adRemoved, adRemoveFailed)
    {
        adSonar.removeAd(adUnit, adRemoved, adRemoveFailed);
    }
};

autoAddDeps(adSonarBridge, `$adSonar`);
mergeInto(LibraryManager.library, adSonarBridge);