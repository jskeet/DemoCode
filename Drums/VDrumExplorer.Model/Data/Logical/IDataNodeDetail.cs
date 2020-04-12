// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Model.Data.Logical
{
    /// <summary>
    /// Common interface for details to be displayed for a given tree node.
    /// </summary>
    public interface IDataNodeDetail
    {
        /// <summary>
        /// Description of the details.
        /// </summary>
        string Description { get; }
    }
}
