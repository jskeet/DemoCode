﻿// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Net.Sockets;
using System.Text;

namespace DmxLighting
{
    /// <summary>
    /// Propagates changes to a <see cref="DmxUniverse"/> via
    /// streaming ACN.
    /// </summary>
    public sealed class StreamingAcnSender : IDisposable
    {
        // Generated by guidgenerator.com
        // Converted straight to a byte array to avoid having to do so multiple times.
        private static readonly byte[] guidBytes = Guid.Parse("d3514e10-7cb9-4106-97e0-9abad81cb1b5").ToByteArray();

        private readonly UdpClient client;
        private Action disposalActions;

        // FIXME: This really needs to be per universe...
        private byte sequenceNumber;

        public string SourceName => "TODO: Make this configurable";

        /// <summary>
        /// Creates a sender for a given host and the default port (5568).
        /// </summary>
        /// <param name="host">The host to send packets to.</param>
        public StreamingAcnSender(string host) : this(host, 5568)
        {
        }

        /// <summary>
        /// Creates a sender for a given host and the specified port.
        /// </summary>
        /// <param name="host">The host to send packets to.</param>
        /// <param name="port">The UDP port to send packets to.</param>
        public StreamingAcnSender(string host, int port)
        {
            //client = new UdpClient(host, port);
            client = new UdpClient();
            client.Connect(host, port);

            // TODO: Emit the initial values?
        }

        public void WatchUniverse(DmxUniverse universe)
        {
            universe.ChannelsChanged += HandleChannelsChanged;
            disposalActions += () => universe.ChannelsChanged -= HandleChannelsChanged;
        }

        public void Dispose()
        {
            disposalActions?.Invoke();
        }

        public void SendUniverse(DmxUniverse universe) => SendPacket(universe, universe.CloneChannels());

        private void HandleChannelsChanged(object sender, byte[] channels)
        {
            DmxUniverse universe = (DmxUniverse) sender;
            SendPacket(universe, channels);
        }

        private void SendPacket(DmxUniverse universe, byte[] channels)
        {
            byte[] packet = new byte[125 + channels.Length];

            // TODO: Don't encode the same string multiple times...

            // Root layer
            SetInt16(0, 0x10);         // Preamble size
            SetInt16(2, 0);            // Postamble size
            SetString(4, "ASC-E1.17"); // ACN Packet Identifier
            SetInt16(16, (short) (0x7000 | (packet.Length - 16))); // PDU length
            SetInt32(18, 0x00000004);  // VECTOR_ROOT_E131_DATA
            Buffer.BlockCopy(guidBytes, 0, packet, 22, 16); // Sender CID

            // Framing layer
            SetInt16(38, (short) (0x7000 | (packet.Length - 38))); // PDU length
            SetInt32(40, 0x00000002);  // VECTOR_E131_DATA_PACKET
            SetString(44, SourceName); // Source name (max length 63 bytes due to null termination)
            packet[108] = 100;         // Priority (100 = default)
            SetInt16(109, 0);          // Synchronization address (universe) - 0 is unsynchronized
            packet[111] = sequenceNumber++; // Sequence number: FIXME!
            packet[112] = 0;           // Options
            SetInt16(113, universe.UniverseNumber);

            // DMP layer
            SetInt16(115, (short) (0x7000 | (packet.Length - 115))); // PDU length
            packet[117] = 2;           // VECTOR_DMP_SET_PROPERTY
            packet[118] = 0xa1;        // Address type and data type
            SetInt16(119, 0);          // First property and address
            SetInt16(121, 1);          // Address increment
            SetInt16(123, (short) channels.Length);
            Buffer.BlockCopy(channels, 0, packet, 125, channels.Length);

            // Off we go!
            client.Send(packet, packet.Length);

            void SetInt16(int index, short value)
            {
                packet[index] = (byte) (value >> 8);
                packet[index + 1] = (byte) (value & 0xff);
            }

            void SetInt32(int index, int value)
            {
                packet[index] = (byte) (value >> 24);
                packet[index + 1] = (byte) (value >> 16);
                packet[index + 2] = (byte) (value >> 8);
                packet[index + 3] = (byte) (value & 0xff);
            }

            void SetString(int index, string value) =>
                Encoding.UTF8.GetBytes(value, 0, value.Length, packet, index);
        }
    }
}
