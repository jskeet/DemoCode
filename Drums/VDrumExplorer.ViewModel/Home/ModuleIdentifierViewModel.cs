// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model;
using VDrumExplorer.Utility;

namespace VDrumExplorer.ViewModel.Home;

/// <summary>
/// View model for a <see cref="ModuleIdentifier"/>
/// </summary>
public class ModuleIdentifierViewModel : ViewModelBase<ModuleIdentifier>
{
    private readonly bool includeRevision;

    private ModuleIdentifierViewModel(ModuleIdentifier model, bool includeRevision) : base(model)
    {
        this.includeRevision = includeRevision;
    }

    public ModuleIdentifier Identifier => Model;
    public string DisplayName => includeRevision ? $"{Model.Name} (rev 0x{Model.SoftwareRevision:x})" : Model.Name;

    internal static IReadOnlyList<ModuleIdentifierViewModel> GetIdentifiersForKnownSchemas()
    {
        var identifiers = ModuleSchema.KnownSchemas.Keys;

        return identifiers
            .OrderBy(id => id.Name)
            .ToReadOnlyList(id => new ModuleIdentifierViewModel(id, identifiers.Count(x => x.Name == id.Name) > 1));
    }
}
