// Copyright 2016 Jon Skeet.
// Licensed under the Apache License Version 2.0.

using Shed.Controllers.Lighting;
using Shed.Controllers.Onkyo;
using Shed.Controllers.Reflection;
using Shed.Controllers.Sonos;
using System.Collections.Immutable;
using System.Linq;

namespace Shed.Controllers
{
    /// <summary>
    /// "Factory"-ish for access to controllers, both strongly typed and via reflection (IController).
    /// TODO: Rename from "Factory" which is a horrible name.
    /// </summary>
    public static class Factory
    {
        public static LightingController Lighting { get; } = new LightingController("192.168.1.255");
        public static SonosController Sonos { get; } = new SonosController("192.168.1.20");
        public static OnkyoController Amplifier { get; } = new OnkyoController("192.168.1.11");
        public static IImmutableList<IController> AllControllers { get; } =
            new object[] { Sonos, Lighting, Amplifier }
                .Select(x => new ReflectiveController(x))
                .ToImmutableList<IController>();
    }
}
