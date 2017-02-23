// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

namespace OddsAndEnds
{
    delegate j j(MiddleBit x);

    class MiddleBit
    {
        public static implicit operator MiddleBit(j _) => null;
        public static implicit operator j(MiddleBit _) => null;
        public static MiddleBit operator <=(string lhs, MiddleBit rhs) => null;
        public static MiddleBit operator <=(MiddleBit lhs, MiddleBit rhs) => null;

        // Useless, but required by compiler. Picky, picky, picky.
        public static MiddleBit operator >=(MiddleBit lhs, MiddleBit rhs) => null;
        public static MiddleBit operator >=(string lhs, MiddleBit rhs) => null;
    }

    class AsciiArtProperty
    {
        j _ => __ => ___ => "Awesome code" <= ___ <= __ <= _ ;

        // Stuff here

        j __ => ___ => ____ => _____ => "Awesomer code" <= _____ <= ____ <= ___ <= __;

    }
}
