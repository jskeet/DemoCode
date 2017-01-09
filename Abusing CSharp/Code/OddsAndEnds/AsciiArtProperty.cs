// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OddsAndEnds
{
    delegate j j(MiddleBit x);

    class MiddleBit
    {
        public static implicit operator MiddleBit(j _) => null;
        public static implicit operator j(MiddleBit _) => null;
        public static MiddleBit operator <=(string lhs, MiddleBit rhs) => null;
        public static MiddleBit operator >=(string lhs, MiddleBit rhs) => null;
        public static MiddleBit operator <=(MiddleBit lhs, MiddleBit rhs) => null;
        public static MiddleBit operator >=(MiddleBit lhs, MiddleBit rhs) => null;
    }

    class AsciiArtProperty
    {
        j _ => __ => ___ => ____ => "Some text" <= ____ <= ___ <= __ <= _ ;        
    }
}
