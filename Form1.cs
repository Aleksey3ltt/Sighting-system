using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace Sighting_system
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;    //calibration
            button3.Enabled = false;    //targeting
            label2.Enabled = false;     //select the region of interest with the mouse
        }

        ArduinoClass arduinoClass = new ArduinoClass();
        private FilterInfoCollection? CaptureDevices;
        private VideoCaptureDevice? videoSource;
        Bitmap? img;
        Bitmap? bitmapSample;
        private bool mouseDown = false;
        private bool trackerFinished = false;
        private bool button2WasClicked = false;
        private bool button3WasClicked = false;
        Point locationXY;
        Point locationX1Y1;
        Rectangle rectMouse;
        Rectangle rect;
        int frameCount = 0;
        int count = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            CaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in CaptureDevices)
            {
                comboBox1.Items.Add(Device.Name);
            }
            comboBox1.SelectedIndex = 0;
            videoSource = new VideoCaptureDevice();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            videoSource = new VideoCaptureDevice(CaptureDevices[comboBox1.SelectedIndex].MonikerString);
            videoSource.NewFrame += videoSource_NewFrame;
            videoSource.Start();
            button2.Enabled = true; //calibration
            arduinoClass.InitPort();
        }

        private void videoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            img = (Bitmap)eventArgs.Frame.Clone();
            if (rectMouse.Width > 0 & rectMouse.Height > 0)
            {
                var cloned = new Bitmap(img).Clone(rectMouse, img.PixelFormat);
                bitmapSample = new Bitmap(cloned, new Size(rectMouse.Width, rectMouse.Height));
                cloned.Dispose();
            }
            Image<Bgr, byte> source = img.ToImage<Bgr, byte>();
            CvInvoke.DrawMarker(source, new Point(640, 360), new MCvScalar(0, 255, 255), MarkerTypes.Cross, 64, 1);
            CvInvoke.Rectangle(source, new Rectangle(new Point(640 - 16, 360 - 16), new Size(32, 32)), new MCvScalar(0, 255, 255), 1);

            if (button3WasClicked == false)
                pictureBox1.Image = source.AsBitmap();
            else
            {
                var template = new Bitmap(pictureBox2.Image).ToImage<Bgr, byte>();

                //template
                double minValFirst = 0.0;
                double maxValFirst = 0.0;
                Point minLocFirst = new Point();
                Point maxLocFirst = new Point();

                if (frameCount == count)
                {
                    count += 1;
                    Mat imgOutFirst = new Mat();
                    CvInvoke.MatchTemplate(source, template, imgOutFirst, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);
                    CvInvoke.MinMaxLoc(imgOutFirst, ref minValFirst, ref maxValFirst, ref minLocFirst, ref maxLocFirst);
                    rect = new Rectangle(maxLocFirst, template.Size);
                    int centerX = rect.X + rect.Width / 2;
                    int centerY = rect.Y + rect.Height / 2;
                    string coordRect = "(" + centerX + ", " + centerY + ")";
                    string deltaCoord = "(" + (640 - centerX) + ", " + (360 - centerY) + ")";
                    CvInvoke.PutText(source, "Count:        " + Convert.ToString(count), new Point(10, 15), FontFace.HersheySimplex, 0.4, new MCvScalar(255, 0, 0));
                    CvInvoke.PutText(source, "Coord:        " + coordRect, new Point(10, 30), FontFace.HersheySimplex, 0.4, new MCvScalar(255, 0, 0));
                    CvInvoke.PutText(source, "Coord(delta): " + deltaCoord, new Point(10, 45), FontFace.HersheySimplex, 0.4, new MCvScalar(255, 0, 0));
                    CvInvoke.Rectangle(source, rect, new MCvScalar(0, 255, 0), 3);
                    CvInvoke.DrawMarker(source, new Point(centerX, centerY), new MCvScalar(0, 255, 0), MarkerTypes.Diamond);
                    pictureBox1.Image = source.AsBitmap();
                }
                frameCount++;
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (button2WasClicked == true)
            {
                mouseDown = true;
                locationXY = e.Location;
                trackerFinished = false;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseDown == true)
            {
                locationX1Y1 = e.Location;
                mouseDown = false;
            }
            pictureBox2.Image = bitmapSample;
            int coordrectMouseX = rectMouse.X + rectMouse.Width / 2;
            int coordrectMouseY = rectMouse.Y + rectMouse.Height / 2;
            label1.Text = "Coordinates: " + coordrectMouseX.ToString() + ", " + coordrectMouseY.ToString();
            trackerFinished = false;
            button3.Enabled = true;   //targeting
            label2.Enabled = false;   //select the region of interest with the mouse
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown == true)
            {
                locationX1Y1 = e.Location;
                Refresh();
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

            Pen pen = new Pen(Color.Red, 4);
            if (button3WasClicked == false & trackerFinished == false)
            {
                e.Graphics.DrawRectangle(pen, GetRect());
            }
        }

        private Rectangle GetRect()
        {
            rectMouse = new Rectangle();
            rectMouse.X = Math.Min(locationXY.X, locationX1Y1.X);
            rectMouse.Y = Math.Min(locationXY.Y, locationX1Y1.Y);
            rectMouse.Width = Math.Abs(locationXY.X - locationX1Y1.X);
            rectMouse.Height = Math.Abs(locationXY.Y - locationX1Y1.Y);
            return rectMouse;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            if (button2WasClicked == true)
            {
                arduinoClass.SetInitialAngles();
            }
            else
            {
                arduinoClass.SetCalibration();
            }
            button2.Text = "Setting the initial angles";
            button2.Enabled = true;  //calibration
            label2.Enabled = true;  //select the region of interest with the mouse
            button2WasClicked = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3WasClicked = true;
            int deltaX = 640 - (rectMouse.X + rectMouse.Width / 2);
            int deltaY = 360 - (rectMouse.Y + rectMouse.Height / 2);
            arduinoClass.Targeting(deltaX, deltaY);
            trackerFinished = true;
            button3.Enabled = false;   //targeting
            button3WasClicked = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            arduinoClass.ClosePort();

            if (videoSource != null)
            {
                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource = null;
                    pictureBox1.Image = null;
                }
            }
        }
    }
}