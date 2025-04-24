const bridgeUtils = {
    $bridgeUtils: {
        logInfo: function(message)
        {
            console.log(`[Unigram Ads] ${UTF8ToString(message)}`);
        },

        logWarning: function(message)
        {
            console.warn(`[Unigram Ads] ${UTF8ToString(message)}`);
        },

        logError: function(message)
        {
            console.error(`[Unigram Ads] ${UTF8ToString(message)}`);
        },

        allocString: function(data)
        {
            let ptr;

            if (typeof allocate === 'undefined')
            {
                bridgeUtils.logInfo("Detected Unity version 2023+");

                const length = lengthBytesUTF8(data) + 1;

                ptr = _malloc(length);

                stringToUTF8(data, ptr, length);

                return ptr;
            }

            return allocate(intArrayFromString(data), 'i8', ALLOC_NORMAL);
        },

        sendToUnity: function(callback)
        {
            if (typeof dynCall === "function")
            {
                dynCall('v', callback);

                return;
            }

            bridgeUtils.logWarning("dynCall is not available");
        },

        sendDataToUnity: function(callback, stringPtr)
        {
            if (typeof dynCall === "function")
            {
                dynCall('vi', callback, [stringPtr]);
            }
            else
            {
                bridgeUtils.logWarning("dynCall is not available");
            }

            if (typeof _free === "function")
            {
                _free(stringPtr);

                return;
            }

            bridgeUtils.logWarning("_free is not available");
        },

        sendEventPayloadToUnity: function(objectName,
            methodName, message, adType, jsonPayload)
        {
            const parsedObjectName = UTF8ToString(objectName);
            const parsedMethodName = UTF8ToString(methodName);

            const parsedAdType = UTF8ToString(adType);
            const parsedJsonPayload = UTF8ToString(jsonPayload);

            let parsedMessage = UTF8ToString(message);

            if (parsedAdType)
            {
                parsedMessage = JSON.stringify(
                {
                    adType: parsedAdType,
                    payload: JSON.parse(parsedJsonPayload)

                });
            }

            if (typeof SendMessage === "function")
            {
                SendMessage(parsedObjectName, parsedMethodName, parsedMessage);

                bridgeUtils.logInfo(`Send event '${parsedMessage}' to ${parsedObjectName}.${methodName}`);

                return;
            }

            bridgeUtils.logWarning("SendMessage is not available in current context");
        }
    },

    AllocString: function(data)
    {
        return bridgeUtils.allocString(data);
    },

    LogInfo: function(message)
    {
        bridgeUtils.logInfo(message);
    },

    LogWarning: function(message)
    {
        bridgeUtils.logWarning(message);
    },

    LogError: function(message)
    {
        bridgeUtils.logError(message);
    },

    SendToUnity: function(callback)
    {
        bridgeUtils.sendToUnity(callback);
    },

    SendDataToUnity: function(callback, stringPtr)
    {
        bridgeUtils.sendDataToUnity(callback, stringPtr);
    },

    SendEventToUnity: function(objectName, methodName, 
        message, adType, jsonPayload)
    {
        bridgeUtils.sendEventPayloadToUnity(objectName, 
            methodName, message, adType, jsonPayload);
    }
};

autoAddDeps(bridgeUtils, `$bridgeUtils`);
mergeInto(LibraryManager.library, bridgeUtils);