using System;

namespace UnigramAds.Core.Events
{
    public interface IRewardVideoAdCallbacks
    {
        event Action OnRewarded;
    }
}