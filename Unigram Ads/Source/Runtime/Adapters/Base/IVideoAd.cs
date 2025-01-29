using System;

namespace UnigramAds.Core.Adapters
{
    public interface IVideoAd
    {
        void Show();
        void Destroy();

        virtual void Destroy(string adUnit) { }

        event Action OnShowFinished;
        event Action<string> OnShowFailed;
    }
}