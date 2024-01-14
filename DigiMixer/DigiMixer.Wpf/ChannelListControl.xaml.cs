using JonSkeet.WpfUtil;
using System.Windows.Controls;
using System.Windows.Input;

namespace DigiMixer.Wpf;
/// <summary>
/// Interaction logic for ChannelListControl.xaml
/// </summary>
public partial class ChannelListControl : UserControl
{
    private ChannelListViewModel ViewModel => (ChannelListViewModel) DataContext;

    public ChannelListControl()
    {
        InitializeComponent();
    }

    // TODO: Add keyboard shortcut (ctrl-ins?) for "add".
    private void HandleKeyDown(object sender, KeyEventArgs e) => ViewModel.MaybeHandleKeyboard(mappingList, e);
}
