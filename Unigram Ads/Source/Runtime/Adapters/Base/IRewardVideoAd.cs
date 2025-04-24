using System;
using UnigramAds.Core.Events;

namespace UnigramAds.Core.Adapters
{
    public interface IRewardVideoAd : IVideoAd, IRewardVideoAdCallbacks
    {
        void Show(Action adFinished);
    }
}