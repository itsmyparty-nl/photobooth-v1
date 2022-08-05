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

using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.infrastructure.hardware
{
    public class TriggerControlStub : ITriggerControl
    {
        private readonly Timer _autoTriggerTimer;

        #region ITriggerControl Members

        public string Id { get; }

        public void Arm()
        {
            _logger.LogDebug("TriggerControlStub.{Id}: Arm called", Id);

            if (_triggerTimeout.HasValue)
            {
                _autoTriggerTimer.Change(_triggerTimeout.Value * 1000, Timeout.Infinite);
            }
        }

        public void Release()
        {
            _logger.LogDebug("TriggerControlStub.{Id}: Release called", Id);
            if (_triggerTimeout.HasValue)
            {
                _autoTriggerTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public int Lock(bool indicateLock)
        {
            _logger.LogDebug("TriggerControlStub.{Id}: Lock called", Id);
            return 0;
        }

        public void Unlock(int lockId)
        {
            _logger.LogDebug("TriggerControlStub.{Id}: Unlock called", Id);
        }

        public event EventHandler<TriggerControlEventArgs> Fired;

        #endregion

        #region IHardwareController Members

        public void Initialize()
        {
            _logger.LogDebug("TriggerControlStub.{Id}: Initialize called", Id);
        }

        public void DeInitialize()
        {
            _logger.LogDebug("TriggerControlStub.{Id}: Deinitialize called", Id);                
        }

        #endregion

        private readonly ILogger<TriggerControlStub> _logger;
        private readonly int? _triggerTimeout; 

        public TriggerControlStub(string id, int? triggerTimeout, ILogger<TriggerControlStub> logger)
        {
            _logger = logger;
            Id = id;
            _triggerTimeout = triggerTimeout;
            logger.LogDebug("TriggerControlStub.{Id}: Created with timeout {TimeOut}s", Id, triggerTimeout);    

            _autoTriggerTimer = new Timer(AutoTriggerTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void AutoTriggerTimerCallback(object state)
        {
            if (Fired != null)
            {
                _logger.LogDebug("TriggerControlStub.{Id}: Triggered", Id);    
                Fired.Invoke(this, new TriggerControlEventArgs(Id));
            }
            _autoTriggerTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
