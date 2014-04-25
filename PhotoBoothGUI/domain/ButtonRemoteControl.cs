using System;
using Gtk;
using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.domain
{
	public class ButtonRemoteControl: IRemoteControl
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
			Triggered.BeginInvoke (this, new RemoteControlEventArgs (Id), (a)=>{},null);
		}

		public string Id { get; private set;}

		public void Prepare()
		{
			button.Visible = true;
		}

		public void Release()
		{
			button.Visible = false;
		}

		public event EventHandler<RemoteControlEventArgs> Triggered;

		public void Dispose()
		{
			//TODO: Implement
		}
	}
}

