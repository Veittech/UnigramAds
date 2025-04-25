using UnigramAds.Common;

namespace UnigramAds.Data
{
    [System.Serializable]
    public sealed class NativeEventPayloadData
    {
        public NativeAdTypes AdType { get; set; }
        public string EventId { get; set; }
    }
}