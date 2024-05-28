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


using com.prodg.photobooth.config;
using com.prodg.photobooth.domain;
using com.prodg.photobooth.domain.image;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SixLabors.ImageSharp;

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
        [TestCase(FilterType.Polaroid)]
        [TestCase(FilterType.AlternativePolaroid)]
        [TestCase(FilterType.DarkGrayscale)]
        [TestCase(FilterType.Grayscale)]
        public void TestProcessImagesChain(FilterType filterType)
        {
            Mock<ILoggerFactory> loggerFactoryStub = new MockRepository(MockBehavior.Loose).Create<ILoggerFactory>();
            Mock<ILogger<ImageProcessingChain>> loggerStub = new MockRepository(MockBehavior.Loose).Create<ILogger<ImageProcessingChain>>();
            Mock<ISettings> settingsStub = new MockRepository(MockBehavior.Loose).Create<ISettings>();

            settingsStub.SetupGet(s => s.CollageGridWidth).Returns(2);
            settingsStub.SetupGet(s => s.CollageGridHeight).Returns(2);
            settingsStub.SetupGet(s => s.CollageScalePercentage).Returns(1.0f);
            settingsStub.SetupGet(s => s.CollagePaddingPixels).Returns(20);
            settingsStub.SetupGet(s => s.CollageAspectRatio).Returns(1.5);
            settingsStub.SetupGet(s => s.Filter).Returns(filterType);
        
            var imageProcessingChain = new ImageProcessingChain(loggerFactoryStub.Object,loggerStub.Object, settingsStub.Object);
        
            var session = LoadPhotoSession(@"resources/testImages", imageProcessingChain.RequiredImages);
            
            //Act
            var image = imageProcessingChain.Process(session);
            Assert.That(image, Is.Not.Null);
            Assert.That(image!.Height, Is.GreaterThan(100));
            Assert.That(image!.Width, Is.GreaterThan(100));
            image.SaveAsJpeg($"out-{filterType}.jpg");
        }
        
        [TestCase("halloween")]
        [TestCase("halloween-small")]
        public void TestProcessImagesOverlay(string overlayPath)
        {
            Mock<ILoggerFactory> loggerFactoryStub = new MockRepository(MockBehavior.Loose).Create<ILoggerFactory>();
            Mock<ILogger<ImageProcessingChain>> loggerStub = new MockRepository(MockBehavior.Loose).Create<ILogger<ImageProcessingChain>>();
            Mock<ISettings> settingsStub = new MockRepository(MockBehavior.Loose).Create<ISettings>();

            var overlayFullPath = Path.Combine($"resources", "overlay", $"{overlayPath}.png");
            settingsStub.SetupGet(s => s.CollageGridWidth).Returns(2);
            settingsStub.SetupGet(s => s.CollageGridHeight).Returns(2);
            settingsStub.SetupGet(s => s.CollageScalePercentage).Returns(1.0f);
            settingsStub.SetupGet(s => s.CollagePaddingPixels).Returns(20);
            settingsStub.SetupGet(s => s.CollageAspectRatio).Returns(1.32);
            settingsStub.SetupGet(s => s.OverlayImageFilename).Returns(overlayFullPath);
        
            var imageProcessingChain = new ImageProcessingChain(loggerFactoryStub.Object,loggerStub.Object, settingsStub.Object);
        
            var session = LoadPhotoSession(@"resources/testImages", imageProcessingChain.RequiredImages);
            
            //Act
            var image = imageProcessingChain.Process(session);
            Assert.That(image, Is.Not.Null);
            Assert.That(image!.Height, Is.GreaterThan(100));
            Assert.That(image!.Width, Is.GreaterThan(100));
            image.SaveAsJpeg($"out-overlay-{overlayPath}.jpg");
        }

        [Test]
        public void TestProcessImagesCollage()
        {
            //Arrange
            Mock<ILogger<CollageImageProcessor>> loggerStub = new MockRepository(MockBehavior.Loose).Create<ILogger<CollageImageProcessor>>();
            Mock<ISettings> settingsStub = new MockRepository(MockBehavior.Loose).Create<ISettings>();

            settingsStub.SetupGet(s => s.CollageGridWidth).Returns(2);
            settingsStub.SetupGet(s => s.CollageGridHeight).Returns(2);
            settingsStub.SetupGet(s => s.CollageScalePercentage).Returns(1.0f);
            settingsStub.SetupGet(s => s.CollagePaddingPixels).Returns(15);
            settingsStub.SetupGet(s => s.CollageAspectRatio).Returns(1.32f);
            
            var imageProcessor = new CollageImageProcessor(loggerStub.Object, settingsStub.Object);
            var session = LoadPhotoSession(@"resources/testImages", imageProcessor.RequiredImages);
            
            //Act
            var image = imageProcessor.Process(session);
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Height, Is.GreaterThan(100));
            Assert.That(image.Width, Is.GreaterThan(100));
            image.SaveAsJpeg("test-collage-out.jpg");
        }

        private static PhotoSession LoadPhotoSession(string imageFolder, int requiredImages)
        {
            var session = new PhotoSession("Test", 0, imageFolder);
            for (var i = 1; i <= requiredImages; i++)
            {
                session.AddPicture(Path.Combine(imageFolder, i + ".jpg"));
            }
            return session;
        }
    }
}
