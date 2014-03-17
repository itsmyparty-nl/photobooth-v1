using System.Runtime.InteropServices;

namespace LibGPhoto2
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PortInfoHandle
    {
        public PortType type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string path;

        /* Private */
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string library_filename;
    }
    
    public interface IPortInfo
    {
        PortInfoHandle Handle { get;}
        string Name { get; }
        string Path { get; }
    }
}