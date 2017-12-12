using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WLClient
{
    public partial class ClientForm : Form
    {
        //  操作
        public int MouseX, MouseY;
        public bool PushingRight = false;

        Timer timer;

        public ClientForm()
        {
            InitializeComponent();

            sis.GameDef.Form = this;
            sis.GameDef.LogBox = textBox1;

            textBox1.AppendText("Enter Hostname\n");

            sis.SStd.AdjustWindowSize(this, 700, 500);

            sis.CPlayer.LoadSpellImage();
            sis.DrawCache.Initialize();

            timer = new Timer();
            timer.Interval = 200;   //  move order interval
            timer.Tick += new EventHandler(ClientClock);
            timer.Enabled = true;
        }

        private void ClientClock(Object source, EventArgs e)
        {
            if (PushingRight && sis.CPlayer.ValidMousePos(MouseX, MouseY))
            {
                //  move order
                //  use GetGamePos to get position
                sis.IPair pos = sis.CPlayer.GetGamePos(MouseX, MouseY);
                sis.ClientPT.PushControl(0, (int)pos.X, (int)pos.Y, 0, null);
            }
        }
        private void ClientForm_KeyUp(object sender, KeyEventArgs e)
        {
            sis.IPair pos = null;
            switch (e.KeyCode)
            {
                case Keys.Q:
                case Keys.W:
                case Keys.E:
                case Keys.R:
                case Keys.S:
                case Keys.D:
                case Keys.F:
                    if (sis.CPlayer.ValidMousePos(MouseX, MouseY))
                    {
                        //  cast order
                        pos = sis.CPlayer.GetGamePos(MouseX, MouseY);
                        sis.ClientPT.PushControl(1, (int)pos.X, (int)pos.Y, e.KeyCode, null);
                    }
                    break;
                default:
                    break;
            }
        }
        private void ClientForm_Paint(object sender, PaintEventArgs e)
        {
            sis.CPlayer.FillGame(e.Graphics);
            sis.CPlayer.DrawPlayerUI(e.Graphics);
        }
        private void ClientForm_SizeChanged(object sender, EventArgs e)
        {
            sis.CPlayer.CacheSize(ClientRectangle.Width, ClientRectangle.Height);

            textBox1.Location = new Point(sis.CPlayer.CurrentDisplayWidth, 0);
            textBox1.Size = new Size(ClientRectangle.Width - sis.CPlayer.CurrentDisplayWidth, ClientRectangle.Height - 22);

            textBox2.Location = new Point(sis.CPlayer.CurrentDisplayWidth, ClientRectangle.Height - 22);
            textBox2.Size = new Size(ClientRectangle.Width - sis.CPlayer.CurrentDisplayWidth, 22);

            Invalidate();
        }
        delegate void DLSetText(String text);
        public void PushLog(String text) {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new DLSetText(PushLog), new object[] { text });
                }
                else
                {
                    textBox1.AppendText(text);
                }
            }
            catch
            {

            }
        }
        delegate void DLClose();
        public void ClientClose()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new DLClose(ClientClose));
                }
                else
                {
                    sis.ClientPT.PushString("Close");
                }
            }
            catch
            {

            }
        }
        public void UpdateForm()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new DLClose(UpdateForm));
                }
                else
                {
                    Invalidate();
                }
            }
            catch
            {

            }
        }
        //  mouse
        private void ClientForm_MouseMove(object sender, MouseEventArgs e)
        {
            MouseX = e.X;
            MouseY = e.Y;

            if (textBox2.Focused) textBox1.Focus();
        }
        //  wheel
        private void TextBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (sis.CPlayer.ValidMousePos(MouseX, MouseY) == false) return;
            int scroll_value = e.Delta * 120;
            if (scroll_value > 0)
            {
                //  增倍
                if (sis.CPlayer.CurrentScale != 2) sis.CPlayer.CurrentScale <<= 1;
            }
            else
            {
                if (sis.CPlayer.CurrentScale != 1) sis.CPlayer.CurrentScale >>= 1;
            }
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            TextBox1_MouseWheel(null, e);
        }
        private void ClientForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                PushingRight = true;
                if (sis.CPlayer.ValidMousePos(MouseX, MouseY))
                {
                    //  move order
                    //  use GetGamePos to get position
                    sis.IPair pos = sis.CPlayer.GetGamePos(MouseX, MouseY);
                    sis.ClientPT.PushControl(0, (int)pos.X, (int)pos.Y, 0, null);
                }
            }
        }

        private void ClientForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) PushingRight = false;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0 && textBox2.Text.Last() == '\n')
            {
                Char[] text = textBox2.Text.ToCharArray();
                sis.ClientPT.PushString(new String(text, 0, text.Length - 2));
                textBox2.Clear();
            }
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            sis.DrawThread.Exit();
        }
    }
}
