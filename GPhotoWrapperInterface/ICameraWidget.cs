using System.Runtime.InteropServices;

namespace LibGPhoto2
{
    public enum CameraWidgetType
    {				/* Value (get/set):     */
        GP_WIDGET_WINDOW,
        GP_WIDGET_SECTION,
        GP_WIDGET_TEXT,		/* char *               */
        GP_WIDGET_RANGE,	/* float                */
        GP_WIDGET_TOGGLE,	/* int                  */
        GP_WIDGET_RADIO,	/* char *               */
        GP_WIDGET_MENU,		/* char *               */
        GP_WIDGET_BUTTON,	/* CameraWidgetCallback */
        GP_WIDGET_DATE		/* int                  */
    }
	
    public interface ICameraWidget
    {
        void Dispose();
        HandleRef Handle { get; }
        int ChildCount { get; }
        void Append(ICameraWidget child);
        void Prepend(ICameraWidget child);
        ICameraWidget? GetChild (int n);
        ICameraWidget? GetChild (string label);
        ICameraWidget GetChildByID (int id);
        ICameraWidget GetRoot ();
        void SetInfo (string info);
        string GetInfo ();
        int GetID ();
        CameraWidgetType GetWidgetType ();
        string GetLabel ();
        void GetRange (out float min, out float max, out float increment);
        void AddChoice (string choice);
        int ChoicesCount ();
        string GetChoice (int n);
        bool Changed ();
    }
}