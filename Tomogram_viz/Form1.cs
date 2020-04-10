using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Tomogram_viz
{
    public partial class Form1 : Form
    {
        Bin Tg = new Bin();
        View view = new View();
        bool loaded = false;
        int currentLayer;
        public Form1()
        {
            InitializeComponent();
            trackBar2.Maximum = 1000;
            trackBar3.Minimum = 500;
            trackBar3.Maximum = 3000;
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string str = dialog.FileName;
                Tg.readBin(str);
                trackBar1.Maximum = Bin.Z - 1;
                view.SetupView(glControl1.Width, glControl1.Height);
                loaded = true;
                glControl1.Invalidate();
            }
        }

        bool needReload = false;
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (loaded)
            {
                if (radioButton1.Checked == true)
                {
                    view.DrawQuadstrip(currentLayer, trackBar2.Value, trackBar3.Value);
                    glControl1.SwapBuffers();
                }
                if (radioButton3.Checked == true)
                {
                    view.DrawQuads(currentLayer, trackBar2.Value, trackBar3.Value);
                    glControl1.SwapBuffers();
                }
                if (radioButton2.Checked == true)
                {
                    if(needReload)
                    {
                        view.genereateTextureImage(currentLayer, trackBar2.Value, trackBar3.Value);
                        view.Load2DTexture();
                        needReload = false;
                    }
                    view.DrawTexture();
                    glControl1.SwapBuffers();
                }
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            currentLayer = trackBar1.Value;
            needReload = true;
            //glControl1.Invalidate();
        }
        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                displayFPS();
                glControl1.Invalidate();
            }
        }

        int FrameCount;
        DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);
        void displayFPS()
        {
            if (DateTime.Now >= NextFPSUpdate)
            {
                this.Text = String.Format("CT Visualizer (fps = {0})", FrameCount);
                NextFPSUpdate = DateTime.Now.AddSeconds(1);
                FrameCount = 0;
            }
            FrameCount++;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            Application.Idle += Application_Idle;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            needReload = true;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            needReload = true;
        }
    }
}
