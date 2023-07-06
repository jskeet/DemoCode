// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace DigiMixer.Ssc;

/// <summary>
/// SSC addresses, both generic and at specific Sennheiser products.
/// </summary>
public static class SscAddresses
{
    /// <summary>
    /// Addresses under the /osc node.
    /// </summary>
    public static class Osc
    {
        /// <summary>
        /// The address of the xid property, /osc/xid. This can be
        /// used to correlate requests with responses.
        /// </summary>
        public const string Xid = "/osc/xid";

        /// <summary>
        /// The address of the error property, /osc/error.
        /// </summary>
        /// <remarks>
        /// The value of this property is an array of objects (typically only one).
        /// Each object represents a tree of errors, in the same form as
        /// a normal message. Each address with an error has a value of an array,
        /// with an integer element (the error code) followed by an object element
        /// with a "desc" property for the description of the error.
        /// </remarks>
        public const string Error = "/osc/error";
    }

    /// <summary>
    /// Addresses under the /device node.
    /// </summary>
    public static class Device
    {
        /// <summary>
        /// Addresses under the /device/identity node.
        /// </summary>
        public static class Identity
        {
            public const string Product = "/device/identity/product";
            public const string Name = "/device/identity/name";
            public const string Serial = "/device/identity/serial";
            public const string Version = "/device/identity/version";
        }

        public const string Name = "/device/name";
        public const string Location = "/device/location";
    }

    /// <summary>
    /// Addresses for the EW-D1.
    /// </summary>
    public static class EwD1
    {
        public static class Rx1
        {
            public const string SignalQuality = "/rx1/rf_quality";
            public const string Warnings = "/rx1/warnings";
        }

        public static class Mates
        {
            public static class Tx1
            {
                public const string BatteryType = "/mates/tx1/bat_type";
                public const string BatteryLifetime = "/mates/tx1/bat_lifetime";
                public const string BatteryGauge = "/mates/tx1/bat_gauge";
                public const string BatteryState = "/mates/tx1/bat_state";
                public const string BatteryHealth = "/mates/tx1/bat_health";
                public const string BatteryCycles = "/mates/tx1/bat_cycles";
                public const string BatteryBars = "/mates/tx1/bat_bars";
                public const string Warnings = "/mates/tx1/warning";
                public const string Accoustic = "/mates/tx1/accoustic";
                public const string DeviceType = "/mates/tx1/device_type";
            }
        }
    }

    /// <summary>
    /// Addresses for the EW-DX EM 2.
    /// </summary>
    public static class EwDx
    {
        public static Rx Rx1 { get; } = GetRx(1);
        public static Rx Rx2 { get; } = GetRx(2);
        public static Rx GetRx(int rx) => new Rx($"/rx{rx}");

        public class Rx
        {
            private readonly string prefix;

            internal Rx(string prefix) =>
                this.prefix = prefix;

            public string Name => $"{prefix}/name";
            public string Warnings => $"{prefix}/warnings";
        }

        public static class M
        {
            public static Rx Rx1 { get; } = GetRx(1);
            public static Rx Rx2 { get; } = GetRx(2);
            public static Rx GetRx(int rx) => new Rx($"/m/rx{rx}");

            public class Rx
            {
                private readonly string prefix;

                internal Rx(string prefix) =>
                    this.prefix = prefix;

                public string SignalStrength => $"{prefix}/rssi";
                public string SignalQuality => $"{prefix}/rsqi";
                public string Antenna => $"{prefix}/divi";
            }
        }

        public static class Mates
        {
            public static Tx Tx1 { get; } = GetTx(1);
            public static Tx Tx2 { get; } = GetTx(2);
            public static Tx GetTx(int tx) => new Tx($"/mates/tx{tx}");

            public class Tx
            {
                private readonly string prefix;

                internal Tx(string prefix)
                {
                    this.prefix = prefix;
                    Battery = new TxBattery($"{prefix}/battery");
                }

                public string Name => $"{prefix}/name";
                public string Warnings => $"{prefix}/warnings";
                public string Type => $"{prefix}/type";
                public string Capsule => $"{prefix}/capsule";
                public string Mute => $"{prefix}/mute";

                public TxBattery Battery { get; }

                public class TxBattery
                {
                    private readonly string prefix;

                    internal TxBattery(string prefix) =>
                        this.prefix = prefix;

                    public string Type => $"{prefix}/type";
                    public string Lifetime => $"{prefix}/lifetime";
                    public string Gauge => $"{prefix}/gauge";
                }                
            }
        }
    }

    /// <summary>
    /// Addresses for the CHG-70N product.
    /// </summary>
    public static class Chg70N
    {
        public static class Bays
        {
            public const string DeviceType = "/bays/device_type";
            public const string BatteryTimeToFull = "/bays/bat_timetofull";
            public const string BatteryHealth = "/bays/bat_health";
            public const string BatteryGauge = "/bays/bat_gauge";
            public const string BatteryCycles = "/bays/bat_cycles";
        }
    }
}
