using System.Windows;
using VDrumExplorer.ViewModel.Dialogs;

namespace VDrumExplorer.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for MultiPasteDialog.xaml
    /// </summary>
    public partial class MultiPasteDialog : Window
    {
        public MultiPasteDialog()
        {
            InitializeComponent();
        }

        private void Cancel(object sender, RoutedEventArgs e) =>
            DialogResult = false;

        private void Paste(object sender, RoutedEventArgs e) =>
            DialogResult = true;

        private void SelectAll(object sender, RoutedEventArgs e) =>
            SetCheckedForAllCandidates(true);

        private void SelectNone(object sender, RoutedEventArgs e) =>
            SetCheckedForAllCandidates(false);

        private void SetCheckedForAllCandidates(bool value)
        {
            var vm = (MultiPasteViewModel) DataContext;
            foreach (var item in vm.Candidates)
            {
                item.Checked = value;
            }
        }
    }
}
