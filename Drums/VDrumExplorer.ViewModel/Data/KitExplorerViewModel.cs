﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows.Input;
using VDrumExplorer.Model;
using VDrumExplorer.Proto;

namespace VDrumExplorer.ViewModel.Data
{
    public class KitExplorerViewModel : DataExplorerViewModel
    {

        public Kit Kit { get; }

        public int DefaultKitNumber
        {
            get => Kit.DefaultKitNumber;
            set
            {
                Kit.DefaultKitNumber = Kit.Schema.ValidateKitNumber(value);
                RaisePropertyChanged(nameof(DefaultKitNumber));
            }
        }

        private int kitCopyTargetNumber;
        public int KitCopyTargetNumber
        {
            get => kitCopyTargetNumber;
            set => SetProperty(ref kitCopyTargetNumber, Kit.Schema.ValidateKitNumber(value));
        }

        public KitExplorerViewModel(IViewServices viewServices, ILogger logger, DeviceViewModel deviceViewModel, Kit kit)
            : base(viewServices, logger, deviceViewModel, kit.Data)
        {
            Kit = kit;
            kitCopyTargetNumber = kit.DefaultKitNumber;
        }

        public override ICommand OpenCopyInKitExplorerCommand => CommandBase.NotImplemented;
        public override ICommand CopyKitCommand => CommandBase.NotImplemented;
        public override ICommand ImportKitFromFileCommand => CommandBase.NotImplemented;
        public override ICommand ExportKitCommand => CommandBase.NotImplemented;

        protected override string ExplorerName => "Kit Explorer";
        public override string SaveFileFilter => FileFilters.KitFiles;

        protected override void SaveToStream(Stream stream) => Kit.Save(stream);

        protected override void CopyDataToDevice() =>
            CopyDataToDevice(Model.LogicalRoot, Model.Schema.GetKitRoot(KitCopyTargetNumber).Container.Address);
    }
}
