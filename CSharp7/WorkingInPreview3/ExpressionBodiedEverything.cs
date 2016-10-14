using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkingInPreview3
{
    class ExpressionBodiedEverything
    {
        private int x;

        //ExpressionBodiedEverything(int x) => this.x = x;

        private int foo;
        public int Foo
        {
            get => foo;
            set => foo = value;
        }
    }
}
