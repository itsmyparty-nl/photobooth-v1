using System;
using System.Runtime.InteropServices;

namespace LibGPhoto2
{
#if false
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct _CameraFilesystem
	{
		
		internal delegate ErrorCode _CameraFilesystemGetFileFunc (HandleRef fs, char *folder, char *filename, CameraFileType type, HandleRef file, void *data, HandleRef context);

		internal delegate ErrorCode _CameraFilesystemDeleteFileFunc (HandleRef fs, char *folder, char *filename, void *data, HandleRef context);

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_set_file_funcs (HandleRef fs, _CameraFilesystemGetFileFunc get_file_func, _CameraFilesystemDeleteFileFunc del_file_func, void *data);
		
		internal delegate ErrorCode _CameraFilesystemGetInfoFunc (HandleRef fs, char *folder, char *filename, CameraFileInfo *info, void *data, HandleRef context);

		internal delegate ErrorCode _CameraFilesystemSetInfoFunc (HandleRef fs, char *folder, char *filename, CameraFileInfo info, void *data, HandleRef context);

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_set_info_funcs (HandleRef fs, _CameraFilesystemGetInfoFunc get_info_func, _CameraFilesystemSetInfoFunc set_info_func, void *data);

		internal delegate ErrorCode _CameraFilesystemPutFileFunc (HandleRef fs, char *folder, HandleRef file, void *data, HandleRef context);

		internal delegate ErrorCode _CameraFilesystemDeleteAllFunc (HandleRef fs, char *folder, void *data, HandleRef context);

		internal delegate ErrorCode _CameraFilesystemDirFunc (HandleRef fs, char *folder, char *name, void *data, HandleRef context);

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_set_folder_funcs (HandleRef fs, _CameraFilesystemPutFileFunc put_file_func, _CameraFilesystemDeleteAllFunc delete_all_func, _CameraFilesystemDirFunc make_dir_func, _CameraFilesystemDirFunc remove_dir_func, void *data);

		internal delegate ErrorCode _CameraFilesystemListFunc (HandleRef fs, char *folder, HandleRef list, void *data, HandleRef context);

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_set_list_funcs (HandleRef fs, _CameraFilesystemListFunc file_list_func, _CameraFilesystemListFunc folder_list_func, void *data);

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_append (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, [MarshalAs(UnmanagedType.LPTStr)] string filename, HandleRef context);

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_set_file_noop (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, HandleRef file, HandleRef context);

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_dump (HandleRef fs);
	}
#endif

	public class CameraFilesystem : Object, ICameraFilesystem
	{
		bool need_dispose;
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_new (out IntPtr fs);
		
		public CameraFilesystem()
		{
			IntPtr native;
			
			Error.CheckError (gp_filesystem_new(out native));
			
			this.handle = new HandleRef (this, native);
			need_dispose = true;
		}
		
		unsafe internal CameraFilesystem(IntPtr fs)
		{
			this.handle = new HandleRef (this, fs);
			need_dispose = false;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_free (HandleRef fs);

		protected override void Cleanup ()
		{
			if (need_dispose)
				Error.CheckError (gp_filesystem_free(this.Handle));
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_list_files (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, HandleRef list, HandleRef context);

		public ICameraList ListFiles (string folder, IContext context)
		{
			ErrorCode result;
			CameraList list = new CameraList();
			unsafe
			{
				result = gp_filesystem_list_files (this.Handle, folder, list.Handle, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
			return list;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_list_folders (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, HandleRef list, HandleRef context);

		public ICameraList ListFolders (string folder, IContext context)
		{
			ErrorCode result;
			CameraList list = new CameraList();
			unsafe
			{
				result = gp_filesystem_list_folders (this.Handle, folder, list.Handle, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
			return list;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_get_file (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, [MarshalAs(UnmanagedType.LPTStr)] string filename, CameraFileType type, HandleRef file, HandleRef context);


		public ICameraFile GetFile (string folder, string filename, CameraFileType type, IContext context)
		{
			ErrorCode result;
			CameraFile file = new CameraFile();
			unsafe
			{
				result = gp_filesystem_get_file (this.Handle, folder, filename, type, file.Handle, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
			return file;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_put_file (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, HandleRef file, HandleRef context);

		public void PutFile (string folder, ICameraFile file, IContext context)
		{
			ErrorCode result;
			unsafe
			{
				result = gp_filesystem_put_file (this.Handle, folder, file.Handle, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_delete_file (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, [MarshalAs(UnmanagedType.LPTStr)] string filename, HandleRef context);

		public void DeleteFile (string folder, string filename, IContext context)
		{
			ErrorCode result;
			unsafe
			{
				result = gp_filesystem_delete_file (this.Handle, folder, filename, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_delete_all (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, HandleRef context);

		public void DeleteAll (string folder, IContext context)
		{
			ErrorCode result;
			unsafe
			{
				result = gp_filesystem_delete_all (this.Handle, folder, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_make_dir (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, [MarshalAs(UnmanagedType.LPTStr)] string name, HandleRef context);

		public void MakeDirectory (string folder, string name, IContext context)
		{
			ErrorCode result;
			unsafe
			{
				result = gp_filesystem_make_dir (this.Handle, folder, name, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_remove_dir (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, [MarshalAs(UnmanagedType.LPTStr)] string name, HandleRef context);

		public void RemoveDirectory (string folder, string name, IContext context)
		{
			ErrorCode result;
			unsafe
			{
				result = gp_filesystem_remove_dir (this.Handle, folder, name, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_get_info (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, [MarshalAs(UnmanagedType.LPTStr)] string filename, out CameraFileInfo info, HandleRef context);

		public CameraFileInfo GetInfo (string folder, string filename, IContext context)
		{
			ErrorCode result;
			CameraFileInfo fileinfo = new CameraFileInfo();
			unsafe
			{
				result = gp_filesystem_get_info  (this.Handle, folder, filename, out fileinfo, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
			return fileinfo;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_set_info (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, [MarshalAs(UnmanagedType.LPTStr)] string filename, CameraFileInfo info, HandleRef context);

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_set_info_noop (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, CameraFileInfo info, HandleRef context);

		public void SetInfo (string folder, string filename, CameraFileInfo fileinfo, IContext context)
		{
			ErrorCode result;
			unsafe
			{
				result = gp_filesystem_set_info (this.Handle, folder, filename, fileinfo, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_number (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, [MarshalAs(UnmanagedType.LPTStr)] string filename, HandleRef context);
		
		public int GetNumber (string folder, string filename, IContext context)
		{
			ErrorCode result;
			unsafe
			{
				result = gp_filesystem_number (this.Handle, folder, filename, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
			return (int)result;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_name (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, int filenumber, out string filename, HandleRef context);

		public string GetName (string folder, int number, IContext context)
		{
			ErrorCode result;
			string name;
			unsafe
			{
				result = gp_filesystem_name (this.Handle, folder, number, out name, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
			return name;
		}
		
		//[DllImport ("libgphoto2.so")]
		//private static extern ErrorCode gp_filesystem_get_folder (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string filename, IntPtr folder, HandleRef context);

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_count (HandleRef fs, [MarshalAs(UnmanagedType.LPTStr)] string folder, HandleRef context);
		
		public int Count (string folder, IContext context)
		{
			ErrorCode result;
			unsafe
			{
				result = gp_filesystem_count (this.Handle, folder, context.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
			return (int)result;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_filesystem_reset (HandleRef fs);

		public void Reset ()
		{
			ErrorCode result;
			unsafe
			{
				result = gp_filesystem_reset (this.Handle);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}
	}
}
