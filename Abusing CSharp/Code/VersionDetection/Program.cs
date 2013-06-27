// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
#pragma warning disable
using System;
using System.Collections;
using System.Collections.Generic;

namespace VersionDetection
{
    class Program
    {
        private static void Main(string[] args)
        {
            bool disposingForEach = DetectDisposingForEach();
            bool methodGroupVariance = DetectMethodGroupVariance();
            bool smartTypeInference = DetectSmartTypeInference();
            bool defaulting = new VersionHelper.T2().DetectDefaulting();
            bool saneForEachCapture = DetectSaneForEachCapture();

            Console.WriteLine("Results...");
            Console.WriteLine("Disposing foreach loop? (v1.2 vs v1.0) - {0}", disposingForEach);
            Console.WriteLine("Method group variance? (v2.0 vs 1.2) - {0}", methodGroupVariance);
            Console.WriteLine("Smart type inference? (v3.0 vs v2.0) - {0}", smartTypeInference);
            Console.WriteLine("Handling of default parameters? (v4.0 vs 3.0) - {0}", defaulting);
            Console.WriteLine("Sane capture of foreach iteration variable? (v5.0 vs v4.0) - {0}", saneForEachCapture);
        }

        #region C# 1.2 / C# 1.0
        // Note: this *isn't* a specification change, which I had originally thought.
        // I *believe* it's a compiler change (that the 1.0 compiler had a bug) but that's hard
        // to verify now. I'm going to look for a genuine difference to abuse...
        class Funky : IEnumerable, IEnumerator, IDisposable
        {
            internal bool disposed;

            public IEnumerator GetEnumerator() { return this; }
            public object Current { get { return null; } }
            public bool MoveNext() { return false; }
            public void Reset() { }
            public void Dispose() { disposed = true; }
        }

        private static bool DetectDisposingForEach()
        {
            Funky funky = new Funky();
            foreach (object x in funky) { }
            return funky.disposed;
        }
        #endregion

        #region C# 2.0 / C# 1.2
        class VariantBase
        {
            internal bool variance;
            public void Foo(object sender, EventArgs args)
            {
            }
        }

        class VariantDerived : VariantBase
        {
            public void Foo(object sender, object args)
            {
                variance = true;
            }
        }

        static bool DetectMethodGroupVariance()
        {
            VariantDerived derived = new VariantDerived();
            EventHandler handler = new EventHandler(derived.Foo);
            handler(null, null);
            return derived.variance;
        }
        #endregion

        #region C# 3.0 / C# 2.0
        class TypeInferenceBase
        {
            public bool Foo<T1, T2>(T1 t1, T2 t2) { return false; }
        }

        class TypeInferenceDerived : TypeInferenceBase
        {
            public bool Foo<T>(T t1, T t2) { return true; }
        }

        private static bool DetectSmartTypeInference()
        {
            return new TypeInferenceDerived().Foo(new object(), "");
        }
        #endregion

        #region C# 5.0 / C# 4.0
        private delegate bool Foo();
        private static bool DetectSaneForEachCapture()
        {
            List<Foo> foos = new List<Foo>();
            foreach (bool value in new bool[] { true, false })
            {
                foos.Add(delegate { return value; });
            }
            return foos[0]();
        }
        #endregion
    }
}
