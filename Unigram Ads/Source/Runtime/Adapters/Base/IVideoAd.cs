using System;
using UnigramAds.Core.Events;

namespace UnigramAds.Core.Adapters
{
    public interface IVideoAd: IDisposable, IVideoAdCallbacks
    {
        void Show();
        void Destroy();

        virtual void Destroy(string adUnit) { }
    }
}