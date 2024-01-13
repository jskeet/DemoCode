using JonSkeet.WpfUtil;
using NodaTime.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DigiMixer.Controls;
/// <summary>
/// Interaction logic for MixerControlPanel.xaml
/// </summary>
public partial class MixerControlPanel : UserControl
{
    private static readonly LocalDateTimePattern DefaultFilePattern = LocalDateTimePattern.CreateWithInvariantCulture("uuuu-MM-dd HH-mm");

    private DigiMixerViewModel ViewModel => (DigiMixerViewModel) DataContext;

    public MixerControlPanel()
    {
        InitializeComponent();
    }

    private void MuteAll(object sender, RoutedEventArgs e)
    {
        foreach (var channelVm in ViewModel.InputChannels)
        {
            channelVm.Muted = true;
        }
    }

    private void SaveSnapshot(object sender, RoutedEventArgs e)
    {/*
        var snapshot = DigiMixerSnapshot.FromMixerViewModel(ViewModel);
        if (snapshot is null)
        {
            // This isn't really great, but it's unlikely to be a problem.
            Dialogs.ShowErrorDialog("Waiting for data", "The mixer has not received all the necessary data yet. Please wait and try again.");
            return;
        }
        var now = NodaTimeUtilities.GetCurrentLocalDateTime();
        var defaultFile = $"Snapshot {DefaultFilePattern.Format(now)}";
        var file = Dialogs.ShowSaveFileDialog(MaybeCreateSnapshotsDirectory(), "JSON files|*.json", defaultFile);
        if (file is null)
        {
            return;
        }
        JsonUtilities.SaveJson(file, snapshot);*/
    }

    private void LoadSnapshot(object sender, RoutedEventArgs e)
    {/*
        var file = Dialogs.ShowOpenFileDialog(MaybeCreateSnapshotsDirectory(), "JSON files|*.json");
        if (file is null)
        {
            return;
        }
        var snapshot = JsonUtilities.LoadJson<DigiMixerSnapshot>(file);
        snapshot.CopyToViewModel(ViewModel);*/
    }

    private static string MaybeCreateSnapshotsDirectory()
    {
        return "";
        /*
        var directory = ConfigurationLocations.MixerSnapshotsDirectory;
        Directory.CreateDirectory(directory);
        return directory;*/
    }
}
