// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OddsAndEnds
{
    [AttributeUsage(AttributeTargets.GenericParameter)]
    public sealed class MustBeImmutable : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Immutable : Attribute { }

    class Foo<[MustBeImmutable] T>
    {
    }

    [Immutable] class Bar
    {
        public string Name { get; }
        public Bar(string name)
        {
            Name = name;
        }
    }

    class Baz
    {
        public string Name { get; set; }
    }

    class TypeParameterAttributes
    {
        static void Main()
        {
            Foo<Bar> fooBar = new Foo<Bar>();
            // TODO: Write the Roslyn analyzer to handle this...
            Foo<Baz> fooBaz = new Foo<Baz>();
        }
    }
}
