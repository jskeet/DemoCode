using System;

namespace CSharp7
{
    public class OutDeconstruction
    {
        public static void Main()
        {

            
            var (code, message) = new Deconstructable { X = 10, Message = "Hi" }; // deconstruct any object with a proper Deconstruct method into existing variables
            Console.Write(message); // world

            var (code2, message2, exception) = new Deconstructable();
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