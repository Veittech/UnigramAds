const adSonarBridge = {
    $adSonar: {
        AdsSonarController: null,

        isAvailableAdsSonar: function()
        {
            return !!this.AdsSonarController;
        },

        initAdSonar: function(appId, isTesting, callback)
        {
            this.AdsSonarController = window.Sonar;

            if (!this.isAvailableAdsSonar())
            {
                console.warn(`Failed to initialize Ad Sonar bridge`);

                dynCall('vi', callback, [0]);

                return;
            }

            console.log(`Ads Sonar bridge initialized`);

            dynCall('vi', callback, [1]);
        },

        showRewardedAd: function(adUnit, 
            successCallback, errorCallback)
        {
            if (!this.isAvailableAdsSonar())
            {
                console.warn('Ad Sonar sdk is not initialized');

                const errorPtr = allocate(intArrayFromString(
                    "NOT_INITIALIZED"), 'i8', ALLOC_NORMAL);

                dynCall('vi', errorCallback, [errorPtr]);

                _free(errorPtr);

                return;
            }

            const adPlacement = UTF8ToString(adUnit);

            this.AdsSonarController.show({ adUnit: adPlacement, loader: true, 
                onStart: () =>
                {
                    console.log('Rewarded ad started loading');
                },
                onShow: () =>
                {
                    console.log('Rewarded ad start showing');
                },
                onError: () =>
                {
                    console.warn('Rewarded ad show failed');
                },
                onClose: () =>
                {
                    console.log('Rewarded ad closed');
                },
            }).then((result) =>
            {
                console.log(`Ad Sonar ad status: ${result.status}`);

                if (result.status === 'error')
                {
                    console.error(`Failed to show rewarded ad, claimed status: `+
                        `${result.status}, reason: ${result.message}`);
                        
                    const errorPtr = allocate(intArrayFromString(
                        result.message), 'i8', ALLOC_NORMAL);

                    dynCall('vi', errorCallback, [errorPtr]);

                    _free(errorPtr);

                    return;
                }

                dynCall('v', successCallback);

                console.log(`Rewarded Ad successfully shown`);
            });
        },

        showInterstitialAd: function(adUnit,
            successCallback, errorCallback)
        {
            if (!this.isAvailableAdsSonar())
            {
                console.warn('Ad Sonar sdk is not initialized');

                const errorPtr = allocate(intArrayFromString(
                    "NOT_INITIALIZED"), 'i8', ALLOC_NORMAL);

                dynCall('vi', errorCallback, [errorPtr]);

                _free(errorPtr);

                return;
            }

            const adPlacement = UTF8ToString(adUnit);

            this.AdsSonarController.show({ adUnit: adPlacement, loader: true, 
                onStart: () =>
                {
                    console.log('Interstitial ad started loading');
                },
                onShow: () =>
                {
                    console.log('Interstitial ad start showing');
                },
                onError: () =>
                {
                    console.warn('Interstitial ad show failed');
                },
                onClose: () =>
                {
                    console.log('Interstitial ad closed');
                },
            }).then((result) =>
            {
                console.log(`Ad sonar ad status: ${result.status}`);

                if (result.status === 'error')
                {
                    console.error(`Failed to show interstitial ad, claimed status: `+
                        `${result.status}, reason: ${result.message}`);
                        
                    const errorPtr = allocate(intArrayFromString(
                        result.message), 'i8', ALLOC_NORMAL);

                    dynCall('vi', errorCallback, [errorPtr]);

                    _free(errorPtr);

                    return;
                }

                console.log(`Interstitial Ad successfully shown`);

                dynCall('v', successCallback);

                return;
            });
        },

        removeAd: function(adUnit,
            successCallback, errorCallback)
        {
            if (!this.isAvailableAdsSonar())
            {
                console.warn(`Ad Sonar sdk is not initialized`);

                return;
            } 

            const adPlacement = UTF8ToString(adUnit);

            this.AdsSonarController.remove({ adUnit: adUnit }).then((result) =>
            {   
                if (result.status === 'error')
                {
                    console.error(`Failed to remove ad unit, reasoN: ${result.message}`);
                        
                    const errorPtr = allocate(intArrayFromString(
                        result.message), 'i8', ALLOC_NORMAL);

                    dynCall('vi', errorCallback, [errorPtr]);

                    _free(errorPtr);

                    return;
                }

                console.log(`Ad unit successfully removed, status: ${result.status}`);

                dynCall('v', successCallback);
            });
        }
    },

    InitAdSonar: function(appId, isTesting, callback)
    {
        adSonar.initAdSonar(appId, isTesting, callback);
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