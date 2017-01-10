// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
namespace NullConditional
{
    class Logger
    {
        // TODO: Decide the log level from a configuration or whatever...
        public ActiveLogger Debug { get; } = null;
        public ActiveLogger Info { get; } = new ActiveLogger("INFO");
        public ActiveLogger Warn { get; } = new ActiveLogger("WARNING");
    }
}
