using System;
using System.Collections.Generic;

namespace PCSX2_Configurator.Core
{
    interface IProcessHelpers
    {
        string GetWindowTitleText(IntPtr window);
        IEnumerable<IntPtr> GetProcessWindows(int pid);
    }
}
