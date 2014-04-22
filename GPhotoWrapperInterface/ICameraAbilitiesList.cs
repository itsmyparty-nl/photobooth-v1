using System.Runtime.InteropServices;

namespace LibGPhoto2
{
    public enum CameraDriverStatus
    {
        Production,
        Testing,
        Experimental,
        Depreciated
    }

    public enum CameraOperation
    {
        None = 0,
        CaptureImage = 1 << 0,
        CaptureVideo = 1 << 1,
        CaptureAudio = 1 << 2,
        CapturePreview = 1 << 3,
        Config = 1 << 4
    }

    public enum CameraFileOperation
    {
        None = 0,
        Delete = 1 << 1,
        Preview = 1 << 3,
        Raw = 1 << 4,
        Audio = 1 << 5,
        Exif = 1 << 6
    }

    public enum CameraFolderOperation
    {
        None = 0,
        DeleteAll = 1 << 0,
        PutFile = 1 << 1,
        MakeDirectory = 1 << 2,
        RemoveDirectory = 1 << 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraAbilities
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string model;
        public CameraDriverStatus status;

        public PortType port;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public int[] speed;

        public CameraOperation operations;
        public CameraFileOperation file_operations;
        public CameraFolderOperation folder_operations;

        public int usb_vendor;
        public int usb_product;
        public int usb_class;
        public int usb_subclass;
        public int usb_protocol;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string library;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string id;

        public int reserved1;
        public int reserved2;
        public int reserved3;
        public int reserved4;
        public int reserved5;
        public int reserved6;
        public int reserved7;
        public int reserved8;
    }
    
    public interface ICameraAbilitiesList
    {
        void Load (IContext context);
        void Detect (IPortInfoList info_list, ICameraList l, IContext context);
        int Count ();
        int LookupModel (string model);
        CameraAbilities GetAbilities (int index);
        void Append (CameraAbilities abilities);
        HandleRef Handle { get; }
        void Dispose ();
    }
}