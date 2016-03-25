// Copyright 2016 Jon Skeet.
// Licensed under the Apache License Version 2.0.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Shed.Controllers.Reflection
{
    internal sealed class ReflectiveController : IController
    {
        public IImmutableList<ICommand> Commands { get; }
        public string Name { get; }

        internal ReflectiveController(object target)
        {
            var typeInfo = target.GetType().GetTypeInfo();
            Name = GetDescription(typeInfo);
            Commands = typeInfo.DeclaredMethods
                .Select(m => new { Method = m, Description = GetDescription(m) })
                .Where(pair => pair.Description != null)
                .Select(pair => new ReflectiveCommand(target, pair.Method, pair.Description))
                .ToImmutableList<ICommand>();
        }

        private static string GetDescription(MemberInfo info)
        {
            return info
                .GetCustomAttributes()
                .OfType<DescriptionAttribute>()
                .SingleOrDefault()
                ?.Description;
        }

        private class ReflectiveCommand : ICommand
        {
            private readonly MethodInfo method;
            private readonly object target;

            public string Name => method.Name;
            public string Description { get; }

            internal ReflectiveCommand(object target, MethodInfo method, string description)
            {
                this.target = target;
                this.method = method;
                Description = description;
            }

            public void Execute(string[] arguments)
            {
                method.Invoke(target, arguments.Zip(method.GetParameters(), (arg, parameter) => Convert.ChangeType(arg, parameter.ParameterType)).ToArray());
            }
        }
    }
}
