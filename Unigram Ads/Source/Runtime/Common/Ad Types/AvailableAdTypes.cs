namespace UnigramAds.Common
{
    internal sealed class AvailableAdTypes
    {
        public const string FULLSCREEN_AD = "FullscreenMedia";
        public const string REWARDED_VIDEO = "RewardedVideo";

        internal static AdTypes GetAdById(string adId)
        {
            AdTypes type = AdTypes.None;

            switch (adId)
            {
                case FULLSCREEN_AD:
                    type = AdTypes.FullscreenStatic;

                    break;
                case REWARDED_VIDEO:
                    type = AdTypes.RewardedVideo;

                    break;
            }

            return type;
        }

        internal static string GetAdByType(AdTypes type)
        {
            string adType = string.Empty;

            switch (type)
            {
                case AdTypes.FullscreenStatic:
                    adType = FULLSCREEN_AD;

                    break;
                case AdTypes.RewardedVideo:
                    adType = REWARDED_VIDEO;

                    break;
            }

            return adType;
        }
    }
}