using System;
using System.Windows;

namespace DigiMixer.Wpf;
/// <summary>
/// Interaction logic for MixerWindow.xaml
/// </summary>
public partial class MixerWindow : Window
{
    public MixerWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (DataContext is IDisposable disp)
        {
            disp.Dispose();
        }
    }
}
