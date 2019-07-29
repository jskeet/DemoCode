using Sanford.Multimedia.Midi;
using System.Collections.Generic;
using System.Linq;

namespace VDrumExplorer.Midi
{
    public sealed class DeviceInfo
    {
        /// <summary>
        /// Local device ID, used to create an InputDevice/OutputDevice.
        /// </summary>
        public int LocalDeviceId { get; }
        
        /// <summary>
        /// Human-readable device name
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Midi manufacturer ID; this appears to always return 1, so is not reliable.
        /// </summary>
        public ManufacturerId ManufacturerId { get; }
        
        /// <summary>
        /// Midi product ID.
        /// </summary>
        public short ProductId { get; }

        public DeviceInfo(int localDeviceId, string name, ManufacturerId manufacturerId, short productId) =>
            (LocalDeviceId, Name, ManufacturerId, ProductId) = (localDeviceId, name, manufacturerId, productId);

        public static IReadOnlyList<DeviceInfo> GetInputDevices() => Enumerable
            .Range(0, InputDevice.DeviceCount)
            .Select(InputDevice.GetDeviceCapabilities)
            .Select((caps, localId) => new DeviceInfo(localId, caps.name, (ManufacturerId) caps.mid, caps.pid))
            .ToList()
            .AsReadOnly();

        public static IReadOnlyList<DeviceInfo> GetOutputDevices() => Enumerable
            .Range(0, OutputDeviceBase.DeviceCount)
            .Select(OutputDeviceBase.GetDeviceCapabilities)
            .Select((caps, localId) => new DeviceInfo(localId, caps.name, (ManufacturerId)caps.mid, caps.pid))
            .ToList()
            .AsReadOnly();

        public override string ToString() => new { LocalDeviceId, Name, ManufacturerId, ProductId }.ToString();
    }
}
