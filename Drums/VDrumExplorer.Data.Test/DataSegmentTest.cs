// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;
using System;
using System.Linq;

namespace VDrumExplorer.Data.Test
{
    public class DataSegmentTest
    {
        [Test]
        public void GetData_Simple()
        {
            var start = new ModuleAddress(0x10);
            var segment = new DataSegment(start, new byte[] { 1, 2, 3 });
            Assert.AreEqual(1, segment[start]);
            Assert.AreEqual(2, segment[start + 1]);
            Assert.AreEqual(3, segment[start + 2]);
        }

        [Test]
        public void GetData_7BitBoundary()
        {
            var start = new ModuleAddress(0x70);
            var segment = new DataSegment(start, Enumerable.Range(0, 0x20).Select(x => (byte) x).ToArray());
            Assert.AreEqual(0, segment[new ModuleAddress(0x70)]);
            Assert.AreEqual(0xf, segment[new ModuleAddress(0x7f)]);
            // We skip 0x80-0xff.
            Assert.AreEqual(0x10, segment[new ModuleAddress(0x100)]);
            Assert.AreEqual(0x1f, segment[new ModuleAddress(0x10f)]);
        }

        [Test]
        public void Contains()
        {
            var start = new ModuleAddress(0x70);
            var segment = new DataSegment(start, Enumerable.Range(0, 0x20).Select(x => (byte) x).ToArray());
            Assert.IsFalse(segment.Contains(new ModuleAddress(0x6f)));
            Assert.IsTrue(segment.Contains(new ModuleAddress(0x70)));
            Assert.IsTrue(segment.Contains(new ModuleAddress(0x7f)));
            Assert.IsTrue(segment.Contains(new ModuleAddress(0x100)));
            Assert.IsTrue(segment.Contains(new ModuleAddress(0x10f)));
            Assert.IsFalse(segment.Contains(new ModuleAddress(0x110)));
        }

        [Test]
        public void OverlongSegment()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DataSegment(new ModuleAddress(0), new byte[256]));
        }

        [Test]
        public void Comparer()
        {
            var segment1 = new DataSegment(new ModuleAddress(0), new byte[100]);
            var segment2 = new DataSegment(new ModuleAddress(10), new byte[200]);
            var segment3 = new DataSegment(new ModuleAddress(20), new byte[100]);

            // Note: segment2 ends after segment3, but that's irrelevant in terms of the comparer
            var comparer = DataSegment.AddressComparer;
            Assert.That(comparer.Compare(segment1, segment1), Is.EqualTo(0));
            Assert.That(comparer.Compare(segment1, segment2), Is.LessThan(0));
            Assert.That(comparer.Compare(segment1, segment3), Is.LessThan(0));

            Assert.That(comparer.Compare(segment2, segment1), Is.GreaterThan(0));
            Assert.That(comparer.Compare(segment2, segment2), Is.EqualTo(0));
            Assert.That(comparer.Compare(segment2, segment3), Is.LessThan(0));

            Assert.That(comparer.Compare(segment3, segment1), Is.GreaterThan(0));
            Assert.That(comparer.Compare(segment3, segment2), Is.GreaterThan(0));
            Assert.That(comparer.Compare(segment3, segment3), Is.EqualTo(0));
        }

        [Test]
        public void End()
        {
            var start = new ModuleAddress(0x70);
            var segment = new DataSegment(start, new byte[0x20]);
            Assert.AreEqual(new ModuleAddress(0x110), segment.End);
        }
    }
}
