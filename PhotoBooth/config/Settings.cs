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
using System.IO;
using System.Collections.Specialized;
using System.Configuration;
using com.prodg.photobooth.common;

namespace com.prodg.photobooth.config
{
    /// <summary>
    /// The photobooth application settings
    /// <para>
    /// This implementation provides read only settings read from the app.config file of the application running the photobooth.
    /// Exceptions will only be thrown when accessing the file in the constructor, no exceptions are to be thrown when accessing
    /// the settings from the application.
    /// </para> 
    /// </summary>
    public class Settings: ISettings
    {
        #region Setting key definitions

        private const string SerialPortNameKey = "SerialPortName";
        private const string SerialPortBaudRateKey = "SerialPortBaudRate";
        private const string SerialPortDtrEnableKey = "SerialPortDtrEnable";
        private const string StoragePathKey = "StoragePath";
        private const string CollageGridWidthKey = "CollageGridWidth";
        private const string CollageGridHeightKey = "CollageGridHeight";
        private const string CollageScalePercentageKey = "CollageScalePercentage";
        private const string CollagePaddingPixelsKey = "CollagePaddingPixels";
        private const string CollageAspectRatioKey = "CollageAspectRatio";
		private const string PrinterNameKey = "PrinterName";
        private const string PrintMarginTopKey = "PrintMarginTop";
        private const string PrintMarginLeftKey = "PrintMarginLeft";
        private const string PrintMarginBottomKey = "PrintMarginBottom";
        private const string PrintMarginRightKey = "PrintMarginRight";

        #endregion

        #region ISettings Members

        public string SerialPortName { get; private set; }
        public int SerialPortBaudRate { get; private set; }
        public bool SerialPortDtrEnable { get; private set; }
        public string StoragePath { get; private set; }
        public int CollageGridWidth { get; private set; }
        public int CollageGridHeight { get; private set; }
        public float CollageScalePercentage { get; private set; }
        public int CollagePaddingPixels { get; private set; }
        public double CollageAspectRatio { get; private set; }
		public string PrinterName { get; private set; }
        public int PrintMarginTop { get; private set; }
        public int PrintMarginLeft { get; private set; }
        public int PrintMarginBottom { get; private set; }
        public int PrintMarginRight { get; private set; }

        #endregion

        /// <summary>
        /// C'tor
        /// <para>
        /// Reads in all settings from the app config file of the current application
        /// </para>
        /// </summary>
		public Settings(ILogger logger)
        {
            try
            {
                // Get the AppSettings section.
                NameValueCollection appSettings =
                   ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    throw new Exception("Application settings are empty or could noy be found");
                }

                //Read in all settings
                SerialPortName = appSettings.Get(SerialPortNameKey);
                SerialPortBaudRate = Convert.ToInt32(appSettings.Get(SerialPortBaudRateKey));
                SerialPortDtrEnable = Convert.ToBoolean(appSettings.Get(SerialPortDtrEnableKey));
                StoragePath = appSettings.Get(StoragePathKey);
                CollageGridWidth = Convert.ToInt32(appSettings.Get(CollageGridWidthKey));
                CollageGridHeight = Convert.ToInt32(appSettings.Get(CollageGridHeightKey));
                CollageScalePercentage = Convert.ToSingle(appSettings.Get(CollageScalePercentageKey));
                CollagePaddingPixels = Convert.ToInt32(appSettings.Get(CollagePaddingPixelsKey));
                CollageAspectRatio = Convert.ToDouble(appSettings.Get(CollageAspectRatioKey));
				PrinterName = appSettings.Get(PrinterNameKey);
                PrintMarginTop = Convert.ToInt32(appSettings.Get(PrintMarginTopKey));
                PrintMarginLeft = Convert.ToInt32(appSettings.Get(PrintMarginLeftKey));
                PrintMarginBottom = Convert.ToInt32(appSettings.Get(PrintMarginBottomKey));
                PrintMarginRight = Convert.ToInt32(appSettings.Get(PrintMarginRightKey));

				try
				{
				if (!Directory.Exists(StoragePath))
				{
						Directory.CreateDirectory(StoragePath);
				}
				}
				catch (Exception ex)
				{
					logger.LogException(string.Format(@"Error while creating directory {0}",StoragePath),ex);
				}

            }
            catch (ConfigurationErrorsException e)
            {
                throw new Exception("Application settings could not be read", e);
            }
        }
    }
}
