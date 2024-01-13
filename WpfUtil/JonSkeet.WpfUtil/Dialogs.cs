// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace JonSkeet.WpfUtil;

public static class Dialogs
{
    /// <summary>
    /// Returns the user's download folder, or null if it can't be determined.
    /// </summary>
    /// <returns></returns>
    public static string GetDownloadsFolder()
    {
        Guid guid = new Guid("{374DE290-123F-4565-9164-39C4925E467B}");
        int result = SHGetKnownFolderPath(guid, dwFlags: 0, hToken: IntPtr.Zero, out IntPtr outPath);
        if (result >= 0)
        {
            string path = Marshal.PtrToStringUni(outPath);
            Marshal.FreeCoTaskMem(outPath);
            return path;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Opens a save file dialog and prompts the user with the given parameters.
    /// The resulting filename is returned, or null if the user cancelled the dialog.
    /// </summary>
    /// <param name="initialDirectory">The initial dialog to prompt with.</param>
    /// <param name="filter">The file filter, in the form "description|pattern|description|pattern"
    /// where patterns are semi-colon separated., e.g
    /// "PowerPoint files|*.ppt;*.pptx|Image files|*.jpg;*.png</param>
    /// <param name="file">The initial file name to suggest, if any.</param>
    /// <returns>The chosen filename, or null if the user cancelled.</returns>
    public static string ShowSaveFileDialog(string initialDirectory, string filter, string file = "")
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            InitialDirectory = initialDirectory,
            FileName = file
        };
        var result = dialog.ShowDialog();
        return result == true ? dialog.FileName : null;
    }

    /// <summary>
    /// Shows the given error message in a dialog, with an OK button.
    /// </summary>
    /// <param name="title">The title to display.</param>
    /// <param name="message">The message within the dialog.</param>
    public static void ShowErrorDialog(string title, string message) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    /// <summary>
    /// Shows the given information message in a dialog, with an OK button.
    /// </summary>
    /// <param name="title">The title to display.</param>
    /// <param name="message">The message within the dialog.</param>
    public static void ShowInfoDialog(string title, string message) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    /// <summary>
    /// Shows the given warning message in a dialog, with an OK button.
    /// </summary>
    /// <param name="title">The title to display.</param>
    /// <param name="message">The message within the dialog.</param>
    public static void ShowWarningDialog(string title, string message) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public static bool ShowYesNoDialog(string title, string message, MessageBoxImage icon = MessageBoxImage.Question) =>
        MessageBox.Show(message, title, MessageBoxButton.YesNo, icon) == MessageBoxResult.Yes;

    public static bool? ShowYesNoCancelDialog(string title, string message, MessageBoxImage icon = MessageBoxImage.Question) =>
        MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, icon) switch
        {
            MessageBoxResult.Yes => true,
            MessageBoxResult.No => false,
            _ => null
        };

    /// <summary>
    /// Opens an open file dialog and prompts the user with the given parameters.
    /// The resulting filename is returned, or null if the user cancelled the dialog.
    /// </summary>
    /// <param name="initialDirectory">The initial dialog to prompt with.</param>
    /// <param name="filter">The file filter, in the form "description|pattern|description|pattern"
    /// where patterns are semi-colon separated., e.g
    /// "PowerPoint files|*.ppt;*.pptx|Image files|*.jpg;*.png"</param>
    /// <param name="file">The initial file name to suggest, if any.</param>
    /// <returns>The chosen filename, or null if the user cancelled.</returns>
    public static string ShowOpenFileDialog(string initialDirectory, string filter, string file = "")
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            InitialDirectory = initialDirectory,
            FileName = file
        };
        var result = dialog.ShowDialog();
        return result == true ? dialog.FileName : null;
    }

    [DllImport("Shell32.dll")]
    private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);

    /// <summary>
    /// Common code for dialogs where a model may or may not have been changed.
    /// If the dialog result is null (typically due to pressing escape or just closing the dialog)
    /// then the original and new models are compared. If they're equal, DialogResult is set to false
    /// without prompting. Otherwise, the user is prompted to save changes, discard them, or cancel the operation.
    /// </summary>
    public static void HandleDialogClosing(Window window, CancelEventArgs e, string originalModelJson, string newModelJson, string modelType)
    {
        // If we already know what to do, there's no more work needed.
        if (window.DialogResult is bool)
        {
            return;
        }
        // The new model is the original model; no changes required, so we can cancel.
        if (originalModelJson == newModelJson)
        {
            window.DialogResult = false;
            return;
        }

        // The user has made changes, but not indicated what to do with them. Ask them.
        var saveDiscardCancelResult = ShowYesNoCancelDialog("Save changes?", $"Save changes made to this {modelType}?");
        switch (saveDiscardCancelResult)
        {
            case null:
                e.Cancel = true;
                break;
            case false:
                window.DialogResult = false;
                break;
            case true:
                window.DialogResult = true;
                break;
        }
    }
}
