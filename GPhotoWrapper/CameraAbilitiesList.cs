using System.Runtime.InteropServices;

namespace LibGPhoto2
{
	public class CameraAbilitiesList : Object, ICameraAbilitiesList
	{
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_abilities_list_new (out IntPtr native);

		public CameraAbilitiesList()
		{
			IntPtr native;

			Error.CheckError (gp_abilities_list_new (out native));

			this.handle = new HandleRef (this, native);
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_abilities_list_free (HandleRef list);
		
		protected override void Cleanup ()
		{
			gp_abilities_list_free(this.handle);
		}
		
		[DllImport ("libgphoto2.so")]
		internal unsafe static extern ErrorCode gp_abilities_list_load (HandleRef list, HandleRef context);

		public void Load (IContext context)
		{
			unsafe {
				ErrorCode result = gp_abilities_list_load (this.Handle, context.Handle);
				
				if (Error.IsError (result))
					throw Error.ErrorException(result);
			}
		}
		
		[DllImport ("libgphoto2.so")]
		internal unsafe static extern ErrorCode gp_abilities_list_detect (HandleRef list, HandleRef info_list, HandleRef l, HandleRef context);

		public void Detect (IPortInfoList info_list, ICameraList l, IContext context)
		{
			Error.CheckError (gp_abilities_list_detect (this.handle, info_list.Handle, 
								    l.Handle, context.Handle));
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_abilities_list_count (HandleRef list);

		public int Count ()
		{
			ErrorCode result = gp_abilities_list_count (this.handle);

			if (Error.IsError (result)) 
				throw Error.ErrorException (result);

			return (int)result;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_abilities_list_lookup_model (HandleRef list, string model);

		public int LookupModel (string model)
		{
			ErrorCode result = gp_abilities_list_lookup_model(this.handle, model);

			if (Error.IsError (result))
				throw Error.ErrorException (result);
	
			return (int)result;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_abilities_list_get_abilities (HandleRef list, int index, out CameraAbilities abilities);

		public CameraAbilities GetAbilities (int index)
		{
			CameraAbilities abilities = new CameraAbilities ();

			Error.CheckError (gp_abilities_list_get_abilities(this.Handle, index, out abilities));

			return abilities;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_abilities_list_append (HandleRef list, ref CameraAbilities abilities);

		public void Append (CameraAbilities abilities)
		{
			Error.CheckError (gp_abilities_list_append (this.Handle, ref abilities));
		}
	}
}
