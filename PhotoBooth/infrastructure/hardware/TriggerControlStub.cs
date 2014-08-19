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
using com.prodg.photobooth.common;

namespace com.prodg.photobooth.infrastructure.hardware
{
    public class TriggerControlStub : ITriggerControl
    {
        private readonly Timer autoTriggerTimer;

        #region ITriggerControl Members

        public string Id { get; private set; }

        public void Arm()
        {
            logger.LogDebug(String.Format(CultureInfo.InvariantCulture,
                                            "TriggerControlStub.{0}: Arm called", Id));

            if (triggerTimeout.HasValue)
            {
                autoTriggerTimer.Change(Timeout.Infinite, triggerTimeout.Value*1000);
            }
        }

        public void Release()
        {
            logger.LogDebug(String.Format(CultureInfo.InvariantCulture,
                                            "TriggerControlStub.{0}: Release called", Id));                
        }

        public int Lock(bool indicateLock)
        {
            logger.LogDebug(String.Format(CultureInfo.InvariantCulture,
                                            "TriggerControlStub.{0}: Lock called", Id));
            return 0;
        }

        public void Unlock(int lockId)
        {
            logger.LogDebug(String.Format(CultureInfo.InvariantCulture,
                                "TriggerControlStub.{0}: Unlock called", Id));                
        }

        public event EventHandler<TriggerControlEventArgs> Fired;

        #endregion

        #region IHardwareController Members

        public void Initialize()
        {
            logger.LogDebug(String.Format(CultureInfo.InvariantCulture,
                    "TriggerControlStub.{0}: Initialize called", Id));                
        }

        public void DeInitialize()
        {
            logger.LogDebug(String.Format(CultureInfo.InvariantCulture,
                    "TriggerControlStub.{0}: Deinitialize called", Id));                
        }

        #endregion

        private readonly ILogger logger;
        private readonly Nullable<int> triggerTimeout; 

        public TriggerControlStub(string id, Nullable<int> triggerTimeout, ILogger logger)
        {
            this.logger = logger;
            Id = id;
            this.triggerTimeout = triggerTimeout;

            autoTriggerTimer = new Timer(state => Fired.Invoke(this, new TriggerControlEventArgs(Id)), null,
                Timeout.Infinite, Timeout.Infinite);
        }
    }
}
