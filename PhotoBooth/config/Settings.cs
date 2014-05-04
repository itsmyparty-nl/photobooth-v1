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
using System.Collections.Specialized;
using System.Configuration;

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

        #endregion

        #region ISettings Members

        /// <summary>
        /// The name of the serial port connection to the hardware e.g. COM1 on Windows or /dev/tty0 on linux
        /// </summary>
        public string SerialPortName { get; private set; }

        /// <summary>
        /// The baud rate of the serial port connection to the hardware e.g. 9600
        /// </summary>
        public int SerialPortBaudRate { get; private set; }

        /// <summary>
        /// Enable / disable the Data Terminal Ready (DTR) signal of the serial port connection to the hardware
        /// </summary>
        /// <remarks>Data Terminal Ready (DTR) is typically enabled during XON/XOFF software handshaking and
        ///  Request to Send/Clear to Send (RTS/CTS) hardware handshaking, and modem communications.</remarks>
        public bool SerialPortDtrEnable { get; private set; }

        /// <summary>
        /// The base path for storing images and other related data
        /// </summary>
        public string StoragePath { get; private set; }

        /// <summary>
        /// The grid Width of the collage created by the collage image processer (default: 2)
        /// </summary>
        public int CollageGridWidth { get; private set; }

        /// <summary>
        /// The grid Height of the collage created by the collage image processer (default: 2)
        /// </summary>
        public int CollageGridHeight { get; private set; }

        /// <summary>
        /// The scaling percentage for input images to be used in the collageby the collage image processer (default: 0.25) 
        /// </summary>
        public float CollageScalePercentage { get; private set; }

        /// <summary>
        /// The number of pixels to pad outside of images in the collageby the collage image processer (default: 30) 
        /// </summary>
        public int CollagePaddingPixels { get; private set; }

        #endregion

        /// <summary>
        /// C'tor
        /// <para>
        /// Reads in all settings from the app config file of the current application
        /// </para>
        /// </summary>
        public Settings()
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
            }
            catch (ConfigurationErrorsException e)
            {
                throw new Exception("Application settings could not be read", e);
            }
        }
    }
}
