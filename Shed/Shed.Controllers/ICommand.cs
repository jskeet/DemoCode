// Copyright 2016 Jon Skeet.
// Licensed under the Apache License Version 2.0.

namespace Shed.Controllers
{
    /// <summary>
    /// A command within a controller.
    /// </summary>
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }

        void Execute(params string[] arguments);
    }
}
