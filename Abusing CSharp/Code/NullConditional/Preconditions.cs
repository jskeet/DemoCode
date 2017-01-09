// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.

using System;
using static System.FormattableString;

namespace NullConditional
{
    public static class Preconditions
    {
        public static T CheckNotNull<T>(T value, string paramName) where T : class =>
            value ?? throw new ArgumentNullException(paramName);

        public static FailedCheck CheckArgument(bool condition, string paramName) =>
            condition ? null : new FailedCheck(message => new ArgumentException(message, paramName));

        public static FailedCheck CheckState(bool condition) =>
            condition ? null : new FailedCheck(message => new InvalidOperationException(message));
    }

    public class FailedCheck
    {
        private Func<string, Exception> factory;

        internal FailedCheck(Func<string, Exception> factory) => this.factory = factory;

        public void Report(FormattableString message)
        {
            GC.SuppressFinalize(this);
            throw factory(Invariant(message));
        }

        ~FailedCheck() => throw new UnreportedExceptionException(factory("(unreported)"));
    }

    internal class UnreportedExceptionException : Exception
    {
        internal UnreportedExceptionException(Exception innerException)
            : base("Failed check was not reported", innerException)
        {
        }
    }
}
