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
  
  Copyright 2015 Patrick Bronneberg
*/
#endregion

using System;
using System.IO;
using com.prodg.photobooth.common;
using com.prodg.photobooth.config;
using com.prodg.photobooth.domain;
using com.prodg.photobooth.domain.image;
using NUnit.Framework;
using Rhino.Mocks;

namespace Test_Photobooth.domain
{
    [TestFixture]
    public class TestImageProcessor
    {

        [TestCase(FilterType.Brownie)]
        [TestCase(FilterType.VintagePinhole)]
        [TestCase(FilterType.Technicolor)]
        [TestCase(FilterType.Sepia)]
        [TestCase(FilterType.Original)]
        [TestCase(FilterType.Kodachrome)]
        [TestCase(FilterType.DesaturateLuminance)]
        [TestCase(FilterType.AlternativePolaroid)]
        [TestCase(FilterType.DarkGrayscale)]
        [TestCase(FilterType.FrozenBlue)]
        [TestCase(FilterType.FrozenIce)]
        public void TestProcessImages2x2(FilterType filterType)
        {
            ConsoleLogger logger = new ConsoleLogger();
            ISettings settings = MockRepository.GenerateStub<ISettings>();

            settings.Stub(s => s.CollageGridWidth).Return(2);
            settings.Stub(s => s.CollageGridHeight).Return(2);
            settings.Stub(s => s.CollageScalePercentage).Return(1);
            settings.Stub(s => s.CollagePaddingPixels).Return(20);
            settings.Stub(s => s.CollageAspectRatio).Return(1.5);
            settings.Stub(s => s.Filter).Return(filterType);

            var imageProcessingChain = new ImageProcessingChain(logger, settings);

            string baseFolder = @"C:\Users\nly96192\Downloads\HHDxSoftwareInloopdag\";
            //string baseFolder = @"C:\Users\nly96192\Desktop\Test";
            ProcessImages(imageProcessingChain, baseFolder);
        }

        [Test]
        public void TestProcessImages3x3()
        {
            ConsoleLogger logger = new ConsoleLogger();
            ISettings settings = MockRepository.GenerateStub<ISettings>();

            settings.Stub(s => s.CollageGridWidth).Return(3);
            settings.Stub(s => s.CollageGridHeight).Return(3);
            settings.Stub(s => s.CollageScalePercentage).Return(1);
            settings.Stub(s => s.CollagePaddingPixels).Return(20);
            settings.Stub(s => s.CollageAspectRatio).Return(1.5);

            var imageProcessor = new CollageImageProcessor(logger, settings);

            string baseFolder = @"C:\temp\3x3";
            ProcessImages(imageProcessor, baseFolder);
        }

        [Test]
        public void TestProcessImages4x4()
        {
            ConsoleLogger logger = new ConsoleLogger();
            ISettings settings = MockRepository.GenerateStub<ISettings>();

            settings.Stub(s => s.CollageGridWidth).Return(4);
            settings.Stub(s => s.CollageGridHeight).Return(4);
            settings.Stub(s => s.CollageScalePercentage).Return(0.5f);
            settings.Stub(s => s.CollagePaddingPixels).Return(5);
            settings.Stub(s => s.CollageAspectRatio).Return(1.5);

            var imageProcessor = new CollageImageProcessor(logger, settings);

            string baseFolder = @"C:\temp\4x4";
            ProcessImages(imageProcessor, baseFolder);
        }

        private static void ProcessImages(IMultiImageProcessor imageProcessor, string baseFolder)
        {
            foreach (String imageFolder in Directory.GetDirectories(baseFolder))
            {
                string id = imageFolder.Replace(Path.GetDirectoryName(imageFolder) + "\\", "");
                try
                {
                    using (
                        PhotoSession session = new PhotoSession("Test",
                            0, imageFolder))
                    {
                        for (int i = 1; i <= imageProcessor.RequiredImages; i++)
                        {
                            session.AddPicture(Path.Combine(imageFolder, i + ".jpg"));
                        }
                        imageProcessor.Process(session);
                    }
                }
                catch (Exception)
                {
                    // do nothing
                }
            }
        }
    }
}
