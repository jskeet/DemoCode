namespace VDrumExplorer.Midi
{
    public sealed class IdentityResponse
    {
        public int DeviceId { get; }
        public int FamilyCode { get; }
        public int FamilyNumberCode { get; }
        public int SoftwareRevision { get; }

        public IdentityResponse(int deviceId, int familyCode, int familyNumberCode, int softwareRevision) =>
            (DeviceId, FamilyCode, FamilyNumberCode, SoftwareRevision) = (deviceId, familyCode, familyNumberCode, softwareRevision);

        public override string ToString() => new { DeviceId, FamilyCode, FamilyNumberCode, SoftwareRevision }.ToString();
    }
}
