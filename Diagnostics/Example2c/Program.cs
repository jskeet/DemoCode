using System;
using System.Windows.Forms;

namespace Example2c
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var groupBox = new GroupBox();
            var items = new[] { "Foo", "Bar", "Baz" };

            foreach (var item in items)
            {
                var button = new Button { Text = item };
                groupBox.Controls.Add(button);
            }

            var form = new Form { Controls = { groupBox } };
            Application.Run(form);
        }
    }
}
