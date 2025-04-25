const adsGramBridge = {
    $adsGram: {
        AdsGramController: null,

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

        showNativeAd: function(adUnit, isTestMode, successCallback, errorCallback)
        {
            if (!window.Adsgram)
            {
                const reasonPtr = adsGram.parseAllocString("SDK_NOT_INITIALIZED");

                dynCall('vi', errorCallback, [reasonPtr]);

                _free(reasonPtr);

                return;
            }

            adsGram.initAdUnit(adUnit, isTestMode);

            this.AdsGramController.show()
            .then((result) =>
            {
                if (result.done)
                {
                    console.log(`[Unigram Ads] Finished show AdsGram `+
                        `native ad, result: ${JSON.stringify(result)}`);

                    dynCall('v', successCallback);

                    return;
                }

                const errorPtr = adsGram.parseAllocString(result.description);

                dynCall('vi', errorCallback, [errorPtr]);

                -_free(errorPtr);
            })
            .catch((error) =>
            {
                const reasonPtr = adsGram.parseAllocString(
                    error.description || 'UNKNOWN_ERROR');

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

        addListener: function(eventType, callback)
        {
            if (this.isAvailableAdsGram())
            {
                const eventId = UTF8ToString(eventType);

                this.AdsGramController.addEventListener(eventId, function ()
                {
                    console.log(`[Unigram Ads] Subscribed to native `+
                        `ad event with id: ${eventId}`);

                    dynCall('v', callback);
                });
            }
        },

        removeListener: function(eventType, callback)
        {
            if (this.isAvailableAdsGram())
            {
                const eventId = UTF8ToString(eventType);

                this.AdsGramController.removeEventListener(eventId, function ()
                {
                    console.log(`[Unigram Ads] Unsubscribed from native `+
                        `ad event with id: ${eventId}`);

                    dynCall('v', callback);
                });
            }
        }
    },

    InitAdsGram: function(callback)
    {
        adsGram.initAdsGram(callback);
    },

    ShowAd: function(adUnit, isTestMode, adShown, adShowFailed)
    {
        adsGram.showNativeAd(adUnit, isTestMode, adShown, adShowFailed);
    },

    DestroyAd: function()
    {
        adsGram.destroyAd();
    },

    AddListener: function(eventType, callback)
    {
        adsGram.addListener(eventType, callback);
    },

    RemoveListener: function(eventType, callback)
    {
        adsGram.removeListener(eventType, callback);
    }
};

autoAddDeps(adsGramBridge, `$adsGram`);
mergeInto(LibraryManager.library, adsGramBridge);