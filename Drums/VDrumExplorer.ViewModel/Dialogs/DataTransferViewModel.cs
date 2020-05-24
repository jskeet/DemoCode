// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VDrumExplorer.Midi;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Device;
using VDrumExplorer.Model.Schema.Physical;

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

        private string currentItem = "Progress";
        public string CurrentItem
        {
            get => currentItem;
            protected set => SetProperty(ref currentItem, value);
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
        private readonly ILogger logger;
        private readonly Func<IProgress<TransferProgress>, CancellationToken, Task<T>> transferFunction;

        public DataTransferViewModel(ILogger logger, string title, Func<IProgress<TransferProgress>, CancellationToken, Task<T>> transferFunction)
            : base(title)
        {
            this.transferFunction = transferFunction;
            this.logger = logger;
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
                logger.LogError($"Failure while loading {CurrentItem}", e);
                throw;
            }
            finally
            {
                DialogResult = success;
            }
        }

        private void UpdateProgress(TransferProgress progress) =>
            (Total, Completed, CurrentItem) = (progress.Total, progress.Completed, progress.Current);
    }
}
