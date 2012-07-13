using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LazyCow
{
    public partial class KeysSettings : Form
    {
        public event EventHandler ChangedKeys;

        public KeysSettings()
        {
            InitializeComponent();

            textBox_apiKey.Text = LazyCow.Properties.Settings.Default.api_key;
            textBox_sharedSecret.Text = LazyCow.Properties.Settings.Default.shared_secret;

            var root = new TreeNode("Settings");
            var child = new TreeNode("Authentication");
            root.Nodes.Add(child);

            treeView1.Nodes.Add(root);
            treeView1.SelectedNode = child;
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            LazyCow.Properties.Settings.Default.api_key = textBox_apiKey.Text;
            LazyCow.Properties.Settings.Default.shared_secret = textBox_sharedSecret.Text;
            ChangedKeys(this, new EventArgs());
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
