using System.Runtime.InteropServices;

namespace LibGPhoto2
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CameraText
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (32 * 1024))]
        string text;

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct ICameraFilePath
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string folder;
    }

    public enum CameraCaptureType
    {
        Image,
        Movie,
        Sound
    }

    public interface ICamera
    {
        void SetAbilities (CameraAbilities abilities);
        CameraAbilities GetAbilities ();
        void SetPortInfo (IPortInfo portinfo);
        IPortInfo GetPortInfo ();
        int GetPortSpeed ();
        void SetPortSpeed (int speed);
        void Init (IContext IContext);
        void Exit (IContext IContext);
        ICameraFilePath Capture (CameraCaptureType type, IContext IContext);
        ICameraFile CapturePreview (IContext IContext);
        ICameraWidget GetConfig (IContext IContext);
        void SetConfig (IContext IContext, ICameraWidget widget);
        ICameraList ListFiles (string folder, IContext IContext);
        ICameraList ListFolders (string folder, IContext IContext);
        void PutFile (string folder, ICameraFile file, IContext IContext);
        void DeleteAll (string folder, IContext IContext);
        void MakeDirectory (string folder, string name, IContext IContext);
        void RemoveDirectory (string folder, string name, IContext IContext);
        ICameraFile GetFile (string folder, string name, CameraFileType type, IContext IContext);
        void DeleteFile (string folder, string name, IContext IContext);
        CameraFileInfo GetFileInfo (string folder, string name, IContext IContext);
        void SetFileInfo (string folder, string name, CameraFileInfo fileinfo, IContext IContext);
        CameraText GetManual (IContext IContext);
        CameraText GetSummary (IContext IContext);
        CameraText GetAbout (IContext IContext);
        ICameraFilesystem GetFS();
        HandleRef Handle { get; }
        void Dispose ();
    }
}