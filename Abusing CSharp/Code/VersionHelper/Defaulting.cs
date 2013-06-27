// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

namespace VersionHelper
{
    public class T1
    {
        public bool DetectDefaulting()
        {
            return false;
        }
    }

    public class T2 : T1
    {
        public bool DetectDefaulting(int x = 0)
        {
            return true;
        }
    }
}
