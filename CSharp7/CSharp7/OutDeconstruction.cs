// Copyright 2016 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    public class OutDeconstruction
    {
        public static void Main()
        {
            int code = 20;
            string message = "Before";
            // Deconstruct any object with a proper Deconstruct method into existing variables
            // (or properties)
            (code, message) = new Deconstructable { X = 10, Message = "Hi" };
            Console.Write(message); // "Hi"

            // Deconstruct into new var variables
            var (code2, message2, exception) = new Deconstructable();
            var (code3, message3) = new Deconstructable();

            // Deconstruct from existing instance
            var instance = new Deconstructable { X = 10, Message = "Hi" };
            var (code4, message4) = instance;
        }
    }

    public class Deconstructable
    {
        public int X { get; set; }
        public string Message { get; set; }
        public Exception Error { get; set; }

        // Nifty trick: use tuple deconstruction for the body
        // Performance implications?
        public void Deconstruct(out int x, out string message) => (x, message) = (X, Message);

        internal void Deconstruct(out int x, out string message, out Exception e)
        {
            x = X;
            message = Message;
            e = Error;
        }

        internal void CopyTo(Deconstructable other) => (other.X, other.Message, other.Error) = this;
    }
}