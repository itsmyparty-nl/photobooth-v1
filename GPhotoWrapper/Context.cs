using System.Runtime.InteropServices;

namespace LibGPhoto2
{
	public class Context : Object, IContext
	{
		[DllImport ("libgphoto2")]
		private static extern IntPtr gp_context_new ();

		public Context ()
		{
			this.handle = new HandleRef (this, gp_context_new ());
		}
		
		[DllImport ("libgphoto2")]
		private static extern void gp_context_unref   (HandleRef context);

		protected override void Cleanup ()
		{
			gp_context_unref(handle);
		}
	}
}
