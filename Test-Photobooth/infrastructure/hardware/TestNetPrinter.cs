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

using System.Drawing;
using System.Drawing.Imaging;
using com.prodg.photobooth.common;
using com.prodg.photobooth.config;
using NUnit.Framework;

namespace com.prodg.photobooth.infrastructure.hardware
{
    [TestFixture]
    public class TestNetPrinter: NetPrinter
    {
        private static readonly ILogger logger = new ConsoleLogger();
        private static readonly ISettings settings = null;
        
        public TestNetPrinter(): 
            base(settings, logger)
        { }

        [Test]
        public void TestScaleToPageX()
        {
            var image = new Bitmap(800,600);
            FillWithColor(image, Brushes.Black);
            var page = new Bitmap(1281, 720);
            FillWithColor(page, Brushes.White);
            page.SetResolution(300,300);
            //ScaleAndCenterToPage(Graphics.FromImage(page), new Rectangle(0,0,page.Width/3,page.Height/3), image);

            page.Save(@"c:\temp\testx.png", ImageFormat.Png);
        }

        [Test]
        public void TestScaleToPageY()
        {
            var page = new Bitmap(800, 600);
            var image = new Bitmap(1281, 720);
            FillWithColor(image, Brushes.Black);
            FillWithColor(page, Brushes.White);
            page.SetResolution(300, 300);
            
            //ScaleAndCenterToPage(Graphics.FromImage(page), new Rectangle(0, 0, page.Width / 3, page.Height / 3), image);

            page.Save(@"c:\temp\testy.png", ImageFormat.Png);
        }


        private void FillWithColor(Image image, Brush brush)
        {
            using (Graphics grp = Graphics.FromImage(image))
            {
                grp.FillRectangle(brush, 0, 0, image.Width, image.Height);
            }
        }
    }
}
