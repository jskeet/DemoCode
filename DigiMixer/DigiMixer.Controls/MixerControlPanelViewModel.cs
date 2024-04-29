using JonSkeet.CoreAppUtil;
using JonSkeet.WpfUtil;
using NodaTime;
using NodaTime.Text;
using System.IO;
using System.Windows.Input;

namespace DigiMixer.Controls;

public class MixerControlPanelViewModel
{
    private static readonly InstantPattern DefaultSnapshotFilePattern = InstantPattern.CreateWithInvariantCulture("'Snapshot ' uuuu-MM-dd HH-mm'Z.json'");

    private readonly string snapshotsDirectory;

    public DigiMixerViewModel Mixer { get; }
    public ICommand LoadSnapshotCommand { get; }
    public ICommand SaveSnapshotCommand { get; }

    public MixerControlPanelViewModel(DigiMixerViewModel mixer, string snapshotsDirectory)
    {
        Mixer = mixer;
        this.snapshotsDirectory = snapshotsDirectory;
        LoadSnapshotCommand = ActionCommand.FromAction(LoadSnapshot);
        SaveSnapshotCommand = ActionCommand.FromAction(SaveSnapshot);
    }

    private void SaveSnapshot()
    {
        var snapshot = DigiMixerSnapshot.FromMixerViewModel(Mixer);
        if (snapshot is null)
        {
            // This isn't really great, but it's unlikely to be a problem.
            Dialogs.ShowErrorDialog("Waiting for data", "The mixer has not received all the necessary data yet. Please wait and try again.");
            return;
        }
        MaybeCreateSnapshotsDirectory();
        var now = SystemClock.Instance.GetCurrentInstant();
        var defaultFile = DefaultSnapshotFilePattern.Format(now);
        var file = Dialogs.ShowSaveFileDialog(snapshotsDirectory, "JSON files|*.json", defaultFile);
        if (file is null)
        {
            return;
        }
        JsonUtilities.SaveJson(file, snapshot);
    }

    private void LoadSnapshot()
    {
        MaybeCreateSnapshotsDirectory();
        var file = Dialogs.ShowOpenFileDialog(snapshotsDirectory, "JSON files|*.json");
        if (file is null)
        {
            return;
        }
        var snapshot = JsonUtilities.LoadJson<DigiMixerSnapshot>(file);
        snapshot.CopyToViewModel(Mixer);
    }

    private void MaybeCreateSnapshotsDirectory() => Directory.CreateDirectory(snapshotsDirectory);
}
