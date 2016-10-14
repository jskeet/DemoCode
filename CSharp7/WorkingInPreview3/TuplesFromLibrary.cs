using ClassLibrary;
using System;

namespace WorkingInPreview3
{
    class TuplesFromLibrary
    {
        static void Main()
        {
            var (px, py) = TupleFactory.GetPosition();
            var velocity = TupleFactory.GetVelocity();

            //Console.WriteLine($"{position.x}, {position.y}");
            Console.WriteLine($"{velocity.dx}, {velocity.dy}");
        }
        
    }
}
