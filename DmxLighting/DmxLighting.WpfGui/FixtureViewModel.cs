using DmxLighting.Data;

namespace DmxLighting.WpfGui
{
    internal class FixtureViewModel
    {
        public FixtureData Data { get; set; }
        public StreamingAcnSender Sender { get; set; }
        public DmxUniverse Universe { get; set; }
    }
}
