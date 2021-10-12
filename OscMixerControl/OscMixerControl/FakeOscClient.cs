// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OscMixerControl
{
    internal sealed class FakeOscClient : IOscClient
    {
        public event EventHandler<OscPacket> PacketReceived;

        public void Dispose()
        {            
        }

        public Task SendAsync(OscPacket packet)
        {
            if (packet is not OscMessage message)
            {
                return Task.CompletedTask;
            }
            var response = GetResponse(message);
            if (response is object)
            {
                PacketReceived?.Invoke(this, response);
            }
            return Task.CompletedTask;
        }

        private static readonly List<Regex> FaderLevelNamePatterns = new[]
        {
            @"/ch/\d\d/mix/fader",
            @"/ch/\d\d/mix/\d\d/level",
            @"/rtn/aux/mix/fader",
            @"/rtn/aux/mix/\d\d/level",
            @"/bus/\d/mix/fader",
            "/lr/mix/fader"
        }.Select(p => new Regex(p)).ToList();

        private static readonly List<Regex> ChannelOnNamePatterns = new[]
        {
            @"/ch/\d\d/mix/on",
            @"/bus/\d/mix/on",
            "/rtn/aux/mix/on",
            "/lr/mix/on"
        }.Select(p => new Regex(p)).ToList();

        private OscPacket GetResponse(OscMessage request)
        {
            var address = request.Address;
            if (address == "/info")
            {
                return new OscMessage("/info", "V1.0", "Fake", "FakeOscClient", "V1.0");
            }
            if (address.EndsWith("/config/name"))
            {
                return request.Count == 0 ? new OscMessage(address, "Fake") : request;
            }
            if (FaderLevelNamePatterns.Any(p => p.IsMatch(address)))
            {
                return request.Count == 0 ? new OscMessage(address, 0f) : request;
            }
            if (ChannelOnNamePatterns.Any(p => p.IsMatch(address)))
            {
                return request.Count == 0 ? new OscMessage(address, 0) : request;
            }
            return null;
        }
    }
}
