namespace UnigramAds.Common
{
    internal sealed class AdEvents
    {
        public const string AD_STARTED = "onStart";
        public const string AD_SKIPPED = "onSkip";
        public const string AD_CLOSED = "onClose";
        public const string AD_REWARD_CLAIMED = "onReward";
        public const string AD_COMPLETE = "onComplete";
        public const string AD_SHOWN = "onShow";
        public const string AD_WATCH_FAILED = "onError";
        public const string AD_REFRESHED = "onRefresh";
        public const string AD_NOT_AVAILABLE = "onBannerNotFound";
        public const string AD_NON_STOP_SHOWN = "onNonStopShow";
        public const string AD_TOO_LONG_SESSION_ERROR = "onTooLongSession";

        internal static string GetEventByType(AdEventsTypes type)
        {
            string targetId = string.Empty;

            switch (type)
            {
                case AdEventsTypes.Started:
                    targetId = AD_STARTED;

                    break;
                case AdEventsTypes.Skipped:
                    targetId = AD_SKIPPED;

                    break;
                case AdEventsTypes.Closed:
                    targetId = AD_CLOSED;

                    break;
                case AdEventsTypes.Shown:
                    targetId = AD_SHOWN;

                    break;
                case AdEventsTypes.Refreshed:
                    targetId = AD_REFRESHED;

                    break;
                case AdEventsTypes.RewardClaimed:
                    targetId = AD_REWARD_CLAIMED;

                    break;
                case AdEventsTypes.Completed:
                    targetId = AD_COMPLETE;

                    break;
                case AdEventsTypes.WatchFailed:
                    targetId = AD_WATCH_FAILED;

                    break;
                case AdEventsTypes.NotAvailable:
                    targetId = AD_NOT_AVAILABLE;

                    break;
                case AdEventsTypes.TryNonStopWatch:
                    targetId = AD_NON_STOP_SHOWN;

                    break;
                case AdEventsTypes.TooLongSession:
                    targetId = AD_TOO_LONG_SESSION_ERROR;

                    break;
            }

            return targetId;
        }

        internal static AdEventsTypes GetEventById(string eventId)
        {
            AdEventsTypes eventType = AdEventsTypes.None;

            switch (eventId)
            {
                case AD_STARTED:
                    eventType = AdEventsTypes.Started;

                    break;
                case AD_SKIPPED:
                    eventType = AdEventsTypes.Skipped;

                    break;
                case AD_COMPLETE:
                    eventType = AdEventsTypes.Completed;

                    break;
                case AD_CLOSED:
                    eventType = AdEventsTypes.Closed;

                    break;
                case AD_SHOWN:
                    eventType = AdEventsTypes.Shown;

                    break;
                case AD_REFRESHED:
                    eventType = AdEventsTypes.Refreshed;

                    break;
                case AD_REWARD_CLAIMED:
                    eventType = AdEventsTypes.RewardClaimed;

                    break;
                case AD_NOT_AVAILABLE:
                    eventType = AdEventsTypes.NotAvailable;

                    break;
                case AD_NON_STOP_SHOWN:
                    eventType = AdEventsTypes.NotAvailable;

                    break;
                case AD_TOO_LONG_SESSION_ERROR:
                    eventType = AdEventsTypes.TooLongSession;

                    break;
            }

            return eventType;
        }
    }
}