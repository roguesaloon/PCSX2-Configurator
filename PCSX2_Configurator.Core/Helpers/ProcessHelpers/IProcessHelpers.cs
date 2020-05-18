using System;

namespace PCSX2_Configurator.Core.Helpers
{
    internal interface IProcessHelpers
    {
        string GetWindowTitleText(IntPtr window);
        (IntPtr handle, string title) FindWindowForProcess(int processId, Func<string, bool> filter = null);
        void SendMessageCopyDataToWindowAnsi(IntPtr window, string data);
    }
}
