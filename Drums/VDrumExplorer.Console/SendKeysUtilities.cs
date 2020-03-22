// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.IO;
using System.Reflection;

namespace VDrumExplorer.Console
{
    /// <summary>
    /// Utility methods to work with the SendKeys class.
    /// </summary>
    public static class SendKeysUtilities
    {
        private static readonly Type sendKeysClass = GetSendKeysClass();

        public static bool HasSendKeys => sendKeysClass is object;

        private static Type GetSendKeysClass()
        {
            try
            {
                var assembly = Assembly.Load("System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                return assembly?.GetType("System.Windows.Forms.SendKeys");
            }
            catch (IOException)
            {
                return null;
            }
        }

        public static void SendWait(string keys)
        {
            if (sendKeysClass is null)
            {
                throw new InvalidOperationException($"Cannot use SendKeys on this platform.");
            }
            var method = sendKeysClass.GetMethod("SendWait", new[] { typeof(string) });
            method.Invoke(null, new object[] { keys });
        }
    }
}
