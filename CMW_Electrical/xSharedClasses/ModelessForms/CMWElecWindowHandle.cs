using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMW_Electrical
{
    /// <summary>
    /// Wrapper class for converting IntPtr to IWin32Window
    /// </summary>
    public class CMWElecWindowHandle : IWin32Window
    {
        IntPtr _hwnd;

        public CMWElecWindowHandle(IntPtr h)
        {
            Debug.Assert(IntPtr.Zero != h, 
                "expected non-null window handle.");

            _hwnd = h;
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }
    }
}
