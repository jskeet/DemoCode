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
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.ViewModel.Dialogs
{
    public sealed class DataTransferViewModel : ViewModelBase
    {
        private readonly ILogger logger;
        public ICommand CancelCommand { get; }
        public string Title { get; }

        private int completed;
        public int Completed
        {
            get => completed;
            private set => SetProperty(ref completed, value);
        }

        private int total;
        public int Total
        {
            get => total;
            private set => SetProperty(ref total, value);
        }

        private string currentItem = "Progress";
        public string CurrentItem
        {
            get => currentItem;
            private set => SetProperty(ref currentItem, value);
        }

        public DataTransferViewModel(ILogger logger, string title)
        {
            this.logger = logger;
            Title = title;
            CancelCommand = new DelegateCommand(Cancel, true);
        }

        private void Cancel()
        {
        }

        internal async Task StoreDataAsync(RolandMidiClient client, IReadOnlyList<DataSegment> segments, CancellationToken cancellationToken)
        {
            Completed = 0;
            Total = segments.Count;

            foreach (var segment in segments)
            {
                CurrentItem = $"Storing segment at {segment.Address.DisplayValue:x8}";
                cancellationToken.ThrowIfCancellationRequested();
                client.SendData(segment.Address.DisplayValue, segment.CopyData());
                await Task.Delay(40);
                Completed++;
            }
            CurrentItem = "Finished";
        }

        internal async Task<List<DataSegment>> LoadDataAsync(RolandMidiClient client, IReadOnlyList<FieldContainer> containers, CancellationToken cancellationToken)
        {
            Completed = 0;
            Total = containers.Count;

            List<DataSegment> segments = new List<DataSegment>(Total);
            foreach (var container in containers)
            {
                CurrentItem = $"Loading {container.Path}";
                segments.Add(await LoadSegment(client, container, cancellationToken));
                Completed++;
            }
            CurrentItem = "Finished";
            return segments;
        }

        private async Task<DataSegment> LoadSegment(RolandMidiClient client, FieldContainer container, CancellationToken token)
        {
            var timerToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
            var effectiveToken = CancellationTokenSource.CreateLinkedTokenSource(token, timerToken).Token;
            try
            {
                var address = container.Address;
                var data = await client.RequestDataAsync(address.DisplayValue, container.Size, effectiveToken);
                return new DataSegment(address, data);
            }
            catch (OperationCanceledException) when (timerToken.IsCancellationRequested)
            {
                logger.LogError($"Device didn't respond for container {container.Path}; aborting.");
                throw;
            }
            catch (Exception e)
            {
                logger.LogError($"Failure while loading {container.Path}", e);
                throw;
            }
        }
    }
}
