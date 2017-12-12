using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WLServer
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();

            int width_offset = 1000 - ClientRectangle.Right;
            int height_offset = 600 - ClientRectangle.Bottom;

            Width += width_offset;
            Height += height_offset;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0 && textBox2.Text.Last() == '\n')
            {
                textBox1.AppendText(textBox2.Text);
                textBox2.Clear();
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            textBox2.Focus();
        }

    }
}
