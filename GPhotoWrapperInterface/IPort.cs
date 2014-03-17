using System.Runtime.InteropServices;

namespace LibGPhoto2
{
    public enum PortType
    {
        None = 0,
        Serial = 1 << 0,
        USB = 1 << 2,
        Disk = 1 << 3,
        PTPIP = 1 << 4
    }

    public enum PortSerialParity
    {
        Off = 0,
        Even,
        Odd
    }

    public enum Pin
    {
        RTS,
        DTR,
        CTS,
        DSR,
        CD,
        RING
    }

    public enum Level
    {
        Low = 0,
        High = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PortPrivateLibrary
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PortPrivateCore
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PortSettingsSerial
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public char[] port;
        public int speed;
        public int bits;
        public PortSerialParity parity;
        public int stopbits;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PortSettingsUSB
    {
        public int inep, outep, intep;
        public int config;
        public int pinterface;
        public int altsetting;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PortSettingsDisk
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public char[] mountpoint;
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct PortSettings
    {
        [FieldOffset(0)]
        public PortSettingsSerial serial;
        [FieldOffset(0)]
        public PortSettingsUSB usb;
        [FieldOffset(0)]
        public PortSettingsDisk disk;
    }

    public interface IPort
    {
        void SetInfo (IPortInfo info);
        IPortInfo GetInfo ();
        void Open ();
        void Close ();
        byte[] Read (int size);
        void Write (byte[] data);
        void SetSettings (PortSettings settings);
        PortSettings GetSettings ();
        int Timeout { get; set; }
        HandleRef Handle { get; }
        void Dispose ();
    }
}