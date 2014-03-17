using System.Runtime.InteropServices;

namespace LibGPhoto2
{
    public interface IContext
    {
        HandleRef Handle { get; }
        void Dispose ();
    }
}