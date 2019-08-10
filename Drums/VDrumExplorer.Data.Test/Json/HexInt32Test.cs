// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;
using System;
using VDrumExplorer.Data.Json;

namespace VDrumExplorer.Data.Test.Json
{
    public class HexInt32Test
    {
        [Test]
        [TestCase("0x0", 0)]
        [TestCase("0x5", 0x5)]
        [TestCase("0x10", 0x10)]
        [TestCase("0x10__0f", 0x100f)]
        [TestCase("0x7f_ff_ff_ff", int.MaxValue)]
        public void Parse_Valid(string text, int expectedValue)
        {
            var hex = HexInt32.Parse(text);
            Assert.AreEqual(expectedValue, hex.Value);
            Assert.AreEqual(hex.Text, text);
            Assert.AreEqual(hex.ToString(), text);

            var negative = HexInt32.Parse($"-{text}");
            Assert.AreEqual(-expectedValue, negative.Value);
        }

        [Test]
        [TestCase("0xrubbish")]
        [TestCase("+0x10")]
        [TestCase("15")]
        [TestCase("--0x15")]
        [TestCase("0x80_00_00_00")]
        // This "should" be int.MinValue, but we don't support that
        [TestCase("-0x80_00_00_00")]
        public void Parse_Invalid(string text) =>
            Assert.Throws<FormatException>(() => HexInt32.Parse(text));

        [Test]
        public void Parse_Null() =>
            Assert.Throws<ArgumentNullException>(() => HexInt32.Parse(null));

        [Test]
        public void Equality()
        {
            // Different text doesn't affect equality
            var hex1 = HexInt32.Parse("0x15_00");
            var hex2 = HexInt32.Parse("0x1500");
            var hex3 = HexInt32.Parse("0x1501");

            Assert.True(hex1.Equals(hex2));
            Assert.False(hex1.Equals(hex3));
            Assert.False(hex1.Equals(null));

            Assert.True(hex1.Equals((object) hex2));
            Assert.False(hex1.Equals((object) hex3));
            Assert.False(hex1.Equals((object) null));

            Assert.AreEqual(hex1.GetHashCode(), hex2.GetHashCode());
            Assert.AreNotEqual(hex1.GetHashCode(), hex3.GetHashCode());
        }
    }
}
