// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

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
