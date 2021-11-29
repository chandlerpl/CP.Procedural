﻿using CP.Procedural.Noise;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WorldGenerator.VisualTests.Pages
{
    /// <summary>
    /// Interaction logic for SimplexNoisePage.xaml
    /// </summary>
    public partial class SimplexNoisePage : Page
    {
        List<PageValues> items;

        public SimplexNoisePage()
        {
            InitializeComponent();
            items = new List<PageValues>();
            items.Add(new PageValues<uint>() { Name = "Seed", Value = 4354758 });
            items.Add(new PageValues<float>() { Name = "Scale", Value = 0.005f });
            items.Add(new PageValues<float>() { Name = "Persistance", Value = 0.5f });
            items.Add(new PageValues<int>() { Name = "Octaves", Value = 4 });
            VisualElement.ItemSources = items;
        }

        private void VisualElement_GenerateClicked(object sender, EventArgs e)
        {
            Generate();
        }

        private async Task Generate()
        {
            SimplexNoise noise = new SimplexNoise((uint)items.Find(r => r.Name == "Seed").Value, (float)items.Find(r => r.Name == "Scale").Value, (float)items.Find(r => r.Name == "Persistance").Value);

            Bitmap src = new Bitmap((int)VisualElement.ResultImage.Width, (int)VisualElement.ResultImage.Height);

            int total = (int)VisualElement.ResultImage.Height * (int)VisualElement.ResultImage.Width;
            float[][] input = new float[total][];
            for (int y = 0; y < VisualElement.ResultImage.Height; y++)
            {
                int yPos = (int)VisualElement.ResultImage.Width * y;
                for (int x = 0; x < VisualElement.ResultImage.Width; x++)
                {
                    input[yPos + x] = new float[3];
                    input[yPos + x][0] = x;
                    input[yPos + x][1] = y;
                    input[yPos + x][2] = 0;
                }
            }

            float[] vals = await noise.NoiseMap((int)items.Find(r => r.Name == "Octaves").Value, FractalType.FBM, input);

            //float[,] vals = await noise.NoiseMap((int)items.Find(r => r.Name == "Octaves").Value, FractalType.FBM, (int)VisualElement.ResultImage.Width, (int)VisualElement.ResultImage.Height, 0);

            for (int y = 0; y < VisualElement.ResultImage.Height; y++)
            {
                int yPos = (int)VisualElement.ResultImage.Width * y;
                for (int x = 0; x < VisualElement.ResultImage.Width; x++)
                {
                    //int val = (int)((vals[y, x] + 1) * 127.5);
                    int val = (int)((vals[yPos + x] + 1) * 127.5);
                    if (val > 255) val = 225;
                    else if (val < 0) val = 0;
                    src.SetPixel(x, y, System.Drawing.Color.FromArgb(val, val, val));
                }
            }
            VisualElement.Image = Utilities.BitmapToImageSource(src);
        }
    }
}
