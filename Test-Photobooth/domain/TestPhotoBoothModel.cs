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

using com.prodg.photobooth.common;
using com.prodg.photobooth.infrastructure.hardware;
using NUnit.Framework;
using Rhino.Mocks;

namespace com.prodg.photobooth.domain
{
    [TestFixture]
    public class TestPhotoBoothModel
    {
        private ILogger logger;
        private IPhotoBoothService service;
        private IHardware hardware;
        private PhotoBoothModel model;

        private ITriggerControl triggerButton;
        private ITriggerControl printButton;
        private ITriggerControl printTwiceButton;
        private ITriggerControl powerButton;
        private ICamera camera;
        private IPrinter printer;

        [SetUp]
        public void Setup()
        {
            service = MockRepository.GenerateStub<IPhotoBoothService>();
            logger = MockRepository.GenerateStub<ILogger>();
            hardware = MockRepository.GenerateStub<IHardware>();

            camera = MockRepository.GenerateStub<ICamera>();
            printer = MockRepository.GenerateStub<IPrinter>();
            triggerButton = MockRepository.GenerateStub<ITriggerControl>();
            printButton = MockRepository.GenerateStub<ITriggerControl>();
            printTwiceButton = MockRepository.GenerateStub<ITriggerControl>();
            powerButton = MockRepository.GenerateStub<ITriggerControl>();

            hardware = new Hardware(camera, printer, triggerButton, printButton, printTwiceButton, powerButton, logger);

            model = new PhotoBoothModel(service, hardware, logger);
        }

        [Test]
        public void TestStartCausesCorrectHardwarePrepare()
        {
            model.Start();

            triggerButton.AssertWasCalled((ctrl) => ctrl.Arm());
            printButton.AssertWasCalled((ctrl) => ctrl.Release());
            printTwiceButton.AssertWasCalled((ctrl) => ctrl.Release());
            powerButton.AssertWasCalled((ctrl) => ctrl.Arm());
        }

    }
}
