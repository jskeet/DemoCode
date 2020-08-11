// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VDrumExplorer.Model.Device;

namespace VDrumExplorer.ViewModel.Dialogs
{
    /// <summary>
    /// Non-generic view-model for the sake of the designer
    /// </summary>
    public class DataTransferViewModel : ViewModelBase
    {
        protected CancellationTokenSource CancellationTokenSource { get; }
        public ICommand CancelCommand { get; }
        public string Title { get; }

        private int completed;
        public int Completed
        {
            get => completed;
            protected set => SetProperty(ref completed, value);
        }

        private int total;
        public int Total
        {
            get => total;
            protected set => SetProperty(ref total, value);
        }

        private string currentItem = "";
        public string CurrentItem
        {
            get => currentItem;
            protected set => SetProperty(ref currentItem, value);
        }

        private string progressDescription = "Progress";
        public string ProgressDescription
        {
            get => progressDescription;
            protected set => SetProperty(ref progressDescription, value);
        }

        private bool? dialogResult;
        public bool? DialogResult
        {
            get => dialogResult;
            set => SetProperty(ref dialogResult, value);
        }

        public DataTransferViewModel(string title)
        {
            Title = title;
            CancelCommand = new DelegateCommand(Cancel, true);
            CancellationTokenSource = new CancellationTokenSource();
        }

        private void Cancel() => CancellationTokenSource.Cancel();

    }

    public sealed class DataTransferViewModel<T> : DataTransferViewModel
    {
        private readonly string progressFormat;
        private readonly ILogger logger;
        private readonly Func<IProgress<TransferProgress>, CancellationToken, Task<T>> transferFunction;

        public DataTransferViewModel(ILogger logger, string title, string progressFormat, Func<IProgress<TransferProgress>, CancellationToken, Task<T>> transferFunction)
            : base(title)
        {
            this.logger = logger;
            this.progressFormat = progressFormat;
            this.transferFunction = transferFunction;
        }

        public async Task<T> TransferAsync()
        {
            bool success = false;
            try
            {
                var result = await transferFunction(new Progress<TransferProgress>(UpdateProgress), CancellationTokenSource.Token);
                success = true;
                return result;
            }
            catch (OperationCanceledException) when (CancellationTokenSource.IsCancellationRequested)
            {
                logger.LogError($"User cancelled transfer operation");
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failure while transferring {CurrentItem}");
                throw;
            }
            finally
            {
                DialogResult = success;
            }
        }

        private void UpdateProgress(TransferProgress progress) =>
            (Total, Completed, ProgressDescription, CurrentItem) = (progress.Total, progress.Completed, string.Format(progressFormat, progress.Current), progress.Current);
    }
}
