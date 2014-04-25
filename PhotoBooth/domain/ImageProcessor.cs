#region PhotoBooth - MIT - (c) 2014 Patrick Bronneberg
/*
  PhotoBooth - an application to control a DIY photobooth

  Permission is hereby granted, free of charge, to any person obtaining
  a copy of this software and associated documentation files (the
  "Software"), to deal in the Software without restriction, including
  without limitation the rights to use, copy, modify, merge, publish,
  distribute, sublicense, and/or sell copies of the Software, and to
  permit persons to whom the Software is furnished to do so, subject to
  the following conditions:

  The above copyright notice and this permission notice shall be
  included in all copies or substantial portions of the Software.
  
  Copyright 2014 Patrick Bronneberg
*/
#endregion

using System.Collections.Generic;
using com.prodg.photobooth.common;
using System.Drawing;
using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// An image processor processes a collection of images into a single image
    /// </summary>
    public class ImageProcessor : IImageProcessor
    {
        private readonly ILogger logger;
        private readonly Color backgroundColor = Color.White;
        private const int GridWidth = 2;
        private const int GridHeight = 2;
        private const float ScalePercentage = 0.25f;
        private const int PaddingPx = 30;
        private const string Extension = ".jpg";

        private readonly ColorMatrix colorMatrix;
        private readonly ImageAttributes attributes;

        

        public ImageProcessor(ILogger logger)
        {
            this.logger = logger;
            //create some image attributes
            attributes = new ImageAttributes();

            //create the grayscale ColorMatrix
            colorMatrix = new ColorMatrix(
                new float[][] 
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

        }

        /// <summary>
        /// Processes the images into a single image
        /// </summary>
        /// <param name="id">The identifier of the image</param>
        /// <param name="images">
        /// A <see cref="IList<System.String>" />
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/> containing the path of the result image
        /// </returns>
        public string Process(string id, string storagePath, IList<string> images)
        {
            logger.LogInfo(string.Format("Processing {0}: {1} images", id, images.Count));
            if (images.Count != (GridWidth * GridHeight))
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Expected exactly {0} images to process", GridWidth * GridHeight));
            }

            return CombineBitmapsGreyScale(id, storagePath, images);
        }

        private string CombineBitmapsGreyScale(string id, string storagePath, IList<string> files)
        {
            //read all images into memory
            var images = new List<Image>();

            try
            {
                int width = 0;
                int height = 0;
                int i = 0;
                foreach (string imagePath in files)
                {
                    //create a Bitmap from the file and add it to the list
                    Image currentImage = new Bitmap(imagePath);
                    if (i == 0)
                    {
                        //Gridwidth - 1 to get the padding in between. +2 to get borders outside the image
                        width = (int)((currentImage.Width * GridWidth) * ScalePercentage + (GridWidth + 1) * PaddingPx);
                        height = (int)((currentImage.Height * GridHeight) * ScalePercentage + (GridHeight + 1) * PaddingPx);
                    }
                    //images.Add(Resize(currentImage));
                    images.Add(currentImage);

                    i++;
                }


                //create a bitmap to hold the combined image
                using (Image finalImage = new Bitmap(width, height))
                {
                    //get a graphics object from the image so we can draw on it
                    using (Graphics g = System.Drawing.Graphics.FromImage(finalImage))
                    {
                        //set background color
						g.Clear(backgroundColor);

                        //go through each image and draw it on the final image

                        int j = 0;
                        foreach (var currentImage in images)
                        {
                            int currentWidth = (int)Math.Floor(currentImage.Width * ScalePercentage);
                            int currentHeight = (int)Math.Floor(currentImage.Height * ScalePercentage);

                            int startX = (j % GridWidth) * (currentWidth + PaddingPx) + PaddingPx;
                            int startY = (j / GridWidth) * (currentHeight + PaddingPx) + PaddingPx;

                            //draw the original image on the new image using the grayscale color matrix
                            g.DrawImage(currentImage, new Rectangle(startX, startY, currentWidth, currentHeight),
                               0, 0, currentImage.Width, currentImage.Height, GraphicsUnit.Pixel, attributes);

                            j++;
                        }
                    }

                    string filename = Path.Combine(storagePath,id+"_montage_"
                        + Extension);
                    finalImage.Save(filename, ImageFormat.Jpeg);
                    return filename;
                }
            }
            finally
            {
                //clean up memory
                foreach (System.Drawing.Bitmap image in images)
                {
                    image.Dispose();
                }
                images.Clear();
            }
        }
    }
}
