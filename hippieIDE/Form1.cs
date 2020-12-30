using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MaterialSkin.Controls;
using MetroSet_UI.Forms;
namespace hippieIDE
{
    public partial class Form1 : MetroSetForm //MaterialForm
    {

        private Color color;
        static private int lines;
        public Form1()
        {
            
            InitializeComponent();
            metroSetLabel1.ForeColor = System.Drawing.Color.White;
            lines = richTextBox1.Text.Split('\n').Length;
            DrawLines();
            color = richTextBox1.ForeColor;
            //richTextBox3.Text = ""
            //richTextBox3.Text = double.TryParse(";", out double d).ToString() + " " + double.TryParse("]", out d).ToString();
        }
         
        public enum ScrollBarType : uint
        {
            SbHorz = 0,
            SbVert = 1,
            SbCtl = 2,
            SbBoth = 3
        }

        public enum Message : uint
        {
            WM_VSCROLL = 0x0115
        }

        public enum ScrollBarCommands : uint
        {
            SB_THUMBPOSITION = 4
        }

        [DllImport("User32.dll")]
        public extern static int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("User32.dll")]
        public extern static int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private void Form1_Load(object sender, EventArgs e)
        {/*
            //richTextBox1.AcceptsTab = true;
            int startIndex = richTextBox1.GetFirstCharIndexFromLine(1);
            richTextBox1.Select(startIndex + 1, 2);

            //Set the selected text fore and background color
            richTextBox1.SelectionColor = System.Drawing.Color.White;
            richTextBox1.SelectionBackColor = System.Drawing.Color.Blue;*/
            richTextBox2.Text = "----------Output----------";
        }

        private void metroSetSwitch1_SwitchedChanged(object sender)
        {
            if (metroSetSwitch1.Switched)
            {
                realcheck.BackColor = minuscheck.BackColor = spacescheck.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
                this.richTextBox4.BackColor = this.richTextBox3.BackColor = this.richTextBox2.BackColor = this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
                this.richTextBox3.ForeColor = this.richTextBox2.ForeColor = this.richTextBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
                this.Style = MetroSet_UI.Enums.Style.Light;
                metroSetLabel1.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                realcheck.BackColor = minuscheck.BackColor = spacescheck.BackColor =
                this.richTextBox4.BackColor = this.richTextBox3.BackColor = this.richTextBox2.BackColor = this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
                this.richTextBox3.ForeColor = this.richTextBox2.ForeColor = this.richTextBox1.ForeColor = System.Drawing.SystemColors.Menu;
                this.Style = MetroSet_UI.Enums.Style.Dark;
                metroSetLabel1.ForeColor = System.Drawing.Color.White;
            }
            color = richTextBox1.ForeColor;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            this.richTextBox1.Size = new System.Drawing.Size(this.ClientSize.Width-30-350-6-26, this.ClientSize.Height - 15 - 112 - 86 - 6);
            this.richTextBox3.Size = new System.Drawing.Size(350, this.ClientSize.Height - 15 - 112 - 86 - 6);
            this.richTextBox2.Size = new System.Drawing.Size(this.ClientSize.Width-30, 112);
            this.richTextBox4.Size = new System.Drawing.Size(25, this.ClientSize.Height - 15 - 112 - 86 - 6);
            //this.ClientSie = new System.Drawing.Size(977, 580);
        }

        private void richTextBox1_VScroll(object sender, EventArgs e)
        {
            int nPos = GetScrollPos(richTextBox1.Handle, (int)ScrollBarType.SbVert);
            nPos <<= 16;
            uint wParam = (uint)ScrollBarCommands.SB_THUMBPOSITION | (uint)nPos;
            SendMessage(richTextBox4.Handle, (int)Message.WM_VSCROLL, new IntPtr(wParam), new IntPtr(0));
        }

        private void richTextBox1_HScroll(object sender, EventArgs e)
        {
            int nPos = GetScrollPos(richTextBox1.Handle, (int)ScrollBarType.SbVert);
            nPos <<= 16;
            uint wParam = (uint)ScrollBarCommands.SB_THUMBPOSITION | (uint)nPos;
            SendMessage(richTextBox4.Handle, (int)Message.WM_VSCROLL, new IntPtr(wParam), new IntPtr(0));
        }
        private void DrawLines()
        {
            
                for (int i = 1; i <= lines; i++)
                    richTextBox4.Text += i.ToString() + '\n';
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
        //int lines = richTextBox1.Text.Split('\n').Length;
            if (lines != richTextBox1.Text.Split('\n').Length)
            {
                lines = richTextBox1.Text.Split('\n').Length;
                richTextBox4.Text = "";
                DrawLines();
                this.richTextBox1.ForeColor = color;
                //richTextBox1.SelectionStart = 0;
                //richTextBox1.SelectionLength = richTextBox1.Text.Length;
                //richTextBox1.SelectionColor = color;
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (e.Handled == )
        }
        private void metroSetButton1_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = 0;
            richTextBox1.SelectionLength = richTextBox1.Text.Length;

            richTextBox1.SelectionColor = color;
            richTextBox1.SelectionStart = 0;
            richTextBox1.SelectionLength = 0;
            //this.richTextBox1.= color;

            HippieLang l = new HippieLang();
            //try
            {
                richTextBox2.Text = l.InputOutput(richTextBox1.Text, out int i, out string[] words, out bool exitCode,
                    outputmode, spaces, minus, real)/* + i + words[i]*/;
                if (!exitCode)
                {
                    int pos  = GetLetterPosition(i, richTextBox1.Text.ToLower(), words, out int length);
                    richTextBox1.SelectionStart = pos;
                    //richTextBox2.Text += length;// GetLetterPosition(i, richTextBox1.Text, words, out length);//start xx111 = 7;
                    //richTextBox1.SelectionStart = i;
                    richTextBox1.SelectionLength = length;//"".Length;, 
                    richTextBox1.SelectionColor = Color.Red;
                }
            }
            //catch (FormatException ex)
            //{
            //    richTextBox2.Text = ex.Message;
            //}
            //richTextBox3.Text += i + '\n'.ToString();
            
        }
        //ноль-слово 0, 
        private int GetLetterPosition(int n, string s, string[] words, out int length)
        {
            //length = 1;
            int i;
            int j = 0;
            //bool letters = false;
            for (i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ' || s[i] == '\n')
                {
                    continue;
                }
                if (j == n)
                    break;
                int k;
                //richTextBox3.Text += '\n'.ToString() + s[i].ToString() + ' '.ToString() + words[j] + ' '.ToString() + i;
                for (k = 0; k < words[j].Length; k++)
                {
                    if (words[j][k] == s[i])
                    {
                        //richTextBox3.Text += s[i];
                        i++;
                    }

                }
                if (k == words[j].Length) {
                    j++;
                i--;}
                
                //else
                //{
                //    if (letters)
                //        continue;
                //    letters = true;
                //    j++;
                //}
                //if (word == j)
                //    return i;
                
            }
            length = words[j].Length;
            return i;
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            this.richTextBox1.ForeColor = color;
        }
        private bool outputmode = false;
        private void metroSetSwitch2_SwitchedChanged(object sender)
        {
            if (metroSetSwitch2.Switched)
            {
                metroSetLabel1.Text = "Output Mode On";
                outputmode = true;
            }
            else
            {
                metroSetLabel1.Text = "Output Mode Off";
                outputmode = false;
            }
        }
        bool minus = false;
        private void minuscheck_Click(object sender, EventArgs e)
        {
            string mesg = "минус после функции";
            if (minus)
            {
                minuscheck.Text = $"OFF {mesg}";
                minus = false;
            }
            else
            {
                minuscheck.Text = $"ON {mesg}";
                minus = true;
            }
        }
        bool spaces = true;
        private void spacescheck_Click(object sender, EventArgs e)
        {
            string mesg = "пробелы между функциями";
            if (spaces)
            {
                spacescheck.Text = $"OFF {mesg}";
                spaces = false;
            }
            else
            {
                spacescheck.Text = $"ON {mesg}";
                spaces = true;
            }
        }
        bool real = false;

        private void realcheck_Click(object sender, EventArgs e)
        {
            string mesg = "исключительно вещ. числа";
            if (real)
            {
                realcheck.Text = $"OFF {mesg}";
                real = false;
            }
            else
            {
                realcheck.Text = $"ON {mesg}";
                real = true;
            }
        }
    }
}
