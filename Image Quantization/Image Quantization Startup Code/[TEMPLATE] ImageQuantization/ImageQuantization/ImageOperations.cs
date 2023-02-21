using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;
using Priority_Queue;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// 
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// 
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }
    public struct graph
    {
        public int src, des;
        public double weight;
    }
    //
    class point : FastPriorityQueueNode
    {
        public point(int vertix, int? parent)
        {
            V = vertix;
            P = parent;
        }
        public int V { get; set; }
        public int? P { get; set; }

    }
    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        public static Dictionary<string, TimeSpan> timeTable = new Dictionary<string, TimeSpan>();
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }
        public static List<RGBPixel> unique;

        public static RGBPixel[,] ImageQuantization(RGBPixel[,] InputImage, int ClusterNumber)
        {
            unique = uniqueList(InputImage);
            graph[] Prim = MST(unique);
            Dictionary<RGBPixel, RGBPixel> result = cluster(Prim, ClusterNumber, unique);
            RGBPixel[,] OutputImage = Palette(InputImage, result);
            return OutputImage;
        }

        // Convert the RGBPixel Array into a Hashset of int carries (red, green, blue) concatnated together. 
        public static List<int> RGBPixelsToInteger(RGBPixel[,] Image)
        {
            List<int> distinctColorsInt = new List<int>();
            foreach (RGBPixel pixel in Image)
            {
                int IntColor = pixel.blue << 16 | pixel.green << 8 | pixel.red;
                distinctColorsInt.Add(IntColor);
            }
            return distinctColorsInt;
        }
        // Get the unique colors in image by a list of RGBPixel struct.
        public static List<RGBPixel> uniqueList(RGBPixel[,] Image)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<RGBPixel> colourPalette = new List<RGBPixel>();
            HashSet<int> uni = new HashSet<int>(RGBPixelsToInteger(Image));
            foreach (int unique in uni)
            {
                RGBPixel pixel;
                pixel.red = (byte)(unique); // 0 from 7 bits in unique int
                pixel.green = (byte)(unique >> 8); // 8 from 15 bits in unique int
                pixel.blue = (byte)(unique >> 16); // 16 from 23 bits in unique int
                colourPalette.Add(pixel);
            }
            stopwatch.Stop();
            timeTable.Add("Distinct Colors List<RGBPixel>", stopwatch.Elapsed);
            return colourPalette;
        }

        // Edge Calculation Function ^/((red1 - red2)^2 + (greedn1 - green2)^2 + (blue1 - blue2)^2)
        public static double WeightCalc(RGBPixel first, RGBPixel second)
        {
            double red, green, blue, value;

            red = Math.Pow(first.red - second.red, 2);
            green = Math.Pow(first.green - second.green, 2);
            blue = Math.Pow(first.blue - second.blue, 2);
            value = Math.Sqrt(red + green + blue);

            return value;
        }

        // find the node that will be use next in the mst 
        /*public static int NodeFinder(construct[] decision)
        {
            double checker = double.MaxValue;
            int value = -1;
            for(int i = 0; i < decision.Length; i++)
            {
                if (decision[i].used == false && decision[i].weight < checker)
                {
                    checker = decision[i].weight;
                    value = i;
                }
            }
            return value;
        }*/

        public static int ParentFinder(int[] control, int nodeNumber)
        {
            if (control[nodeNumber] != nodeNumber)
                control[nodeNumber] = ParentFinder(control, control[nodeNumber]);

            return control[nodeNumber];
        }

        public static void RootControl(int[] control, int NodeNumber1, int NodeNumber2)
        {
            int node1 = ParentFinder(control, NodeNumber1);
            int node2 = ParentFinder(control, NodeNumber2);

            control[node2] = node1;
        }

        public static double MSTSUM = 0;
        public static int[] decision;

        // Minimum spanning tree algorithm using prims Method.
        public static graph[] MST(List<RGBPixel> unique)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            decision = new int[unique.Count];
            FastPriorityQueue<point> dec = new FastPriorityQueue<point>(unique.Count);
            point[] points = new point[unique.Count];
            graph[] MSTResult = new graph[unique.Count - 1];

            for (int i = 0; i < unique.Count; i++)
            {
                points[i] = new point(i, null);
                decision[i] = i;
                if (i == 0)
                    dec.Enqueue(points[i], 0);
                else
                    dec.Enqueue(points[i], int.MaxValue);
            }
            int itr = 0;
            double weight = 0;
            while (dec.Count > 0)
            {
                point node = dec.Dequeue();
                if (node.P != null)
                {
                    MSTSUM += node.Priority;
                    MSTResult[itr].weight = node.Priority;
                    MSTResult[itr].des = node.V;
                    MSTResult[itr].src = (int)node.P;
                    itr++;
                }
                foreach (point n in dec)
                {
                    weight = WeightCalc(unique[n.V], unique[node.V]);
                    if (weight < n.Priority)
                    {
                        n.P = node.V;
                        dec.UpdatePriority(n, (float)weight);
                    }
                }
            }


            /* for(int i = 0; i < unique.Count; i++)
             {
                 int node = NodeFinder(decision);
                 decision[node].used = true;
                 double weight = 0;
                 for(int j = 0; j < unique.Count; j++)
                 {
                     weight = WeightCalc(unique[node], unique[j]);
                     if(decision[j].used == false && decision[j].weight > weight)
                     {
                         decision[j].weight = weight;
                         decision[j].parent = node;
                     }
                 }
             }

             for (int i = 1; i < unique.Count; i++)
             {
                 MSTSUM += decision[i].weight;

                 MSTResult[i - 1].weight = decision[i].weight;
                 MSTResult[i - 1].des = i;
                 MSTResult[i - 1].src = decision[i].parent;
                 decision[i].parent = i;
             }*/

            stopwatch.Stop();
            timeTable.Add("MST Construction", stopwatch.Elapsed);
            return MSTResult;
        }

        public static int count = 0;
        public static Dictionary<RGBPixel, RGBPixel> cluster(graph[] Graph, int k, List<RGBPixel> unique)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<graph> sortedGraph = new List<graph>(Graph);
            sortedGraph.Sort((Graph1, Graph2) => Graph1.weight.CompareTo(Graph2.weight));
            List<int> parent = new List<int>();
            List<RGBPixel> mix = new List<RGBPixel>();
            List<RGBPixel>[] colours = new List<RGBPixel>[unique.Count + 1];
            Dictionary<RGBPixel, RGBPixel> results = new Dictionary<RGBPixel, RGBPixel>();
            while (k - 1 != 0)
            {
                sortedGraph.Remove(sortedGraph[sortedGraph.Count - 1]);
                k--;
            }
            foreach (var graph in sortedGraph)
            {
                int src = ParentFinder(decision, graph.src);
                int dst = ParentFinder(decision, graph.des);
                RootControl(decision, src, dst);
            }
            for (int x = 0; x < unique.Count; x++)
            {
                int par = ParentFinder(decision, x);
                if (!parent.Contains(par))
                {
                    parent.Add(par);
                    colours[par] = new List<RGBPixel>();
                }
                colours[par].Add(unique[x]);
            }
            for (int i = 0; i < parent.Count; i++)
            {
                mix.Add(colorMean(colours[parent[i]]));
                for (int j = 0; j < colours[parent[i]].Count; j++)
                    results.Add(colours[parent[i]][j], mix[i]);
            }
            stopwatch.Stop();
            timeTable.Add("Clustring Time", stopwatch.Elapsed);
            return results;
        }

        public static RGBPixel colorMean(List<RGBPixel> mix)
        {
            RGBPixelD sum;
            RGBPixel result;
            sum.red = 0;
            sum.green = 0;
            sum.blue = 0;
            foreach (RGBPixel color in mix)
            {
                sum.red += (double)color.red;
                sum.green += (double)color.green;
                sum.blue += (double)color.blue;
            }
            result.red = (byte)(sum.red / mix.Count);
            result.green = (byte)(sum.green / mix.Count);
            result.blue = (byte)(sum.blue / mix.Count);
            return result;
        }

        public static List<int> ConvertFromRgbPixelToInt(Dictionary<RGBPixel, RGBPixel> x, RGBPixel[,] pic)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            List<int> values = RGBPixelsToInteger(pic);
            List<int> ret = new List<int>();
            foreach (KeyValuePair<RGBPixel, RGBPixel> pixel in x)
            {
                int key = pixel.Key.blue << 16 | pixel.Key.green << 8 | pixel.Key.red;
                int value = pixel.Value.blue << 16 | pixel.Value.green << 8 | pixel.Value.red;
                result.Add(key, value);
            }
            foreach (int v in values)
            {
                ret.Add(result[v]);
            }
            return ret;
        }
        public static RGBPixel[,] Palette(RGBPixel[,] image, Dictionary<RGBPixel, RGBPixel> results)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int height = GetHeight(image);
            int width = GetWidth(image);
            List<int> pal = ConvertFromRgbPixelToInt(results, image);
            RGBPixel pix;
            int K = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    pix.red = (byte)(pal[K]);
                    pix.green = (byte)(pal[K] >> 8);
                    pix.blue = (byte)(pal[K] >> 16);
                    image[i, j] = pix;
                    K++;
                }
            }
            stopwatch.Stop();
            timeTable.Add("Image Reconstruction", stopwatch.Elapsed);
            return image;
        }
    }
}
