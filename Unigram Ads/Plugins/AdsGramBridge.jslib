const adsGramBridge = {
    $adsGram: {
        AdsGramController: null,

        parseAllocString: function(data)
        {
            let ptr;

            if (typeof allocate === 'undefined')
            {
                console.log(`Detected Unity version 2023+`);

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
                console.warn('Failed to initialize AdsGram bridge');

                dynCall('vi', callback, [0]);

                return;
            }

            console.log('AdsGram bridge initialized');

            dynCall('vi', callback, [1]);
        },

        initAdUnit: function(adUnit)
        {
            const parsedAdUnit = UTF8ToString(adUnit);

            try
            {
                this.AdsGramController = window.Adsgram.init(
                { 
                    blockId: parsedAdUnit
                });

                console.log(`Ad unit ${parsedAdUnit} for AdsGram bridge initialized`);
            }
            catch (error)
            {
                console.error(`Failed to initialize ad unit ${parsedAdUnit} for AdsGram bridge`);
            }
        },

        showNativeAd: function(adUnit, successCallback, errorCallback)
        {
            if (!window.Adsgram)
            {
                const reasonPtr = adsGram.parseAllocString("SDK_NOT_INITIALIZED");

                dynCall('vi', errorCallback, [reasonPtr]);

                _free(reasonPtr);

                return;
            }

            adsGram.initAdUnit(adUnit);

            this.AdsGramController.show().then((result) =>
            {
                if (result.done)
                {
                    console.log(`Reward ads successfully shown, result: ${result}`);
                    console.log(`JSON Result: ${JSON.stringify(result)}`);

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

                console.error(`Failed to show ad, reason: ${error}`);
                console.error(`Error reason: ${JSON.stringify(error, null, 4)}`);

                dynCall('vi', errorCallback, [reasonPtr]);

                _free(reasonPtr);
            });
        },

        destroyAd: function()
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
                const eventIdPtr = adsGram.parseAllocString(eventId);

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
                const eventIdPtr = adsGram.parseAllocString(eventId);

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

    InitAdsGram: function(callback)
    {
        adsGram.initAdsGram(callback);
    },

    ShowAd: function(adUnit, adShown, adShowFailed)
    {
        adsGram.showNativeAd(adUnit, adShown, adShowFailed);
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