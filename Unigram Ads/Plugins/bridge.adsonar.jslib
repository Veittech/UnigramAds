const adSonarBridge = {
    $adSonar: {
        AdsSonarController: null,

        nativeEventTypes: {
            STARTED: "onStart",
            SHOWN: "onShow",
            REWARDED: "onReward",
            SHOW_FAILED: "onError",
            CLOSED: "onClose",
        },

        adTypes: {
            REWARDED: "rewarded",
            INTERSTITIAL: "interstitial"
        },

        validateAllocString: function(data)
        {
            let ptr;

            if (typeof allocate === 'undefined')
            {
                const length = lengthBytesUTF8(data) + 1;

                ptr = _malloc(length);

                stringToUTF8(data, ptr, length);

                return ptr;
            }

            return allocate(intArrayFromString(data), 'i8', ALLOC_NORMAL);
        },

        sendSonarDataToUnity: function(callId, callback, dataPtr)
        {
            if (typeof wasmTable !== 'undefined')
            {
                wasmTable.get(callback).apply(null, dataPtr);

                return;
            }

            if (typeof dynCall !== 'undefined')
            {
                dynCall(callId, callback, dataPtr);
            }
            else
            {
                return;
            }

            if (callId === 'v')
            {
                return;
            }

            _free(dataPtr);
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

                this.sendSonarDataToUnity('vi', callback, [0]);

                return;
            }

            console.log(`[Unigram Ads] Ads Sonar bridge initialized`);

            this.sendSonarDataToUnity('vi', callback, [1]);
        },

        sendAdStatusEvent: function(adType, adEventId)
        {
            let payloadEvent = JSON.stringify(
            {
                AdType: adType,
                EventId: adEventId
            });

            console.log(`[Unigram Ads] Dispatched native AdSonar `+
                `event '${payloadEvent}' to listener`);  

            SendMessage("NativeAdEventListener", "ReceiveEvent", payloadEvent);  
        },

        showRewardedAd: function(adUnit, 
            successCallback, errorCallback)
        {
            if (!this.isAvailableAdsSonar())
            {
                console.warn('[Unigram Ads] Ad Sonar sdk is not initialized');

                const errorPtr = this.validateAllocString("NOT_INITIALIZED");

                this.sendSonarDataToUnity('vi', errorCallback, [errorPtr]);

                return;
            }

            const adPlacement = UTF8ToString(adUnit);

            let adEventId;

            this.AdsSonarController.show({ adUnit: adPlacement, loader: true, 
                onStart: () =>
                {
                    adEventId = this.nativeEventTypes.STARTED;

                    console.log('[Unigram Ads] Rewarded ad started loading');

                    this.sendAdStatusEvent(this.adTypes.REWARDED, adEventId);     
                },
                onShow: () =>
                {
                    adEventId = this.nativeEventTypes.SHOWN;

                    console.log('[Unigram Ads] Rewarded ad start showing');

                    this.sendAdStatusEvent(this.adTypes.REWARDED, adEventId);
                },
                onReward: () =>
                {
                    adEventId = this.nativeEventTypes.REWARDED;

                    console.log('[Unigram Ads] Rewarded ad finished and reward claimed');

                    this.sendAdStatusEvent(this.adTypes.REWARDED, adEventId);
                },
                onError: () =>
                {
                    adEventId = this.nativeEventTypes.SHOW_FAILED;

                    console.warn('[Unigram Ads] Rewarded ad load/show failed');

                    this.sendAdStatusEvent(this.adTypes.REWARDED, adEventId);
                },
                onClose: () =>
                {
                    adEventId = this.nativeEventTypes.CLOSED;

                    console.log('[Unigram Ads] Rewarded ad closed');
                    
                    this.sendAdStatusEvent(this.adTypes.REWARDED, adEventId);
                },
            }).then((result) =>
            {
                console.log(`[Unigram Ads] Ad Sonar ad status: ${result.status}`);

                if (result.status === 'error')
                {
                    console.error(`[Unigram Ads] Failed to show rewarded ad, `+
                        `claimed status: ${result.status}, reason: ${result.message}`);
                        
                    const errorPtr = this.validateAllocString(result.message);

                    this.sendSonarDataToUnity('vi', errorCallback, [errorPtr]);

                    return;
                }

                console.log(`[Unigram Ads] Finished show Ad Sonar `+
                    `rewarded ad by event id: ${adEventId}`);

                this.sendSonarDataToUnity('v', successCallback);

                console.log(`[Unigram Ads] Rewarded Ad successfully shown`);
            });
        },

        showInterstitialAd: function(adUnit,
            successCallback, errorCallback)
        {
            if (!this.isAvailableAdsSonar())
            {
                console.warn('[Unigram Ads] Ad Sonar sdk is not initialized');

                const errorPtr = this.validateAllocString("NOT_INITIALIZED");

                this.sendSonarDataToUnity('vi', errorCallback, [errorPtr]);

                return;
            }

            const adPlacement = UTF8ToString(adUnit);

            let adEventId;

            this.AdsSonarController.show({ adUnit: adPlacement, loader: true, 
                onStart: () =>
                {
                    adEventId = this.nativeEventTypes.STARTED;

                    console.log('[Unigram Ads] Interstitial ad started loading');

                    this.sendAdStatusEvent(this.adTypes.INTERSTITIAL, adEventId);
                },
                onShow: () =>
                {
                    adEventId = this.nativeEventTypes.SHOWN;

                    console.log('[Unigram Ads] Interstitial ad start showing');

                    this.sendAdStatusEvent(this.adTypes.INTERSTITIAL, adEventId);
                },
                onError: () =>
                {
                    adEventId = this.nativeEventTypes.SHOW_FAILED;

                    console.warn('[Unigram Ads] Interstitial ad show failed');

                    this.sendAdStatusEvent(this.adTypes.INTERSTITIAL, adEventId);
                },
                onClose: () =>
                {
                    adEventId = this.nativeEventTypes.CLOSED;

                    console.log('[Unigram Ads] Interstitial ad closed');

                    this.sendAdStatusEvent(this.adTypes.INTERSTITIAL, adEventId);
                },
            }).then((result) =>
            {
                console.log(`[Unigram Ads] Ad sonar ad status: ${result.status}`);

                if (result.status === 'error')
                {
                    console.error(`[Unigram Ads] Failed to show interstitial ad, `+
                        `claimed status: ${result.status}, reason: ${result.message}`);
                        
                    const errorPtr = this.validateAllocString(result.message);

                    this.sendSonarDataToUnity('vi', errorCallback, [errorPtr]);

                    return;
                }

                console.log(`[Unigram Ads] Finished show Ad Sonar `+
                    `interstitial ad by event id: ${adEventId}`);

                this.sendSonarDataToUnity('v', successCallback);

                console.log(`[Unigram Ads] Interstitial Ad successfully shown`);

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
                    console.error(`[Unigram Ads] Failed to remove `+
                        `ad unit, reasoN: ${result.message}`);
                        
                    const errorPtr = this.validateAllocString(result.message);

                    this.sendSonarDataToUnity('vi', errorCallback, [errorPtr]);

                    return;
                }

                console.log(`[Unigram Ads] Ad unit successfully `+
                    `removed, status: ${result.status}`);

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