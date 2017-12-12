using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace WLServer
{
    public partial class ServerForm : Form
    {
        public System.Windows.Forms.Timer timer;
        public ServerForm()
        {
            InitializeComponent();

            Width += 1;

            sis.ServerPT.Form = this;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 20;
            timer.Tick += new EventHandler(ServerClock);
            timer.Enabled = true;

            sis.Game.Initialize();

            sis.ServerPT.StartListen();
        }
        void ServerClock(object sender, EventArgs e)
        {
            sis.Game.Update();
        }

        delegate void DLText(String text);
        public void PushLog(String text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new DLText(PushLog), new object[] { text });
                }
                else
                {
                    textBox1.AppendText(text);
                }
            }
            catch (ObjectDisposedException) { ; }
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
        private void Server_SizeChanged(object sender, EventArgs e)
        {
            textBox1.Size = new Size(ClientRectangle.Width, ClientRectangle.Height - 22);

            textBox2.Location = new Point(0, textBox1.Size.Height);
            textBox2.Size = new Size(ClientRectangle.Width, 22);
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            sis.ServerPT.Exit();
        }
    }
}
