using System;
using UnityEngine;

namespace UnigramAds.Utils
{
    internal sealed class UnigramAdsLogger
    {
        private static bool _isEnabled;

        internal bool IsEnabled => _isEnabled;

        internal const string PREFIX = "[Unigram Ads]";

        internal static void Enabled()
        {
            _isEnabled = true;
        }

        internal static void Disable()
        {
            _isEnabled = false;
        }

        internal static void Log(object message)
        {
            if (_isEnabled)
            {
                Debug.Log($"{PREFIX} {message}");
            }
        }

        internal static void LogWarning(object message)
        {
            if (_isEnabled)
            {
                Debug.LogWarning($"{PREFIX} {message}");
            }
        }

        internal static void LogError(object message)
        {
            if (_isEnabled)
            {
                Debug.LogError($"{PREFIX} {message}");
            }
        }

        internal static void LogException(Exception exception)
        {
            if (_isEnabled)
            {
                Debug.LogException(exception);
            }
        }
    }
}