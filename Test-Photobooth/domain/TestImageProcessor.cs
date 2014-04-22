﻿#region PhotoBooth - MIT - (c) 2014 Patrick Bronneberg
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.prodg.photobooth.common;
using com.prodg.photobooth.domain;
using NUnit.Framework;

namespace Test_Photobooth.domain
{
    [TestFixture]
    public class TestImageProcessor
    {
        private ImageProcessor imageProcessor;
        private IList<string> images;
        [SetUp]
        public void Setup()
        {
            ConsoleLogger logger = new ConsoleLogger();
            imageProcessor = new ImageProcessor(logger);
            images = new List<string> { @"C:\Data\Projects\PhotoBooth\Test-Photobooth\bin\Debug\resources\img1.JPG", @"C:\Data\Projects\PhotoBooth\Test-Photobooth\bin\Debug\resources\img2.JPG", @"C:\Data\Projects\PhotoBooth\Test-Photobooth\bin\Debug\resources\img3.JPG", @"C:\Data\Projects\PhotoBooth\Test-Photobooth\bin\Debug\resources\img4.JPG" };
        }

        [TearDown]
        public void Teardown()
        {
            imageProcessor = null;
        }

        [Test]
        public void TestProcessImages()
        {
            imageProcessor.Process("test",@"c:\temp", images);
        }
    }
}