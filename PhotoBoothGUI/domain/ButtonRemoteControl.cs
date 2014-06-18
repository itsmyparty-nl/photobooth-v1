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
using Gtk;
using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.domain
{
	public class ButtonRemoteControl: ITriggerControl
	{
		private Button button;

		public ButtonRemoteControl (string id, Button button)
		{
			Id = id;
			this.button = button;
			button.Clicked += OnButtonClicked;
			button.Visible = false;
		}

		void OnButtonClicked (object sender, EventArgs e)
		{
			Fired.Invoke(this, new TriggerControlEventArgs(Id));
		}

		public string Id { get; private set;}

        #region ITriggerControl Members


        public void Arm()
        {
            button.Visible = true;
        }

        public void Release()
        {
            button.Visible = false;
        }

        public int Lock(bool showLocked)
        {
            return 0;
        }

        public void Unlock(int LockId)
        {
            //Nothing
        }

        public event EventHandler<TriggerControlEventArgs> Fired;

        #endregion

        #region IHardwareController Members

        public void Initialize()
        {
            //Do nothing
        }

        public void DeInitialize()
        {
            //Do nothing
        }

        #endregion
    }
}

