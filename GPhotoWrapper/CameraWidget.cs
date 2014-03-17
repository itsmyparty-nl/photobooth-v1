using System;
using System.Runtime.InteropServices;

namespace LibGPhoto2
{
	public class CameraWidget : IDisposable, ICameraWidget
	{
		protected HandleRef handle;
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_ref (HandleRef widget);

		public CameraWidget (IntPtr native)
		{
			this.handle = new HandleRef (this, native);
			gp_widget_ref (this.Handle);
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_new (CameraWidgetType type, string lable, out IntPtr widget);

		public CameraWidget (CameraWidgetType type, string label)
		{
			ErrorCode result;
			unsafe 
			{
				IntPtr native;
				result = gp_widget_new(type, label,out native);
				this.handle = new HandleRef (this, native);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		~CameraWidget()
		{
			Dispose(false);
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_unref (HandleRef widget);
		
		protected virtual void Dispose (bool disposing)
		{
			ErrorCode result;
			unsafe
			{
				if (this.Handle.Handle != IntPtr.Zero)
				{
					result = gp_widget_unref(this.Handle);
					if (Error.IsError(result)) throw Error.ErrorException(result);
				}
			}
		}
		
		public HandleRef Handle
		{
			get {
				return handle;
			}
		}

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_append (HandleRef widget, HandleRef child);

		public void Append(ICameraWidget child)
		{
			ErrorCode result;
	
			result = gp_widget_append(this.Handle, child.Handle);
			
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_prepend (HandleRef widget, HandleRef child);
		
		public void Prepend(ICameraWidget child)
		{
			ErrorCode result;

			result = gp_widget_prepend(this.Handle, child.Handle);

			if (Error.IsError(result)) throw Error.ErrorException(result);
		}	
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_count_children (HandleRef widget);

		public int ChildCount
		{
			get
			{
				ErrorCode result;
				
				result = gp_widget_count_children(this.Handle);
					
				if (Error.IsError(result)) throw Error.ErrorException(result);
				return (int)result;
			}
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_get_child (HandleRef widget, int child_number, out IntPtr child);
		
		public ICameraWidget GetChild (int n)
		{
			ErrorCode result;
			IntPtr native;

			result = gp_widget_get_child(this.Handle, n, out native);
			
			if (native != IntPtr.Zero)
			{
				CameraWidget child = new CameraWidget (native);
			
			if (Error.IsError(result)) throw Error.ErrorException(result);
			return child;
			}
			return null;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_get_child_by_label (HandleRef widget, string label, out IntPtr child);
		
		public ICameraWidget GetChild (string label)
		{
			ErrorCode result;
			IntPtr native;
			
			result = gp_widget_get_child_by_label(this.Handle, label, out native);
			if (native != IntPtr.Zero)
			{
				CameraWidget child = new CameraWidget (native);
			
			if (Error.IsError(result)) throw Error.ErrorException(result);
			return child;
			}
			return null;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_get_child_by_id (HandleRef widget, int id, out IntPtr child);
		
		public ICameraWidget GetChildByID (int id)
		{
			ErrorCode result; 			
			IntPtr native;
			
			result = gp_widget_get_child_by_id(this.Handle, id, out native);
			CameraWidget child = new CameraWidget (native);
			
			if (Error.IsError(result)) throw Error.ErrorException(result);
			return child;
		}

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_get_root (HandleRef widget, out IntPtr root);
		
		public ICameraWidget GetRoot ()
		{
			ErrorCode result;
			IntPtr native;

			result = gp_widget_get_root (this.Handle, out native);
			CameraWidget root = new CameraWidget(native);

			if (Error.IsError(result)) throw Error.ErrorException(result);
			return root;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_set_info (HandleRef widget, string info);
		
		public void SetInfo (string info)
		{
			ErrorCode result;

			result = gp_widget_set_info(this.Handle, info);
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_get_info (HandleRef widget, out string info);
		
		public string GetInfo ()
		{
			ErrorCode result;
			string info;
			
			result = gp_widget_get_info (this.Handle, out info);

			if (Error.IsError(result)) throw Error.ErrorException(result);
			return info;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_get_id (HandleRef widget, out int id);
		
		public int GetID ()
		{
			ErrorCode result;
			int id;

			result = gp_widget_get_id (this.Handle, out id);

			if (Error.IsError(result)) throw Error.ErrorException(result);
			return id;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_get_type (HandleRef widget, out CameraWidgetType type);

		public CameraWidgetType GetWidgetType ()
		{
			ErrorCode result;
			CameraWidgetType widget_type;

			result = gp_widget_get_type(this.Handle, out widget_type);

			if (Error.IsError(result)) throw Error.ErrorException(result);
			return widget_type;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_get_label (HandleRef widget, out string label);
		
		public string GetLabel ()
		{
			ErrorCode result;
			string label;

			result = gp_widget_get_label(this.Handle, out label);

			if (Error.IsError(result)) throw Error.ErrorException(result);
			return label;
		}
		
		/*public void SetValue (string value)
		{
			ErrorCode result;
			unsafe
			{
				result = _CameraWidget.gp_widget_set_value(obj, Marshal.StringToHGlobalAnsi(value));
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}
		
		public void SetValue (float value)
		{
			ErrorCode result;
			unsafe
			{
				IntPtr ptr = (void*)value;
				result = _CameraWidget.gp_widget_set_value(obj, ptr);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}
		
		public void SetValue (int value)
		{
			ErrorCode result;
			unsafe
			{
				IntPtr ptr = value;
				result = _CameraWidget.gp_widget_set_value(obj, ptr);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}
		
		/*public void SetValue (CameraWidgetCallback value)
		{
			ErrorCode result;
			unsafe
			{
				IntPtr ptr = &value;
				result = _CameraWidget.gp_widget_set_value(obj, ptr);
			}
			if (Error.IsError(result)) throw Error.ErrorException(result);
		}*/
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_get_range (HandleRef range, out 
								      float min, out float max, out float increment);

		public void GetRange (out float min, out float max, out float increment)
		{
			ErrorCode result;

			result = gp_widget_get_range(this.Handle, out min, out max, out increment);

			if (Error.IsError(result)) throw Error.ErrorException(result);
		}

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_add_choice (HandleRef widget, string choice);
		
		public void AddChoice (string choice)
		{
			ErrorCode result;
			
			result = gp_widget_add_choice (this.Handle, choice);

			if (Error.IsError(result)) throw Error.ErrorException(result);
		}

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_count_choices (HandleRef widget);

		public int ChoicesCount ()
		{
			ErrorCode result;

			result = gp_widget_count_choices(this.Handle);
			
			if (Error.IsError(result)) throw Error.ErrorException(result);
			return (int)result;
		}

		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_get_choice (HandleRef widget, int choice_number, out string choice);

		public string GetChoice (int n)
		{
			ErrorCode result;
			string choice;

			result = gp_widget_get_choice(this.Handle, n, out choice);

			if (Error.IsError(result)) throw Error.ErrorException(result);
			return choice;
		}
		
		[DllImport ("libgphoto2.so")]
		private static extern ErrorCode gp_widget_changed (HandleRef widget);

		public bool Changed ()
		{
			ErrorCode result;

			result = gp_widget_changed(this.Handle);

			if (Error.IsError(result)) throw Error.ErrorException(result);

			if ((int)result == 1)
				return true;
			else
				return false;
			
		}
	}
}
