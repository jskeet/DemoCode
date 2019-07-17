namespace VDrumExplorer.Midi
{
    public sealed class IdentityResponse
    {
        public int DeviceId { get; }
        public int ModelId { get; }

        public IdentityResponse(int deviceId, int modelId) =>
            (DeviceId, ModelId) = (deviceId, modelId);
    }
}
