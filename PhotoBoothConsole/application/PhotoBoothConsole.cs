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

using System;
using System.Globalization;
using System.Threading;
using com.prodg.photobooth.domain;
using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.application
{
	/// <summary>
	/// Simple console application to run the photobooth without a display connected to it
	/// </summary>
    public static class PhotoBoothConsole
	{
        public static void Main (string[] args)
        {
            using (var photoBooth = new PhotoBooth())
            {
                photoBooth.Hardware.Camera.BatteryWarning += OnCameraBatteryWarning;
                //Wait for a shutdown trigger
                photoBooth.ShutdownRequested.WaitOne();
                photoBooth.Hardware.Camera.BatteryWarning -= OnCameraBatteryWarning;
            }
        }

        static void OnCameraBatteryWarning(object sender, CameraBatteryWarningEventArgs e)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture,"WARNING BATTERY LOW: {0}%",e.Level));
        }
	}
}
