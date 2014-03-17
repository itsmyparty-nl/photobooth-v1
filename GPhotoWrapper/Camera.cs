using System;
using System.Runtime.InteropServices;

namespace LibGPhoto2
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct CameraPrivateLibrary
	{
	}
	
	[StructLayout(LayoutKind.Sequential)]
	internal struct CameraPrivateCore
	{
	}
	
#if false
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct CameraFunctions
	{
		internal delegate ErrorCode _CameraExitFunc (_Camera *camera, HandleRef context);

		internal delegate ErrorCode _CameraGetConfigFunc (_Camera *camera, out IntPtr widget, HandleRef context);

		internal delegate ErrorCode _CameraSetConfigFunc (_Camera *camera, HandleRef widget, HandleRef context);

		internal delegate ErrorCode _CameraCaptureFunc (_Camera *camera, CameraCaptureType type, IntPtr path, HandleRef context);

		internal delegate ErrorCode _CameraCapturePreviewFunc (_Camera *camera, _CameraFile *file, HandleRef context);
		
		internal delegate ErrorCode _CameraSummaryFunc (_Camera *camera, IntPtr text, HandleRef context);
		
		internal delegate ErrorCode _CameraManualFunc (_Camera *camera, IntPtr text, HandleRef context);
		
		internal delegate ErrorCode _CameraAboutFunc (_Camera *camera, IntPtr text, HandleRef context);
		
		internal delegate ErrorCode _CameraPrePostFunc (_Camera *camera, HandleRef context);
                                             
		/* Those will be called before and after each operation */
		_CameraPrePostFunc pre_func;
		_CameraPrePostFunc post_func;

		_CameraExitFunc exit;

		/* Configuration */
		_CameraGetConfigFunc       get_config;
		_CameraSetConfigFunc       set_config;

		/* Capturing */
		_CameraCaptureFunc        capture;
		_CameraCapturePreviewFunc capture_preview;

		/* Textual information */
		_CameraSummaryFunc summary;
		_CameraManualFunc  manual;
		_CameraAboutFunc   about;
		
		/* Reserved space to use in the future without changing the struct size */
		IntPtr reserved1;
		IntPtr reserved2;
		IntPtr reserved3;
		IntPtr reserved4;
		IntPtr reserved5;
		IntPtr reserved6;
		IntPtr reserved7;
		IntPtr reserved8;
	}
#endif

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct _Camera
	{
		public IntPtr port;
		public IntPtr fs;
		public IntPtr functions;

		//CameraPrivateLibrary  *pl; /* Private data of camera libraries    */
		//CameraPrivateCore     *pc; /* Private data of the core of gphoto2 */
		public IntPtr p1;
		public IntPtr pc;
		
		public IntPtr GetFS ()
		{
			return fs;
		}
	}
	
	public class Camera : Object, ICamera
	{
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_new (out IntPtr handle);

		public Camera()
		{
			IntPtr native;

			Error.CheckError (gp_camera_new (out native));
			
			handle = new HandleRef (this, native);
		}

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_unref (HandleRef camera);

		protected override void Cleanup ()
		{
			gp_camera_unref(Handle);
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_set_abilities (HandleRef camera, CameraAbilities abilities);

		public void SetAbilities (CameraAbilities abilities)
		{
		        Error.CheckError (gp_camera_set_abilities(Handle, abilities));
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_get_abilities (HandleRef camera, out CameraAbilities abilities);

		public CameraAbilities GetAbilities ()
		{
			CameraAbilities abilities;
			
			Error.CheckError (gp_camera_get_abilities(Handle, out abilities));

			return abilities;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_set_port_info (HandleRef camera, PortInfoHandle info);

		public void SetPortInfo (IPortInfo portinfo)
		{
		    Error.CheckError (gp_camera_set_port_info (Handle, portinfo.Handle));
		}

	    [DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_get_port_info (HandleRef camera, out PortInfoHandle info);

		public IPortInfo GetPortInfo ()
		{
			PortInfoHandle portinfoHandle;
            Error.CheckError(gp_camera_get_port_info(Handle, out portinfoHandle));
		    return new PortInfo(portinfoHandle);
		}
		

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_get_port_speed (HandleRef camera);

		public int GetPortSpeed ()
		{
			return (int) Error.CheckError (gp_camera_get_port_speed (Handle));
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_set_port_speed (HandleRef camera, int speed);

		public void SetPortSpeed (int speed)
		{
			Error.CheckError (gp_camera_set_port_speed (Handle, speed));
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_init (HandleRef camera, HandleRef context);

		public void Init (IContext context)
		{
			Error.CheckError (gp_camera_init (Handle, context.Handle));
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_exit (HandleRef camera, HandleRef context);
		
		public void Exit (IContext context)
		{
			Error.CheckError (gp_camera_exit (Handle, context.Handle));
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_capture (HandleRef camera, CameraCaptureType type, out ICameraFilePath path, HandleRef context);
		
		public ICameraFilePath Capture (CameraCaptureType type, IContext context)
		{
			ICameraFilePath path;

			Error.CheckError (gp_camera_capture (Handle, type, out path, context.Handle));

			return path;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_capture_preview (HandleRef camera, HandleRef file, HandleRef context);
		
		public ICameraFile CapturePreview (IContext context)
		{
			var file = new CameraFile();
			
			Error.CheckError (gp_camera_capture_preview (Handle, file.Handle, context.Handle));

			return file;
		}
		
//#if UNUSED
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_get_config (HandleRef camera, out IntPtr widget, HandleRef context);
		
		public ICameraWidget GetConfig (IContext context)
		{
			IntPtr widgetHandle;
			Error.CheckError (gp_camera_get_config (Handle, out widgetHandle, context.Handle));
			return new CameraWidget(widgetHandle);
		}
	
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_set_config (HandleRef camera, HandleRef widget, HandleRef context);
		
		public void SetConfig (IContext context, ICameraWidget widget)
		{
			Error.CheckError (gp_camera_set_config (Handle, widget.Handle, context.Handle));
		}
//#endif		

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_folder_list_files (HandleRef camera, string folder, HandleRef list, HandleRef context);
		
		public ICameraList ListFiles (string folder, IContext context)
		{
			var fileList = new CameraList ();
			
			Error.CheckError (gp_camera_folder_list_files(Handle, folder, fileList.Handle, context.Handle));

			return fileList;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_folder_list_folders (HandleRef camera, string folder, HandleRef list, HandleRef context);

		public ICameraList ListFolders (string folder, IContext context)
		{
			var fileList = new CameraList();

			Error.CheckError (gp_camera_folder_list_folders (Handle, folder, fileList.Handle, context.Handle));

			return fileList;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_folder_put_file (HandleRef camera, [MarshalAs(UnmanagedType.LPTStr)] string folder, HandleRef file, HandleRef context);
		
		public void PutFile (string folder, ICameraFile file, IContext context)
		{
			Error.CheckError (gp_camera_folder_put_file(Handle, folder, file.Handle, context.Handle));
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_folder_delete_all (HandleRef camera, string folder, HandleRef context);
		
		public void DeleteAll (string folder, IContext context)
		{
			Error.CheckError (gp_camera_folder_delete_all (Handle, folder, context.Handle));
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_folder_make_dir (HandleRef camera, string folder,  string name, HandleRef context);
		
		public void MakeDirectory (string folder, string name, IContext context)
		{
			Error.CheckError (gp_camera_folder_make_dir (Handle, folder, name, context.Handle));
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_folder_remove_dir (HandleRef camera, string folder, string name, HandleRef context);
		
		public void RemoveDirectory (string folder, string name, IContext context)
		{
			Error.CheckError (gp_camera_folder_remove_dir(Handle, folder, name, context.Handle));
		}
		

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_file_get (HandleRef camera, string folder, string file, CameraFileType type, HandleRef cameraFile, HandleRef context);
		
		public ICameraFile GetFile (string folder, string name, CameraFileType type, IContext context)
		{
			var file = new CameraFile();
			
			Error.CheckError (gp_camera_file_get(Handle, folder, name, type, file.Handle, context.Handle));

			return file;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_file_delete (HandleRef camera, string folder, string file, HandleRef context);

		public void DeleteFile (string folder, string name, IContext context)
		{
		    Error.CheckError (gp_camera_file_delete(Handle, folder, name, context.Handle));
		}


	    [DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_file_get_info (HandleRef camera, string folder, string file, out CameraFileInfo info, HandleRef context);
		
		public CameraFileInfo GetFileInfo (string folder, string name, IContext context)
		{
			CameraFileInfo fileinfo;
		    Error.CheckError (gp_camera_file_get_info(Handle, folder, name, out fileinfo, context.Handle));

		    return fileinfo;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_file_set_info (HandleRef camera, string folder, string file, CameraFileInfo info, HandleRef context);
		
		public void SetFileInfo (string folder, string name, CameraFileInfo fileinfo, IContext context)
		{
		    Error.CheckError (gp_camera_file_set_info(Handle, folder, name, fileinfo, context.Handle));
		}

	    [DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_get_manual (HandleRef camera, out CameraText manual, HandleRef context);
		
		public CameraText GetManual (IContext context)
		{
			CameraText manual;
		    Error.CheckError (gp_camera_get_manual(Handle, out manual, context.Handle));
		    return manual;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_get_summary (HandleRef camera, out CameraText summary, HandleRef context);
		
		public CameraText GetSummary (IContext context)
		{
			CameraText summary;

			Error.CheckError (gp_camera_get_summary(Handle, out summary, context.Handle));

			return summary;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_camera_get_about (HandleRef camera, out CameraText about, HandleRef context);

		public CameraText GetAbout (IContext context)
		{
			CameraText about;
			
			Error.CheckError (gp_camera_get_about(Handle, out about, context.Handle));

			return about;
		}
		
		public ICameraFilesystem GetFS()
		{
			CameraFilesystem fs;
			unsafe {
				var obj = (_Camera *)Handle.Handle;
				fs = new CameraFilesystem(obj->GetFS ());
			}
			return fs;
		}
	}
}
