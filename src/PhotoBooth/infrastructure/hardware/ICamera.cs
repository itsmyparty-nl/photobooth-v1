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

namespace com.prodg.photobooth.infrastructure.hardware
{
    public class CameraBatteryWarningEventArgs : EventArgs
    {
        public CameraBatteryWarningEventArgs(int level)
        {
            Level = level;
        }

        public int Level { get; }
    }

    public class CameraStateChangedEventArgs : EventArgs
    {
        public CameraStateChangedEventArgs(bool newState)
        {
            NewState = newState;
        }

        public bool NewState { get; private set; }
    }

    public interface ICamera: IHardwareController
    {
        bool IsReady { get;  }
	    
	    /// <summary>
        /// Emits an event when the camera state changed (from ready to connection lost and vice versa)
        /// </summary>
        event EventHandler<CameraStateChangedEventArgs> StateChanged;
   
        /// <summary>
        /// Emits a warning if the battery level is at or below the warning level
        /// </summary>
        event EventHandler<CameraBatteryWarningEventArgs> BatteryWarning;
        
        /// <summary>
		/// The identifier (model) of the camera
		/// </summary>
		string Id {get;}
		
		/// <summary>
		/// Capture a single shot
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/> to indicate whether the capture succeeded 
		/// </returns>
		/// <remarks>In case the shot did not succeed, a re-initialize of
		/// the camera is forced</remarks>
		bool Capture(string capturePath);
		
		/// <summary>
		/// Clean any data on the camera
		/// </summary>
		void Clean();
	}
}
