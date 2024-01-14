using JonSkeet.WpfUtil;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace DigiMixer.Wpf;

internal class FileLocations
{
    internal static string AppDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigiMixer");

    internal static string ConfigFile => Path.Combine(AppDataDirectory, "config.json");

    internal static string SnapshotsDirectory => Path.Combine(AppDataDirectory, "Snapshots");
    internal static string LoggingDirectory => Path.Combine(AppDataDirectory, "Logs");

    internal static void MaybeCreateAppDataDirectory()
    {
        if (Directory.Exists(AppDataDirectory))
        {
            return;
        }
        var success = false;
        try
        {
            // Create the directory using cmd to create it "for real" as opposed to
            // in the shadow app data directory.
            var psi = new ProcessStartInfo("cmd.exe")
            {
                ArgumentList =
                        {
                            "/c",
                            "mkdir",
                            AppDataDirectory
                        }
            };
            var process = Process.Start(psi);
            process.WaitForExit();
            success = process.ExitCode == 0;
        }
        catch
        {
            // Handled below
        }
        if (!success)
        {
            Dialogs.ShowErrorDialog("Failed to create data directory",
                $"Failed to create directory \"{AppDataDirectory}\".\r\nPlease create the directory manually, or contact skeet@pobox.com for help.");
            Environment.Exit(1);
        }
    }

    internal static void MaybeCreateInitialConfig()
    {
        if (File.Exists(ConfigFile))
        {
            return;
        }
        var config = new DigiMixerAppConfig
        {
            Mixer = new()
            {
                HardwareType = Controls.DigiMixerConfig.MixerHardwareType.Fake,                
                InputChannels =
                {
                    new() { Id = "Channel 1", Channel = 1, InitiallyVisible = true }
                },
                OutputChannels =
                {
                    new() { Id = "Main", Channel = 100, InitiallyVisible = true }
                }
            },
            Logging = new()
            {
                LogLevel =
                {
                    ["Default"] = LogLevel.Debug
                }
            }
        };
        JsonUtilities.SaveJson(ConfigFile, config);
    }
}
