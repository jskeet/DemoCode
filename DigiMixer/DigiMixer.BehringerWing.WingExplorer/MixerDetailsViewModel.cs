using JonSkeet.CoreAppUtil;
using System.Windows.Input;

namespace DigiMixer.BehringerWing.WingExplorer;

public class MixerDetailsViewModel : ViewModelBase
{
    private string ipAddress = "192.168.1.74";
    public string IPAddress
    {
        get => ipAddress;
        set => SetProperty(ref ipAddress, value);
    }

    private ushort port = 2222;
    public ushort Port
    {
        get => port;
        set => SetProperty(ref port, value);
    }

    public required ICommand ConnectCommand { get; init; }
}
