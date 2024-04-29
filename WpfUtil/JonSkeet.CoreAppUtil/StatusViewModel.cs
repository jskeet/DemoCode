// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace JonSkeet.CoreAppUtil;

/// <summary>
/// ViewModel for <see cref="StatusItem"/>.
/// </summary>
public class StatusViewModel : ViewModelBase
{
    private readonly string prefix;

    public StatusViewModel(string prefix)
    {
        this.prefix = prefix;
    }

    private string text;
    public string Text
    {
        get => text;
        private set => SetProperty(ref text, value);
    }

    private string statusText;
    public string StatusText
    {
        get => statusText;
        private set => SetProperty(ref statusText, value);
    }

    private StatusType type;
    public StatusType Type
    {
        get => type;
        set => SetProperty(ref type, value);
    }

    public void ReportError(string statusText) => ReportStatus(StatusType.Error, statusText);

    public void ReportNormal(string statusText) => ReportStatus(StatusType.Normal, statusText);

    public void ReportWarning(string statusText) => ReportStatus(StatusType.Warning, statusText);

    public void Report(StatusType type, string statusText)
    {
        switch (type)
        {
            case StatusType.Normal:
                ReportNormal(statusText);
                break;
            case StatusType.Warning:
                ReportWarning(statusText);
                break;
            default: // Treat unknown status type as error...
                ReportError(statusText);
                break;

        }
    }

    private void ReportStatus(StatusType type, string statusText)
    {
        Type = type;
        StatusText = statusText;
        Text = $"{prefix}: {statusText}";
    }
}

public enum StatusType
{
    Normal = 0,
    Warning = 1,
    Error = 2
}
