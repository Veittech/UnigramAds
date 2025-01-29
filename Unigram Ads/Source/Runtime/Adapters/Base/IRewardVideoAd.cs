using System;

namespace UnigramAds.Core.Adapters
{
    public interface IRewardVideoAd : IVideoAd
    {
        void Show(Action adFinished);
    }
}