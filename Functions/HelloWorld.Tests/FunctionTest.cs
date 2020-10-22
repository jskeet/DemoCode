// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Google.Cloud.Functions.Testing;
using NUnit.Framework;
using System.Threading.Tasks;

namespace HelloWorld.Tests
{
    public class FunctionTest : FunctionTestBase<Function>
    {
        [Test]
        public async Task RequestWritesMessage()
        {
            string text = await ExecuteHttpGetRequestAsync();
            Assert.AreEqual("Hello, Functions Framework.", text);
        }
    }
}
