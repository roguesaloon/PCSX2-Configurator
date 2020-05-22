using System;

namespace PCSX2_Configurator.Helpers
{
    internal interface IProcessHelpers
    {
        string GetWindowTitleText(IntPtr window);
        (IntPtr handle, string title) FindWindowForProcess(int processId, Func<string, bool> filter = null);
        void SendMessageCopyDataToWindowAnsi(IntPtr window, string data);
        void SuppressErrorMessages();
        void RestoreErrorMessages();
    }
}