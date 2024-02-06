using System;
using System.Diagnostics;
using System.Windows.Forms;

///<summary>
///from Jermey Tammik (Building Coder)
///</summary>
namespace DirectObjLoader
{
    public class JtWindowHandler : IWin32Window
    {
        IntPtr _hwnd;

        public JtWindowHandler(IntPtr h)
        {
            Debug.Assert(IntPtr.Zero != h, "expected non-null window handle");

            _hwnd = h;
        }

        public IntPtr Handle
        {
            get
            {
                return _hwnd;
            }
        }
    }
}
