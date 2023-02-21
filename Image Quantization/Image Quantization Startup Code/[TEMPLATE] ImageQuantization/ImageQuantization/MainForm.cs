using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        RGBPixel[,] OutputImage;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void NumberOfClusters_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int cluster = Convert.ToInt16(NumberOfClusters.Text);
            OutputImage = ImageOperations.ImageQuantization(ImageMatrix, cluster);
            ImageOperations.DisplayImage(OutputImage, pictureBox3);

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;

            MSTBox.Text = ImageOperations.MSTSUM.ToString();
            colorBox.Text = ImageOperations.unique.Count.ToString();

            string timeMessage = "";

            foreach (KeyValuePair<string, TimeSpan> time in ImageOperations.timeTable)
            {
                timeMessage += $"{time.Key} is: {time.Value.Hours}:{time.Value.Minutes}:{time.Value.Seconds}.{time.Value.Milliseconds}\n";
            }

            string message = timeMessage + $"Elapsed Time is: {ts.Hours}:{ts.Minutes}:{ts.Seconds}.{ts.Milliseconds}\n";
            MessageBox.Show(message);

            ImageOperations.MSTSUM = 0;
            ImageOperations.timeTable.Clear();
            ImageOperations.unique.Clear();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}