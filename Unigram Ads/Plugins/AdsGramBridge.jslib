const adsGramBridge = {
    $adsGram: {
        AdsGramController: null,

        isAvailableAdsGram: function()
        {
            return !!this.AdsGramController;
        },

        initAdsGram: function(adUnit, 
            isTesting, testingType, callback)
        {
            const parsedAdUnit = UTF8ToString(adUnit);
            const debugMode = UTF8ToString(testingType);

            if (this.AdsGramController)
            {
                console.warn('Sdk already initialized');

                return;
            }

            try
            {
                this.AdsGramController = window.Adsgram.init(
                { 
                    blockId: parsedAdUnit,
                    debug: !!isTesting,
                    debugBannerType: debugMode
                });

                dynCall('vi', callback, [1]);
            }
            catch (error)
            {
                console.error(`Initialization AdsGram failed, ${error}`);

                dynCall('vi', callback, [0]);
            }
        },

        showRewardAd: function(successCallback, errorCallback)
        {
            if (!this.isAvailableAdsGram())
            {
                const reasonPtr = allocate(intArrayFromString(
                    "SDK_NOT_INITIALIZED"), 'i8', ALLOC_NORMAL);

                dynCall('vi', errorCallback, [reasonPtr]);

                _free(reasonPtr);

                return;
            }

            this.AdsGramController.show().then((result) =>
            {
                if (result.done)
                {
                    console.log(`Reward ads successfully shown, result: ${result}`);
                    console.log(`JSON Result: ${JSON.stringify(result)}`);

                    dynCall('v', successCallback);

                    return;
                }

                const errorPtr = allocate(intArrayFromString(
                    result.description), 'i8', ALLOC_NORMAL);

                dynCall('vi', errorCallback, [errorPtr]);

                -_free(errorPtr);
            })
            .catch((error) =>
            {
                const reasonPtr = allocate(intArrayFromString(
                    error.description || 'UNKNOWN_ERROR'), 'i8', ALLOC_NORMAL);

                console.error(`Failed to show ad, reason: ${error}`);
                console.error(`Error reason: ${JSON.stringify(result, null, 4)}`);

                dynCall('vi', errorCallback, [reasonPtr]);

                _free(reasonPtr);
            });
        },

        destroyRewardAd: function()
        {
            if (!this.isAvailableAdsGram())
            {
                console.warn(`Sdk is not initialized`);

                return;
            }

            this.AdsGramController.destroy();
        },

        addListener: function(eventType, callback)
        {
            if (this.isAvailableAdsGram())
            {
                const eventId = UTF8ToString(eventType);
                const eventIdPtr = allocate(intArrayFromString(
                    eventId), 'i8', ALLOC_NORMAL);

                this.AdsGramController.addEventListener(eventId, function ()
                {
                    console.log(`Invoked event with id: ${eventId}`);

                    dynCall('vi', callback, [eventIdPtr]);

                    _free(eventIdPtr);
                });
            }
        },

        removeListener: function(eventType, callback)
        {
            if (this.isAvailableAdsGram())
            {
                const eventId = UTF8ToString(eventType);
                const eventIdPtr = allocate(intArrayFromString(
                    eventId), 'i8', ALLOC_NORMAL);

                this.AdsGramController.removeEventListener(
                    UTF8ToString(eventType), function ()
                {
                    console.log(`Unsubscribed from event with id: ${eventId}`);

                    dynCall('vi', callback, [eventIdPtr]);

                    _free(eventIdPtr);
                });
            }
        }
    },

    InitAdsGram: function(appId, 
        isTesting, testingType, callback)
    {
        adsGram.initAdsGram(appId, isTesting, testingType, callback);
    },

    ShowRewardAd: function(adShown, adShowFailed)
    {
        adsGram.showRewardAd(adShown, adShowFailed);
    },

    DestroyRewardAd: function()
    {
        adsGram.destroyRewardAd();
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