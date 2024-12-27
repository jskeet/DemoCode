using System.Collections.Generic;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Utility;
using VDrumExplorer.ViewModel.Data;

namespace VDrumExplorer.ViewModel.Dialogs;

public class MultiPasteViewModel
{
    public NodeSnapshot Snapshot { get; }
    public IReadOnlyList<CheckableCandidate> Candidates { get; }

    public MultiPasteViewModel(NodeSnapshot snapshot, List<TreeNode> candidates)
    {
        Snapshot = snapshot;
        Candidates = candidates.ToReadOnlyList(c => new CheckableCandidate(c));
    }

    public class CheckableCandidate : ViewModelBase
    {
        private bool isChecked;
        public bool Checked
        {
            get => isChecked;
            set => SetProperty(ref isChecked, value);
        }

        public TreeNode Candidate { get; }

        internal CheckableCandidate(TreeNode candidate)
        {
            Candidate = candidate;
        }
    }
}
