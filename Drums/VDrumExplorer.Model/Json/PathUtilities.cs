// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Model.Json
{
    /// <summary>
    /// Note: at some point we probably want a proper path struct or similar,
    /// but until then we can at least avoid *some* duplicate code...
    /// </summary>
    internal static class PathUtilities
    {
        internal static string AppendPath(string? parentPath, string name) =>
            parentPath switch
            {
                null => "/", // Root node
                "/" => "/" + name, // Direct child of root node
                _ => parentPath + "/" + name // Anything else
            };
    }
}
