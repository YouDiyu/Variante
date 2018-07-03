using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VariationAK
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string Filename;
        Image tmpImage;
        Bitmap oriimage;//originale Image
        Bitmap oriimage0;
        Bitmap oriimage1;
        Size tmpsiz;
        bool mousedown = false;
        bool mouseup = false;
        bool mousemove = false;
        long mouseTick = 0;
        Graphics graf = null;
        Pen pen = new Pen(Color.Red);
        Point pt;
        List<Point> ptlist = null;
        List<Point> ptlistarea = null;
        double[,] Eedge;
        double[,] Eterm;
        double[,] Kontur, dx, dy, absGru;
        int[,] u;
        int[,] u0;
        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog opd = new OpenFileDialog();
            opd.DefaultExt = ".bmp";
            opd.Filter = "Image Files(*.bmp,*.jpg,*.png,*.TIF)|*.bmp;*.jpg;*.png;*.TIF||";
            if (DialogResult.OK != opd.ShowDialog(this))
            {
                return;
            }
            Filename = opd.FileName;
            textBox1.AppendText(opd.FileName);
            lodaimage();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            {
                if (mousedown == false)
                {
                    mousedown = true;
                    button2.Enabled = false;
                }
                else
                {
                    return;
                }
            }
        }
        public void lodaimage()
        {
            tmpImage = Image.FromFile(Filename);
            Size pansiz = panel1.ClientSize;
            tmpsiz = tmpImage.Size;
            if (tmpsiz.Width > pansiz.Width || tmpsiz.Height > pansiz.Height)
            {
                double rImage = tmpsiz.Width * 1.0 / tmpsiz.Height;
                double rWnd = pansiz.Width * 1.0 / pansiz.Height;
                if (rImage < rWnd) // image more high
                {

                    tmpsiz.Height = pansiz.Height;
                    tmpsiz.Width = (int)(tmpsiz.Height * rImage);
                }
                else //image is more wide
                {

                    tmpsiz.Width = pansiz.Width;
                    tmpsiz.Height = (int)(pansiz.Width / rImage);
                }
            }
            panel1.Size = tmpsiz;
            oriimage = new Bitmap(tmpImage, tmpsiz);
            Color pixcolor = Color.FromArgb(0);
            double pixval = 0;
            for (int i = 0; i < oriimage.Height; i++)
            {
                for (int j = 0; j < oriimage.Width; j++)
                {
                    pixcolor = oriimage.GetPixel(j, i);
                    pixval = pixcolor.R * 0.3 + pixcolor.G * 0.59 + pixcolor.B * 0.11;
                    oriimage.SetPixel(j, i, Color.FromArgb(Convert.ToInt32(pixval), Convert.ToInt32(pixval), Convert.ToInt32(pixval)));
                }
            }
            oriimage1 = oriimage;
            //Grauss();//useless
            panel1.BackgroundImage = oriimage1;
            textBox1.AppendText("Image load" + "\n");
            panel1.Refresh();
            oriimage0 = new Bitmap(oriimage1, tmpsiz);
            //oriimage=>grau;oriimage1=>nach gaussian;oriimage0=>copy von oriimage1
        } 
        public void Initialisierung()
        {
            Eedge = new double[oriimage0.Width, oriimage0.Height];
            Eterm = new double[oriimage0.Width, oriimage0.Height];
            for (int i = 0; i < oriimage0.Width; i++)
            {
                for (int j = 0; j < oriimage0.Height; j++)
                {
                    if (i == 0 || j == 0 || i == oriimage0.Width - 1 || j == oriimage0.Height - 1)
                    {
                        Eedge[i, j] = 0;
                    }
                    else
                    {
                        double Iy = 0;
                        double Ix = 0;
                        Iy = -oriimage0.GetPixel(i - 1, j - 1).R - 2 * oriimage0.GetPixel(i - 1, j).R - oriimage0.GetPixel(i - 1, j + 1).R
                            + oriimage0.GetPixel(i + 1, j - 1).R + 2 * oriimage0.GetPixel(i + 1, j).R + oriimage0.GetPixel(i + 1, j + 1).R;
                        Ix = -oriimage0.GetPixel(i - 1, j - 1).R - 2 * oriimage0.GetPixel(i, j - 1).R - oriimage0.GetPixel(i + 1, j - 1).R
                            + oriimage0.GetPixel(i - 1, j + 1).R + 2 * oriimage0.GetPixel(i, j + 1).R + oriimage0.GetPixel(i + 1, j + 1).R;
                        Eedge[i, j] = Math.Sqrt(Ix * Ix + Iy * Iy);
                    }
                }
            }
            for (int i = 0; i < oriimage0.Width; i++)
            {
                for (int j = 0; j < oriimage0.Height; j++)
                {
                    if (i <= 1 || j <= 1 || i >= oriimage0.Width - 2 || j >= oriimage0.Height - 2)
                    {
                        Eterm[i, j] = 0;
                    }
                    else
                    {
                        double Cx, Cy, Cxx, Cyy, Cxy;
                        Cx = 0.5 * (oriimage0.GetPixel(i, j + 1).R - oriimage0.GetPixel(i, j - 1).R);
                        Cy = 0.5 * (oriimage0.GetPixel(i + 1, j).R - oriimage0.GetPixel(i - 1, j).R);
                        Cyy = 0.25 * (oriimage0.GetPixel(i + 2, j).R - 2 * oriimage0.GetPixel(i, j).R + oriimage0.GetPixel(i - 2, j).R);
                        Cxx = 0.25 * (oriimage0.GetPixel(i, j + 2).R - 2 * oriimage0.GetPixel(i, j).R + oriimage0.GetPixel(i, j - 2).R);
                        Cxy = 0.25 * (oriimage0.GetPixel(i + 1, j + 1).R + oriimage0.GetPixel(i - 1, j - 1).R - oriimage0.GetPixel(i - 1, j + 1).R - oriimage0.GetPixel(i + 1, j - 1).R);
                        Eterm[i, j] = (Cyy * Cx * Cx - 2 * Cxy * Cx * Cy + Cxx * Cy * Cy) / Math.Pow(1 + Cx * Cx + Cy * Cy, 3 / 2);
                    }
                }
            }
            ptlistarea = ptlist;
            area();
            u0 = u;
        }
        public void area()
        {
            oriimage1 = new Bitmap(oriimage0);
            graf = Graphics.FromImage(oriimage1);
            for (int i = 1; i < ptlistarea.Count; i++)
            {
                graf.DrawLine(pen, ptlistarea[i - 1], ptlistarea[i]);
            }
            graf.DrawLine(pen, ptlistarea[0], ptlistarea[ptlistarea.Count - 1]);
            graf.Save();
            u = new int[oriimage1.Width, oriimage1.Height];
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    u[i, j] = 0;
                }
            }
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (i == 0 || j == 0 || i == oriimage1.Width - 1 || j == oriimage1.Height - 1)
                    {
                        u[i, j] = 1;
                    }
                    else
                    {
                        if (oriimage1.GetPixel(i, j).R == 255)
                        {
                            u[i, j] = 0;
                        }
                        else
                        {
                            double t;
                            //t = u[i - 1, j + 1] + u[i - 1, j] + u[i - 1, j - 1] + u[i, j + 1] + u[i, j - 1] + u[i + 1, j + 1] + u[i + 1, j] + u[i + 1, j - 1];
                            t = u[i, j + 1] + u[i, j - 1] + u[i - 1, j] + u[i + 1, j];
                            if (t > 0)
                            {
                                u[i, j] = 1;
                            }
                        }
                    }
                }

            }
            for (int i = oriimage1.Width - 2; i >= 1; i--)
            {
                for (int j = 1; j < oriimage1.Height - 1; j++)
                {
                    if (oriimage1.GetPixel(i, j).R == 255 && oriimage1.GetPixel(i, j).G == 0)
                    {
                        u[i, j] = 0;
                    }
                    else
                    {
                        double t;
                        t = u[i, j + 1] + u[i, j - 1] + u[i - 1, j] + u[i + 1, j];
                        if (t > 0)
                        {
                            u[i, j] = 1;
                        }
                    }
                }
            }
            for (int j = 1; j < oriimage.Height - 1; j++)
            {
                for (int i = 1; i < oriimage.Width - 1; i++)
                {
                    if (oriimage1.GetPixel(i, j).R == 255 && oriimage1.GetPixel(i, j).G == 0)
                    {
                        u[i, j] = 0;
                    }
                    else
                    {
                        double t;
                        t = u[i, j + 1] + u[i, j - 1] + u[i - 1, j] + u[i + 1, j];
                        if (t > 0)
                        {
                            u[i, j] = 1;
                        }
                    }
                }
            }
            for (int j = oriimage1.Height - 2; j > 1; j--)
            {
                for (int i = 1; i < oriimage.Width - 1; i++)
                {
                    if (oriimage1.GetPixel(i, j).R == 255 && oriimage1.GetPixel(i, j).G == 0)
                    {
                        u[i, j] = 0;
                    }
                    else
                    {
                        double t;
                        t = u[i, j + 1] + u[i, j - 1] + u[i - 1, j] + u[i + 1, j];
                        if (t > 0)
                        {
                            u[i, j] = 1;
                        }
                    }
                }
            }
            oriimage1 = new Bitmap(oriimage0);
        }
        public void plot1()
        {
            oriimage0 = new Bitmap(oriimage1, tmpsiz); ;
            dx = new double[oriimage1.Width, oriimage1.Height];
            dy = new double[oriimage1.Width, oriimage1.Height];
            double au;
            Kontur = new double[oriimage1.Width, oriimage1.Height];
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (i == 0 || j == 0 || i == (oriimage1.Width - 1) || j == (oriimage1.Height - 1))
                    {
                        dx[i, j] = 0;
                        dy[i, j] = 0;
                    }
                    else
                    {
                        dx[i, j] = (u0[i + 1, j] - u0[i, j]) / Math.Sqrt(Math.Pow(u0[i + 1, j] - u0[i, j], 2) + Math.Pow((u0[i, j + 1] - u0[i, j - 1]) / 2, 2) + 0.0000000001);
                        dy[i, j] = (u0[i, j + 1] - u0[i, j]) / Math.Sqrt(Math.Pow((u0[i + 1, j] - u0[i - 1, j]) / 2, 2) + Math.Pow(u0[i, j + 1] - u0[i, j], 2) + 0.0000000001);
                    }
                    au = Math.Sqrt(Math.Pow(dx[i, j], 2) + Math.Pow(dy[i, j], 2));
                    if (au != 0)
                    {
                        Kontur[i, j] = 1;
                    }
                    else
                    {
                        Kontur[i, j] = 0;
                    }
                }
            }
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (Kontur[i, j] == 1)
                    {
                        oriimage0.SetPixel(i, j, Color.Red);
                    }
                }
            }
            panel1.BackgroundImage = oriimage0;
            panel1.Refresh();
            oriimage0 = oriimage1;
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (mousedown == true)
            {
                mousemove = true;
                ptlist = new List<Point>();
                graf = Graphics.FromImage(oriimage1);
                //pt = new Point(e.Location.X, e.Location.Y);  //nur fuer test
                //textBox1.AppendText(Convert.ToDouble(pt.X) + "," + Convert.ToDouble(pt.Y) + "\n");   //nur fuer test
            }
            else
            {
                return;
            }
        }
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousemove == true)
            {
                long ticks = DateTime.Now.Ticks;
                if ((ticks - mouseTick) < 100 * 10000)
                {
                    return;
                }
                mouseTick = ticks;
                mouseup = true;
                Point mittelpt = new Point(0, 0);
                Rectangle oriimagerec = new Rectangle(mittelpt, oriimage1.Size);
                if (oriimagerec.Contains(e.Location))
                {
                    pt = new Point(e.Location.X, e.Location.Y);
                    ptlist.Add(pt);
                    graf.DrawRectangle(pen, pt.X - 1, pt.Y - 1, 3, 3);
                    if (ptlist.Count > 1)
                    {
                        graf.DrawLine(pen, ptlist[ptlist.Count - 2], ptlist[ptlist.Count - 1]);
                    }
                    graf.Save();
                    panel1.Refresh();
                }

                else
                {
                    return;
                }
            }
            
        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseup == true)
            {
                mousedown = false;
                mousemove = false;
                mouseup = false;
                button2.Enabled = true;
                for (int i = 0; i < ptlist.Count; i++)
                {
                    textBox1.AppendText(Convert.ToDouble(ptlist[i].X) + "," + Convert.ToDouble(ptlist[i].Y) + "\n");
                }
                graf.DrawLine(pen, ptlist[0], ptlist[ptlist.Count - 1]);
                graf.Save();
                panel1.Refresh();
                textBox1.AppendText("Kontour1" + "\n");
                Initialisierung();
            }
            else
            {
                return;
            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            int inter = 0;
            inter = Convert.ToInt16(textBox2.Text);
            for (int t = 0; t < inter; t++)
            {
                oriimage1 = new Bitmap(oriimage0, oriimage1.Width, oriimage1.Height);
                u = u0;
                abs_gradient();
                
                evolutionerode();
                //evolutiondilate();
                SI();
                IS();
                for (int i = 0; i < oriimage1.Width; i++)
                {
                    for (int j = 0; j < oriimage1.Height; j++)
                    {
                        if (i <= 2 || j <= 2 || i >= oriimage1.Width - 2 || j >= oriimage1.Height - 2)
                        {
                            u[i, j] = 1;
                        }
                    }
                }
                u0 = u;
                plot1();
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            int inter = 0;
            inter = Convert.ToInt16(textBox2.Text);
            for (int t = 0; t < inter; t++)
            {
                oriimage1 = new Bitmap(oriimage0, oriimage1.Width, oriimage1.Height);
                u = u0;
                abs_gradient();
                evolutiondilate();
                SI();
                IS();
                for (int i = 0; i < oriimage1.Width; i++)
                {
                    for (int j = 0; j < oriimage1.Height; j++)
                    {
                        if (i <= 2 || j <= 2 || i >= oriimage1.Width - 2 || j >= oriimage1.Height - 2)
                        {
                            u[i, j] = 1;
                        }
                    }
                }
                u0 = u;
                plot1();
            }
        }
        public void SI()//膨胀 腐蚀 dilate erode
        {
            int[,] M;
            M = new int[oriimage1.Width, oriimage1.Height];
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    int aus = 0;
                    if (i == 0 || j == 0 || i == (oriimage1.Width - 1) || j == (oriimage1.Height - 1))
                    {
                        u[i, j] = 1;
                    }
                    else
                    {
                        if (u[i - 1, j - 1] == 1 && u[i, j] == 1 && u[i + 1, j + 1] == 1)
                        {
                            aus = 1;
                        }
                        if (u[i, j - 1] == 1 && u[i, j] == 1 && u[i, j + 1] == 1)
                        {
                            aus = 1;
                        }
                        if (u[i - 1, j + 1] == 1 && u[i, j] == 1 && u[i + 1, j - 1] == 1)
                        {
                            aus = 1;
                        }
                        if (u[i - 1, j] == 1 && u[i, j] == 1 && u[i + 1, j] == 1)
                        {
                            aus = 1;
                        }
                        if (aus == 1)
                        {
                            M[i, j] = 1;
                        }
                        else
                        {
                            M[i, j] = 0;
                        }
                    }
                }
            }
            u = M;
        }
        private void IS()//腐蚀 膨胀  erode dilate
        {
            int[,] M;
            M = new int[oriimage1.Width, oriimage1.Height];
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    int aus = 1;
                    if (i == 0 || j == 0 || i == (oriimage1.Width - 1) || j == (oriimage1.Height - 1))
                    {
                        u[i, j] = 1;
                    }
                    else
                    {
                        if (u[i - 1, j - 1] == 0 && u[i, j] == 0 && u[i + 1, j + 1] == 0)
                        {
                            aus = 0;
                        }
                        if (u[i, j - 1] == 0 && u[i, j] == 0 && u[i, j + 1] == 0)
                        {
                            aus = 0;
                        }
                        if (u[i - 1, j + 1] == 0 && u[i, j] == 0 && u[i + 1, j - 1] == 0)
                        {
                            aus = 0;
                        }
                        if (u[i - 1, j] == 0 && u[i, j] == 0 && u[i + 1, j] == 0)
                        {
                            aus = 0;
                        }
                        if (aus == 0)
                        {
                            M[i, j] = 0;
                        }
                        else
                        {
                            M[i, j] = 1;
                        }
                    }
                }
            }
            u = M;
        }

       

        public void abs_gradient()
        {
            absGru = new double[oriimage1.Width, oriimage1.Height];
            double[,] dx, dy;
            dx = new double[oriimage1.Width, oriimage1.Height];
            dy = new double[oriimage1.Width, oriimage1.Height];
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (i == 0 || j == 0 || i == (oriimage1.Width - 1) || j == (oriimage1.Height - 1))
                    {
                        dx[i, j] = 0;
                        dy[i, j] = 0;
                    }
                    else
                    {
                        dx[i, j] = 0.5 * (u[i + 1, j] - u[i, j]) / Math.Sqrt(Math.Pow(u[i + 1, j] - u[i, j], 2) + Math.Pow((u[i, j + 1] - u[i, j - 1]) / 2, 2) + 0.0000000001) + 0.5 * (u[i, j] - u[i - 1, j]) / Math.Sqrt(Math.Pow(u[i, j] - u[i - 1, j], 2) + Math.Pow((u[i, j + 1] - u[i, j - 1]) / 2, 2) + 0.0000000001);
                        dy[i, j] = 0.5 * (u[i, j + 1] - u[i, j]) / Math.Sqrt(Math.Pow((u[i + 1, j] - u[i - 1, j]) / 2, 2) + Math.Pow(u[i, j + 1] - u[i, j], 2) + 0.0000000001) + 0.5 * (u[i, j] - u[i, j - 1]) / Math.Sqrt(Math.Pow((u[i + 1, j] - u[i - 1, j]) / 2, 2) + Math.Pow(u[i, j] - u[i, j - 1], 2) + 0.0000000001);
                    }
                    absGru[i, j] = Math.Sqrt(Math.Pow(dx[i, j], 2) + Math.Pow(dy[i, j], 2));
                }
            }
        }
        public void evolutiondilate()
        {
            double alpha = 1;
            double beta = 1;
            double lamda = Convert.ToDouble(textBox3.Text);
            double nv = Convert.ToDouble(textBox4.Text);
            double[,] Egsm = new double[oriimage1.Width, oriimage1.Height];
            double sumgr = 0;
            double sumegs = 0;
            double eva = 0;
            for (int i = 0; i < oriimage0.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    Egsm[i, j] = alpha * Eedge[i, j] + beta * Eterm[i, j] + nv*oriimage0.GetPixel(i,j).R;
                }
            }
            for (int i = 0; i < oriimage0.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (u[i, j] == 1)
                    {

                        sumgr = sumgr + 1;
                        sumegs = sumegs + Egsm[i, j];

                    }
                }
            }
            eva = lamda * sumegs / sumgr;
            for (int i = 1; i < oriimage0.Width - 1; i++)
            {
                for (int j = 1; j < oriimage1.Height - 1; j++)
                {
                    if (u[i, j] == 1 && Egsm[i, j] < eva && absGru[i, j] != 0)
                    {
                        u[i, j] = 0;
                    }
                }
            }
        }
        public void evolutionerode()
        {
            double alpha = 1;
            double beta = 1;
            double lamda = Convert.ToDouble(textBox3.Text);
            double nv = Convert.ToDouble(textBox4.Text);
            double[,] Egsm = new double[oriimage1.Width, oriimage1.Height];
            double sumgr = 0;
            double sumegs = 0;
            double eva = 0;
            for (int i = 0; i < oriimage0.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    Egsm[i, j] = alpha * Eedge[i, j] + beta * Eterm[i, j] + nv * oriimage0.GetPixel(i, j).R;
                }
            }
            for (int i = 0; i < oriimage0.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (u[i, j] == 0)
                    {

                        sumgr = sumgr + 1;
                        sumegs = sumegs + Egsm[i, j];
                        //if (Egsm[i, j] < Egsm[i, j + 1] || Egsm[i, j] < Egsm[i, j - 1] || Egsm[i, j] < Egsm[i + 1, j] || Egsm[i, j] < Egsm[i - 1, j])
                        //{
                        //    Egsm[i, j+1] = 1;
                        //}
                        //else
                        //{
                        //    Egsm[i, j] = 0;
                        //}

                        //if (Egsm[i, j] < Egsm[i + 1, j])
                        //{
                        //    if (u[i + 1, j] == 0)
                        //    {
                        //        u[i + 1, j] = 1;
                        //    }
                        //}
                        //if (Egsm[i, j] < Egsm[i - 1, j])
                        //{
                        //    if (u[i - 1, j] == 0)
                        //    {
                        //        u[i - 1, j] = 1;
                        //    }
                        //}
                        //if (Egsm[i, j] < Egsm[i, j + 1])
                        //{
                        //    if (u[i, j + 1] == 0)
                        //    {
                        //        u[i, j + 1] = 1;
                        //    }
                        //}
                        //if (Egsm[i, j] < Egsm[i, j - 1])
                        //{
                        //    if (u[i, j - 1] == 0)
                        //    {
                        //        u[i, j - 1] = 1;
                        //    }
                    }
                }
            }
            eva = lamda * sumegs / sumgr;
            for (int i = 0; i < oriimage0.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (u[i, j] == 0 && Egsm[i, j] < eva && absGru[i, j] != 0)
                    {
                        u[i, j] = 1;
                    }
                    //else
                    //{
                     //   if (u[i, j] == 1 && Egsm[i, j] > eva && absGru[i, j] != 0)
                     //   {
                    //        u[i, j] = 0;
                    //    }
                   // }
 
                }
            }
        }
    }
}
