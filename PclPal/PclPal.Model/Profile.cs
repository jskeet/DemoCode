// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PclPal.Model
{
    public class Profile(string path, IEnumerable<SupportedRuntime> runtimes)
    {
        public string Name { get; } = System.IO.Path.GetFileName(path);
        public String Path { get; } = path;
        public IReadOnlyCollection<SupportedRuntime> SupportedRuntimes { get; } = runtimes.ToList().AsReadOnly();

        public static Profile Load(string path)
        {
            return new Profile(path, Directory.GetFiles(System.IO.Path.Combine(path, "SupportedFrameworks")).Select(SupportedRuntime.Load));
        }

        public static IEnumerable<Profile> LoadAll()
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (root == "")
            {
                root = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }
            var portableRoot = System.IO.Path.Combine(root, "Reference Assemblies/Microsoft/Framework/.NETPortable");
            if (!Directory.Exists(portableRoot))
            {
                throw new IOException(string.Format("Unable to find reference assembly directory root. Tried {0}", portableRoot));
            }

            return Directory.GetDirectories(portableRoot)
                            .SelectMany(frameworkPath => Directory.GetDirectories(System.IO.Path.Combine(frameworkPath, "Profile"), "Profile*"))
                            .Select(Load);
        }
    }
}
