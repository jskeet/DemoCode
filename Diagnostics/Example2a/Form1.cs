using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Example2a
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void FetchBuckets(object sender, EventArgs e)
        {
            var client = await StorageClient.CreateAsync();

            var buckets = client.ListBucketsAsync("jonskeet-uberproject");
            using (var iterator = buckets.GetEnumerator())
            {
                while (await iterator.MoveNext())
                {
                    var bucket = iterator.Current;
                    var button = new Button { Text = bucket.Name };
                    button.Click += delegate { MessageBox.Show($"I would load bucket {bucket.Name}"); };
                    bucketsGroupBox.Controls.Add(button);
                }
            }
        }
    }
}
