using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RabbitHole
{
    internal static class FormHelper
    {
        internal static void ShowText(IEnumerable<string> lines)
        {
            ShowText(lines, 30);
        }

        internal static void ShowText(string text)
        {
            ShowText(new[] { text });
        }

        internal static void ShowText(string text, int size)
        {
            ShowText(new[] { text }, size);
        }

        internal static void ShowText(IEnumerable<string> lines, int size)
        {
            var form = new Form
            {
                Width = 900,
                Height = 700,
                Controls = {
                    new Panel {
                       AutoScroll = true,
                       Dock = DockStyle.Fill,
                       Controls = {
                           new Label {
                               AutoSize = true,
                               Text = string.Join("\r\n", lines),
                               Font = new Font(FontFamily.GenericSansSerif, size),
                           }
                       }
                    }
                }
            };
            Application.Run(form);
        }
    }
}
