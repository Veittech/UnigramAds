const adSonarBridge = {
    $adSonar: {
        AdsSonarController: null,

        isAvailableAdsSonar: function()
        {
            return !!this.AdsSonarController;
        },

        initAdSonar: function(appId, isTesting, callback)
        {
            const parsedAppId = UTF8ToString(appId);

            let newAdSonarScript = `https://static.sonartech.io/lib/1.0.0/sonar.js?appId=${parsedAppId}`;

            if (isTesting)
            {
                newAdSonarScript += "&isDebug=true";

                console.log('Activated debug mode for AdSonar bridge');
            }
            
            let newLibInstance = document.createElement("script");

            newLibInstance.id = "ad-sonar-lib";
            newLibInstance.scr = newAdSonarScript;
            newLibInstance.async = true;

            document.head.appendChild(newLibInstance);

            this.AdsSonarController = window.Sonar;

            console.log(`Created ad sonar instance: ${window.Sonar}, url: ${newAdSonarScript}`);

            if (!this.isAvailableAdsSonar())
            {
                console.warn(`Failed to initialize Ad Sonar bridge`);

              //  dynCall('vi', callback, [0]);

               // return;
            }

            console.log(`Ads Sonar bridge initialized`);

            dynCall('vi', callback, [1]);
        },

        showAd: function(adUnit, successCallback, errorCallback)
        {
            if (!this.isAvailableAdsSonar())
            {
                console.warn('Ad Sonar sdk is not initialized');

                return;
            }

            const adPlacement = UTF8ToString(adUnit);

            this.AdsSonarController.show({ adUnit: adPlacement }).then((result) =>
            {
                if (result.status === 'error')
                {
                    console.error(`Failed to show ad`);
                        
                    const errorPtr = allocate(intArrayFromString(
                            result.message), 'i8', ALLOC_NORMAL);

                    dynCall('vi', errorCallback, [errorPtr]);

                    _free(errorPtr);

                    return;
                }

                console.log(`Ad successfully shown, status: ${result.status}`);

                dynCall('v', successCallback);
            })
            .catch((error) =>
            {
                const errorPtr = allocate(
                        intArrayFromString(error), 'i8', ALLOC_NORMAL);

                dynCall('vi', errorCallback, [errorPtr]);

                _free(errorPtr);
            });
        },

        removeAd: function(adUnit, successCallback, errorCallback)
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
                    console.error(`Failed to remove ad unit`);
                        
                    const errorPtr = allocate(intArrayFromString(
                            result.message), 'i8', ALLOC_NORMAL);

                    dynCall('vi', errorCallback, [errorPtr]);

                    _free(errorPtr);

                    return;
                }

                console.log(`Ad unit successfully removed, status: ${result.status}`);

                dynCall('v', successCallback);
            })  
            .catch((error) =>
            {
                const errorPtr = allocate(
                        intArrayFromString(error), 'i8', ALLOC_NORMAL);

                dynCall('vi', errorCallback, [errorPtr]);

                _free(errorPtr);
            });
        }
    },

    InitAdSonar: function(appId, isTesting, callback)
    {
        adSonar.initAdSonar(appId, isTesting, callback);
    },

    ShowAd: function(adUnit, adShown, adShowFailed)
    {
        adSonar.showAd(adUnit, adShown, adShowFailed);
    },

    RemoveAd: function(adUnit, adRemoved, adRemoveFailed)
    {
        adSonar.removeAd(adUnit, adRemoved, adRemoveFailed);
    }
};

autoAddDeps(adSonarBridge, `$adSonar`);
mergeInto(LibraryManager.library, adSonarBridge);