using System;
using LibGPhoto2;

namespace GPhotoWrapperConsoleTest
{
	class MainClass
	{
		static string basename (string filename)
		{
			char[] chars = { '/', '\\' };
			string[] components = filename.Split (chars);
			return components[components.Length - 1];
		}

		public static int Main (string[] args)
		{
			return TestCamera (args);
		}

		public static int TestCamera (string[] args)
		{
			System.Console.WriteLine ("Testing camera...");
			IContext ctx = new Context ();
			
			try {
				ICamera camera = new Camera ();
				camera.Init (ctx);

				//CameraText text = camera.GetSummary(ctx);
				//System.Console.WriteLine ("summary: "+ text.Text);
				
				CameraAbilities abilities = camera.GetAbilities();
				string camlib_basename = basename (abilities.library);
				System.Console.WriteLine ("Camera:");
			
				System.Console.WriteLine ("Camera: {2,-20}  {0,-20}  {1}", abilities.id, abilities.model, camlib_basename);
				CameraDriverStatus status = abilities.status;
				
			    System.Console.WriteLine ("Status: "+status.ToString());
			
				System.Console.WriteLine ("Capturing image:");
				camera.Capture(CameraCaptureType.Image, ctx);
				
				//string manualText = camera.GetManual(ctx).Text;
				//System.Console.WriteLine ("manual: "+ manualText);
				
				IPortInfo portInfo = camera.GetPortInfo();
				System.Console.WriteLine ("port info: "+ portInfo.Name + ", "+portInfo.Path);
				
				string aboutText = camera.GetAbout(ctx).Text;
				System.Console.WriteLine ("about: "+ camera.GetAbout(ctx).Text);
				
				//CameraFilesystem fs = camera.GetFS();
				//System.Console.WriteLine ("Filesystem: "+ fs.);
				
				
				//camera.CapturePreview(ctx);
				
				// Return non-0 when test fails
				return 0;
			} catch (Exception ex) {
				System.Console.WriteLine (ex.ToString ());
				
				// Return non-0 when test fails
				return 1;
			}
			
		}

		public static int TestCameraAbilitiesList (string[] args)
		{
			System.Console.WriteLine ("Testing libgphoto2-sharp...");
			Context ctx = new Context ();
			CameraAbilitiesList al = new CameraAbilitiesList ();
			al.Load (ctx);
			
			int count = al.Count ();
			
			if (count < 0) {
				System.Console.WriteLine ("CameraAbilitiesList.Count() error: " + count);
				return (1);
			} else if (count == 0) {
				System.Console.WriteLine ("no camera drivers (camlibs) found in camlib dir");
				return (1);
			}
			
			for (int i = 0; i < count; i++) {
				CameraAbilities abilities = al.GetAbilities (i);
				string camlib_basename = basename (abilities.library);
				System.Console.WriteLine ("{0,3}  {3,-20}  {1,-20}  {2}", i, abilities.id, abilities.model, camlib_basename);
			}
			
			// Return non-0 when test fails
			return 0;
		}
		
	}
}
