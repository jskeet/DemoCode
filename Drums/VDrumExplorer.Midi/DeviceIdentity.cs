// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Midi
{
    /// <summary>
    /// Device identity information as returned from the universal non-realtime identity request/reply protocol.
    /// </summary>
    public sealed class DeviceIdentity
    {
        /// <summary>
        /// The device ID, typically 16-31, or 127 for "all devices".
        /// </summary>
        internal byte RawDeviceId { get; }

        /// <summary>
        /// The device ID to display; 1 greater than the raw device ID.
        /// </summary>
        public int DisplayDeviceId => RawDeviceId + 1;
        public int FamilyCode { get; }
        public int FamilyNumberCode { get; }
        public int SoftwareRevision { get; }
        public ManufacturerId ManufacturerId { get; }

        internal DeviceIdentity(byte rawDeviceId, ManufacturerId manufacturerId, int familyCode, int familyNumberCode, int softwareRevision) =>
            (RawDeviceId, ManufacturerId, FamilyCode, FamilyNumberCode, SoftwareRevision) =
            (rawDeviceId, manufacturerId, familyCode, familyNumberCode, softwareRevision);

        public override string ToString() =>
            $"{DisplayDeviceId}: {ManufacturerId} product {FamilyCode}/{FamilyNumberCode}, revision {SoftwareRevision}";
    }
}
