using System;

namespace WorkingInPreview3
{
    public class OutDeconstruction
    {
        public static void Main()
        {

            int code;
            string message;

            var pair = (42, "hello");
            (code, message) = pair; // deconstruct a tuple into existing variables
            Console.Write(message); // hello

            (code, message) = new Deconstructable { X = 10, Message = "Hi" }; // deconstruct any object with a proper Deconstruct method into existing variables
            Console.Write(message); // world

            (int code2, string message2) = pair; // deconstruct into new variables
            var (code3, message3) = new Deconstructable(); // deconstruct into new 'var' variables

            var (c, m, error) = new Deconstructable();

        }
    }

    public class Deconstructable
    {
        public int X { get; set; }
        public string Message { get; set; }
        public Exception Error { get; set; }

        public void Deconstruct(out int x, out string message)
        {
            x = X;
            message = Message;
        }

        internal void Deconstruct(out int x, out string message, out Exception e)
        {
            x = X;
            message = Message;
            e = Error;
        }
    }
}