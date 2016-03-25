// Copyright 2016 Jon Skeet.
// Licensed under the Apache License Version 2.0.

using Shed.Controllers.Reflection;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Shed.Controllers.Sonos
{
    [Description("Sonos")]
    public sealed class SonosController
    {
        private static readonly XNamespace SoapEnvelopeNs = "http://schemas.xmlsoap.org/soap/envelope/";
        
        private static readonly XElement SoapEnvelope = 
            new XElement(SoapEnvelopeNs + "Envelope",
                new XAttribute(SoapEnvelopeNs + "encodingStyle", "http://schemas.xmlsoap.org/soap/encoding/"),
                new XElement(SoapEnvelopeNs + "Body"));

        private static XElement Instance0 = new XElement("InstanceID", 0);

        private static readonly XNamespace AvTransportNs = "urn:schemas-upnp-org:service:AVTransport:1";
        private static readonly XNamespace RenderingControlNs = "urn:schemas-upnp-org:service:RenderingControl:1";

        const string AvTransportPath = "MediaRenderer/AVTransport/Control";
        const string RenderingControlPath  = "MediaRenderer/RenderingControl/Control";

        private readonly string host;
        private readonly int port;

        internal SonosController(string host, int port = 1400)
        {
            this.host = host;
            this.port = port;
        }

        [Description("Pauses any currently-playing music")]
        public void Pause()
        {
            SendCommand(AvTransportPath, new XElement(AvTransportNs + "Pause", Instance0));
        }

        [Description("Plays the current play-list (unpause)")]
        public void Play()
        {
            SendCommand(AvTransportPath, new XElement(AvTransportNs + "Play", Instance0, new XElement("Speed", 1)));
        }

        [Description("Sets the volume to the given percentage")]
        public void SetVolume(int percentage)
        {
            if (percentage < 0 || percentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(percentage), percentage, "Percentage should be in range [0, 100]");
            }

            SendCommand(RenderingControlPath,
                new XElement(RenderingControlNs + "SetVolume",
                    Instance0,
                    new XElement("Channel", "Master"),
                    new XElement("DesiredVolume", percentage)));
        }

        [Description("Skips to the next track")]
        public void Next()
        {
            SendCommand(AvTransportPath, new XElement(AvTransportNs + "Next", Instance0));
        }

        [Description("Moves to the previous track")]
        public void Previous()
        {
            SendCommand(AvTransportPath, new XElement(AvTransportNs + "Previous", Instance0));
        }

        [Description("Restarts the current track")]
        public void Restart()
        {
            SendCommand(AvTransportPath, new XElement(AvTransportNs + "Seek", Instance0, new XElement("Unit", "REL_TIME"), new XElement("Target", "0:00:00")));
        }

        // TODO: Infer the path from the command?
        private void SendCommand(string path, XElement command)
        {
            var uri = $"http://{host}:{port}/{path}";
            var body = new XElement(SoapEnvelope);
            body.Elements().First().Add(command);
            string header = $"\"{command.Name.NamespaceName}#{command.Name.LocalName}\"";

            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(body.ToString()));
            content.Headers.Add("SOAPACTION", header);
            using (var client = new HttpClient())
            {
                Task.Run(() => client.PostAsync(uri, content)).Wait();
            }
        }
    }
}
