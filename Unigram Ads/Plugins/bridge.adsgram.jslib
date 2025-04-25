const adsGramBridge = {
    $adsGram: {
        AdsGramController: null,

        nativeEventTypes: [
            "onStart",
            "onSkip",
            "onReward",
            "onComplete",
            "onError",
            "onBannerNotFound",
            "onNonStopShow",
            "onTooLongSession"
        ],

        parseAllocString: function(data)
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

        isAvailableAdsGram: function()
        {
            return !!this.AdsGramController;
        },

        initAdsGram: function(callback)
        {
            if (!window.Adsgram)
            {
                console.warn('[Unigram Ads] Failed to initialize AdsGram bridge');

                dynCall('vi', callback, [0]);

                return;
            }

            console.log('[Unigram Ads] AdsGram bridge initialized');

            dynCall('vi', callback, [1]);
        },

        initAdUnit: function(adUnit, isTestMode)
        {
            const parsedAdUnit = UTF8ToString(adUnit);

            try
            {
                this.AdsGramController = window.Adsgram.init(
                { 
                    blockId: parsedAdUnit,
                    debug: !!isTestMode
                });

                console.log(`[Unigram Ads] Ad unit ${parsedAdUnit} `+
                    `for AdsGram bridge initialized`);
            }
            catch (error)
            {
                console.error(`[Unigram Ads] Failed to initialize ad `+
                    `unit ${parsedAdUnit} for AdsGram bridge`);
            }
        },

        showNativeAd: function(adType, adUnit, 
            isTestMode, successCallback, errorCallback)
        {
            if (!window.Adsgram)
            {
                const reasonPtr = this.parseAllocString("SDK_NOT_INITIALIZED");

                dynCall('vi', errorCallback, [reasonPtr]);

                _free(reasonPtr);

                return;
            }

            const parsedAdType = UTF8ToString(adType);

            this.initAdUnit(adUnit, isTestMode);
            this.massiveSubscribeToNativeEvents(parsedAdType);

            this.AdsGramController.show()
            .then((result) =>
            {
                if (result.done)
                {
                    console.log(`[Unigram Ads] Finished show AdsGram `+
                        `native ad, result: ${JSON.stringify(result)}`);

                    this.massiveUnSubscribeFromNativeEvents();

                    dynCall('v', successCallback);

                    return;
                }

                const errorPtr = this.parseAllocString(result.description);

                this.massiveUnSubscribeFromNativeEvents();

                dynCall('vi', errorCallback, [errorPtr]);

                _free(errorPtr);
            })
            .catch((error) =>
            {
                const reasonPtr = this.parseAllocString(
                    error.description || 'UNKNOWN_ERROR');

                this.massiveUnSubscribeFromNativeEvents();

                console.error(`[Unigram Ads] Failed to show native ad, reason: ${error}`);
                console.error(`[Unigram Ads] Show error reason: ${JSON.stringify(error, null, 4)}`);

                dynCall('vi', errorCallback, [reasonPtr]);

                _free(reasonPtr);
            });
        },

        destroyAd: function()
        {
            if (!this.isAvailableAdsGram())
            {
                console.warn(`[Unigram Ads] Sdk is not initialized`);

                return;
            }

            this.AdsGramController.destroy();
        },

        subscribeToNativeEvent: function(adType, eventId)
        {
            if (!this.isAvailableAdsGram())
            {
                return;
            }

            this.AdsGramController.addEventListener(eventId, function ()
            {
                adsGram.signEventHandler(adType, eventId);
            });
        },

        unSubscribeFromNativeEvent: function(eventId)
        {
            if (!this.isAvailableAdsGram())
            {
                return;
            }

            this.AdsGramController.removeEventListener(eventId, function ()
            {
                console.log(`[Unigram Ads] Successfully unsubscribed `+
                    `from AdsGram native event: ${eventId}`);
            });
        },

        massiveSubscribeToNativeEvents: function(adType)
        {
            this.nativeEventTypes.forEach((eventId) =>
            {
                this.subscribeToNativeEvent(adType, eventId);
            });
        },

        massiveUnSubscribeFromNativeEvents: function()
        {
            this.nativeEventTypes.forEach((eventId) =>
            {
                this.unSubscribeFromNativeEvent(eventId);
            });
        },

        signEventHandler: function(adType, adEventId)
        {
            const payloadEvent = JSON.stringify(
            {
                AdType: adType,
                EventId: adEventId
            });

            console.log(`[Unigram Ads] AdsGram native event '${payloadEvent}' pushed at listener`);

            SendMessage("NativeAdEventListener", "ReceiveEvent", payloadEvent);
        }
    },

    InitAdsGram: function(callback)
    {
        adsGram.initAdsGram(callback);
    },

    ShowAd: function(adType, adUnit, 
        isTestMode, adShown, adShowFailed)
    {
        adsGram.showNativeAd(adType, adUnit, 
            isTestMode, adShown, adShowFailed);
    },

    DestroyAd: function()
    {
        adsGram.destroyAd();
    }
};

autoAddDeps(adsGramBridge, `$adsGram`);
mergeInto(LibraryManager.library, adsGramBridge);