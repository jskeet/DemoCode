using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OddsAndEnds
{
    class ShowMongolianVowelSeparator
    {
        static void Main()
        {
            var form = new Form
            {
                Controls = { new TextBox { Text = "Copy here: >\u180e<" } }
            };
            Application.Run(form);
        }
    }
}
