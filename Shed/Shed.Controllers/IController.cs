// Copyright 2016 Jon Skeet.
// Licensed under the Apache License Version 2.0.

using System.Collections.Immutable;

namespace Shed.Controllers
{
    /// <summary>
    /// A controller, e.g. for lights or music.
    /// </summary>
    public interface IController
    {
        string Name { get; }
        IImmutableList<ICommand> Commands { get; }
    }
}
