using System.Runtime.InteropServices;

namespace LibGPhoto2
{
    public enum CameraFilePermissions
    {
        None = 0,
        Read = 1 << 0,
        Delete = 1 << 1,
        All = 0xFF
    }

    public enum CameraFileStatus
    {
        NotDownloaded,
        Downloaded
    }

    public enum CameraFileInfoFields
    {
        None = 0,
        Type = 1 << 0,
        Name = 1 << 1,
        Size = 1 << 2,
        Width = 1 << 3,
        Height = 1 << 4,
        Permissions = 1 << 5,
        Status = 1 << 6,
        MTime = 1 << 7,
        All = 0xFF
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraFileInfoAudio
    {
        public CameraFileInfoFields fields;
        public CameraFileStatus status;
        public ulong size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public char[] type;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraFileInfoPreview
    {
        public CameraFileInfoFields fields;
        public CameraFileStatus status;
        public ulong size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public char[] type;

        public uint width, height;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraFileInfoFile
    {
        public CameraFileInfoFields fields;
        public CameraFileStatus status;
        public ulong size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public char[] type;

        public uint width, height;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public char[] name;
        public CameraFilePermissions permissions;
        public long time;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraFileInfo
    {
        public CameraFileInfoPreview preview;
        public CameraFileInfoFile file;
        public CameraFileInfoAudio audio;
    }

    public interface ICameraFilesystem
    {
        ICameraList ListFiles (string folder, IContext context);
        ICameraList ListFolders (string folder, IContext context);
        ICameraFile GetFile (string folder, string filename, CameraFileType type, IContext context);
        void PutFile (string folder, ICameraFile file, IContext context);
        void DeleteFile (string folder, string filename, IContext context);
        void DeleteAll (string folder, IContext context);
        void MakeDirectory (string folder, string name, IContext context);
        void RemoveDirectory (string folder, string name, IContext context);
        CameraFileInfo GetInfo (string folder, string filename, IContext context);
        void SetInfo (string folder, string filename, CameraFileInfo fileinfo, IContext context);
        int GetNumber (string folder, string filename, IContext context);
        string GetName (string folder, int number, IContext context);
        int Count (string folder, IContext context);
        void Reset ();
        HandleRef Handle { get; }
        void Dispose ();
    }
}