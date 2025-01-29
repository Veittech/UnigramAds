namespace UnigramAds.Utils
{
    internal static class UnigramUtils
    {
        internal const string FAKE_BALANCE_SAVE_KEY = "[UNIGRAM ADS] FAKE-BALANCE";

        internal static bool IsSupporedPlatform()
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            UnityEngine.Debug.LogWarning($"{UnigramAdsLogger.PREFIX} Unsupported platform detected, " +
                $"please build the game in WebGl and start testing");

            return false;
#endif
            return true;
        }

        internal static bool IsSuccess(int statusCode)
        {
            if (statusCode == 1)
            {
                return true;
            }

            return false;
        }
    }
}