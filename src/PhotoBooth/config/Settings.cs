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

using System.Configuration;
using System.Globalization;
using com.prodg.photobooth.domain.image;
using Microsoft.Extensions.Configuration;

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
    public class Settings : ISettings
    {
        #region Setting key definitions

        private const string PhotoBoothSection = "PhotoBoothSettings";
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
        private const string SaveSessionsKey = "SaveSessions";
        private const string OffloadSessionsKey = "OffloadSessions";
        private const string OffloadAddressKey = "OffloadAddress";
        private const string TriggerDelayMsKey = "TriggerDelayMs";
        private const string PrintDurationMsKey = "PrintDurationMs";
        private const string EventIdKey = "EventId";
        private const string FilterKey = "Filter";
        private const string FixedImageFilenameKey = "FixedImageFilename";
        private const string OverlayImageFilenameKey = "OverlayImageFilename";
        private const string StubCameraKey = "StubCamera";
        private const string ApiEventIdKey = "ApiEventId";
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
        public FilterType Filter { get; private set; }
        public string PrinterName { get; private set; }
        public int PrintMarginTop { get; private set; }
        public int PrintMarginLeft { get; private set; }
        public int PrintMarginBottom { get; private set; }
        public int PrintMarginRight { get; private set; }
        public int PrintDurationMs { get; private set; }
        public bool SaveSessions { get; private set; }
        public bool OffloadSessions { get; private set; }
        public string OffloadAddress { get; private set; }
        public int TriggerDelayMs { get; private set; }
        public string EventId { get; private set; }
        public string FixedImageFilename { get; private set; }
        public string OverlayImageFilename { get; private set; }
        public string ApiEventId { get; private set; }
        
        public bool StubCamera { get; private set; }
        
        #endregion

        /// <summary>
        /// C'tor
        /// <para>
        /// Reads in all settings from the app config file of the current application
        /// </para>
        /// </summary>
        public Settings(IConfiguration configuration)
        {
            try
            {
                var settings = configuration.GetSection(PhotoBoothSection);

                if (!settings.Exists())
                {
                    throw new Exception(
                        $"Application settings section '{PhotoBoothSection}' is empty or could noy be found");
                }

                //Read in all settings
                SerialPortName = settings[SerialPortNameKey]!;
                SerialPortBaudRate = Convert.ToInt32(settings[SerialPortBaudRateKey]!);
                SerialPortDtrEnable = Convert.ToBoolean(settings[SerialPortDtrEnableKey]!);
                StoragePath = settings[StoragePathKey]!;
                CollageGridWidth = Convert.ToInt32(settings[CollageGridWidthKey]!);
                CollageGridHeight = Convert.ToInt32(settings[CollageGridHeightKey]!);
                CollageScalePercentage = Convert.ToSingle(settings[CollageScalePercentageKey]!, CultureInfo.InvariantCulture);
                CollagePaddingPixels = Convert.ToInt32(settings[CollagePaddingPixelsKey]!);
                CollageAspectRatio = Convert.ToDouble(settings[CollageAspectRatioKey]!, CultureInfo.InvariantCulture);
                Filter = (FilterType)Enum.Parse(typeof(FilterType), settings[FilterKey]!);
                PrinterName = settings[PrinterNameKey]!;
                PrintMarginTop = Convert.ToInt32(settings[PrintMarginTopKey]!);
                PrintMarginLeft = Convert.ToInt32(settings[PrintMarginLeftKey]!);
                PrintMarginBottom = Convert.ToInt32(settings[PrintMarginBottomKey]!);
                PrintMarginRight = Convert.ToInt32(settings[PrintMarginRightKey]!);
                SaveSessions = Convert.ToBoolean(settings[SaveSessionsKey]!);
                OffloadSessions = Convert.ToBoolean(settings[OffloadSessionsKey]!);
                OffloadAddress = settings[OffloadAddressKey]!;
                TriggerDelayMs = Convert.ToInt32(settings[TriggerDelayMsKey]!);
                PrintDurationMs = Convert.ToInt32(settings[PrintDurationMsKey]!);
                EventId = settings[EventIdKey]!;
                FixedImageFilename = settings[FixedImageFilenameKey]!;
                OverlayImageFilename = settings[OverlayImageFilenameKey]!;
                ApiEventId = settings[ApiEventIdKey]!;
                StubCamera = settings[StubCameraKey] != null;

                // Check consistency
                if (!SaveSessions && OffloadSessions)
                {
                    throw new ConfigurationErrorsException("Save sessions must be enabled when offloading is enabled");
                }

                if (!Directory.Exists(StoragePath))
                {
                    Directory.CreateDirectory(StoragePath!);
                }
            }
            catch (ConfigurationErrorsException e)
            {
                throw new Exception("Application settings could not be read", e);
            }
        }
    }
}
