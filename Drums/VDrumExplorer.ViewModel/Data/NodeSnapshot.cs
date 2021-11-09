// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Text.RegularExpressions;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Schema.Logical;

namespace VDrumExplorer.ViewModel.Data
{
    /// <summary>
    /// The snapshot of a logical node with a Data Explorer. This can be pasted within the same
    /// data explorer to a node with the same path after variable removal.
    /// </summary>
    public class NodeSnapshot
    {
        public string Path => SourceNode.Path;

        private readonly string comparisonPath;
        internal TreeNode SourceNode { get; }
        internal ModuleDataSnapshot Data { get; }

        internal NodeSnapshot(TreeNode sourceNode, ModuleDataSnapshot data)
        {
            SourceNode = sourceNode;
            Data = data;
            comparisonPath = RemovePathVariables(Path);
        }

        internal bool IsValidForTarget(TreeNode? targetNode)
        {
            if (targetNode is null)
            {
                return false;
            }
            return RemovePathVariables(targetNode.Path) == comparisonPath;
        }

        private static readonly Regex VariablePattern = new Regex(@"\[\d+\]");
        private string RemovePathVariables(string path) => VariablePattern.Replace(path, "");
    }
}
