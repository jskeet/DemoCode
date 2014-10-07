// Copyright 2014 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;
using NUnit.Framework;

namespace Tests
{
    public class NotNullTest
    {
        // Tests to write:
        // - Every [NotNull] parameter has a check or is used in a call which checks (and is the same name)
        // - Every call to Preconditions.CheckNotNull uses a [NotNull] parameter name
        // - Every call to Preconditions.CheckNotNull uses a string literal as a parameter name
        // - No call to Preconditions.CheckNotNull uses the right parameter as the value

        [Test]
        public async Task CheckParameters()
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync("../../../ProductionCode/ProductionCode.csproj");
            var compilation = await project.GetCompilationAsync();
            // More code to go here...
        }
    }
}
