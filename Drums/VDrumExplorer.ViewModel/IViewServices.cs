// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDrumExplorer.ViewModel.Audio;
using VDrumExplorer.ViewModel.Data;
using VDrumExplorer.ViewModel.Dialogs;
using VDrumExplorer.ViewModel.LogicalSchema;

namespace VDrumExplorer.ViewModel
{
    /// <summary>
    /// Interface implemented by the view to handle behavior that is inherently UI-related,
    /// primarily around opening dialog boxes.
    /// </summary>
    public interface IViewServices
    {
        /// <summary>
        /// Shows an "open file" dialog with the given filter.
        /// </summary>
        /// <param name="filter">The filter for which files to show. See FileDialog.Filter docs for details.</param>
        /// <returns>The selected file, or null if the dialog was cancelled.</returns>
        string? ShowOpenFileDialog(string filter);

        /// <summary>
        /// Shows a "save file" dialog with the given filter.
        /// </summary>
        /// <param name="filter">The filter for which files to show. See FileDialog.Filter docs for details.</param>
        /// <returns>The selected file, or null if the dialog was cancelled.</returns>
        string? ShowSaveFileDialog(string filter);

        int? ChooseCopyKitTarget(CopyKitViewModel viewModel);
        bool ChooseMultiPasteTargets(MultiPasteViewModel viewModel);
        void ShowSchemaExplorer(ModuleSchemaViewModel viewModel);
        void ShowKitExplorer(KitExplorerViewModel viewModel);
        void ShowModuleExplorer(ModuleExplorerViewModel viewModel);
        void ShowInstrumentAudioExplorer(InstrumentAudioExplorerViewModel viewModel);
        void ShowInstrumentRecorderDialog(InstrumentAudioRecorderViewModel viewModel);
        Task<T?> ShowDataTransferDialog<T>(DataTransferViewModel<T> viewModel)
            where T : class;

        void AddRequerySuggestion(EventHandler handler);
        void RemoveRequerySuggestion(EventHandler handler);
    }
}
