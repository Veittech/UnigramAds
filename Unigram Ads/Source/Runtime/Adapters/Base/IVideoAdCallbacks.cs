using System;

namespace UnigramAds.Core.Events
{
    public interface IVideoAdCallbacks
    {
        event Action OnLoaded;
        event Action OnClosed;
        event Action OnShown;
        event Action OnTryNonStopWatch;

        event Action OnShowExpired;
        event Action OnLoadFailed;
        event Action<string> OnShowFailed;
    }
}