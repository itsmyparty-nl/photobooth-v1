using System.Runtime.InteropServices;

namespace LibGPhoto2
{
    public interface ICameraList
    {
        int Count ();
        void SetName (int n, string name);
        void SetValue (int n, string value);
        string GetName (int index);
        string GetValue (int index);
        void Append (string name, string value);
        void Populate (string format, int count);
        void Reset ();
        void Sort ();
        int GetPosition(string name, string value);
        HandleRef Handle { get; }
        void Dispose ();
    }
}