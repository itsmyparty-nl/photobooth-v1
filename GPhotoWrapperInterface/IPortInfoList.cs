using System.Runtime.InteropServices;

namespace LibGPhoto2
{
    public interface IPortInfoList
    {
        void Load ();
        int Count();
        IPortInfo GetInfo (int n);
        int LookupPath (string path);
        int LookupName(string name);
        int Append (IPortInfo info);
        HandleRef Handle { get; }
        void Dispose ();
    }
}