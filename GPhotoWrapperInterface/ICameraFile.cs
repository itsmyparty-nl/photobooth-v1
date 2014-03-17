using System.Runtime.InteropServices;

namespace LibGPhoto2
{
    public enum CameraFileType
    {
        Preview,
        Normal,
        Raw,
        Audio,
        Exif,
        MetaData
    }

    public class MimeTypes
    {
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string WAV = "audio/wav";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string RAW = "image/x-raw";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string PNG = "image/png";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string PGM = "image/x-portable-graymap";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string PPM = "image-x-portable-pixmap";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string JPEG = "image/jpeg";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string TIFF = "image/tiff";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string BMP = "image/bmp";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string QUICKTIME = "video/quicktime";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string AVI = "video/x-msvideo";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string CRW = "image/x-canon-raw";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string UNKNOWN = "application/octet-stream";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string EXIF = "application/x-exif";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string MP3 = "audio/mpeg";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string OGG = "application/ogg";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string WMA = "audio/x-wma";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string ASF = "audio/x-asf";
        [MarshalAs(UnmanagedType.LPTStr)]
        public static string MPEG = "audio/x-wma";
    }
    
    public interface ICameraFile
    {
        void Append (byte[] data);
        void Open (string filename);
        void Save (string filename);
        void Clean (string filename);
        string GetName ();
        void SetName (string name);
        CameraFileType GetFileType ();
        void SetFileType (CameraFileType type);
        string GetMimeType ();
        void SetMimeType (string mime_type);
        void DetectMimeType ();
        void AdjustNameForMimeType ();
        void Convert (string mime_type);
        void Copy (ICameraFile source);
        void SetHeader (byte[] header);
        void SetWidthHeight (int width, int height);
        void SetDataAndSize (byte[] data);
        byte[] GetDataAndSize ();
        HandleRef Handle { get; }
        void Dispose ();
    }
}