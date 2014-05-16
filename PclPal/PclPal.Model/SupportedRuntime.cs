// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PclPal.Model
{
    public class SupportedRuntime(private readonly XElement element)
    {
        public string this[string attribute]
        {
            get { return (string) element.Attribute(attribute); }
        }

        public IEnumerable<string> AttributeNames
        {
            get { return element.Attributes().Select(x => x.Name.LocalName); }
        }

        public string Id { get { return element.Attribute("Identifier").Value; } }

        internal static SupportedRuntime Load(string path)
        {
            return new SupportedRuntime(XElement.Load(path));
        }

        public override string ToString()
        {
            // TODO: Work out whether this is actually the best approach.
            return string.Format("{0} v{1}+", this["DisplayName"], this["MinimumVersion"]);
        }
    }
}
