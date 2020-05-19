using System;
using System.Text;
using System.Runtime.InteropServices;

namespace PCSX2_Configurator.Helpers
{
    internal sealed class WindowsProcessHelpers : IProcessHelpers
    {
        private delegate bool Win32Callback(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(Win32Callback lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        private struct CopyData
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref CopyData lParam);

        public (IntPtr handle, string title) FindWindowForProcess(int processId, Func<string, bool> filter)
        {
            var titleText = default(string);
            var bestHandle = (IntPtr)0;
            var callback = new Win32Callback((handle, extraParam) =>
            {
                GetWindowThreadProcessId(handle, out var pid);
                if (pid == processId)
                {
                    titleText = GetWindowTitleText(handle);
                    var condition = filter == null || filter(titleText);
                    if (GetWindow(handle, 4) == (IntPtr)0 && !string.IsNullOrWhiteSpace(titleText) && condition)
                    {
                        bestHandle = handle;
                        return false;
                    }
                }
                return true;
            });

            EnumWindows(callback, IntPtr.Zero);
            GC.KeepAlive(callback);
            return (bestHandle, titleText);
        }

        public void SendMessageCopyDataToWindowAnsi(IntPtr hWnd, string data)
        {
            var copyData = new CopyData
            {
                dwData = IntPtr.Zero,
                cbData = data.Length + 1,
                lpData = Marshal.StringToCoTaskMemAnsi(data)
            };
            SendMessage(hWnd, 0x004A, IntPtr.Zero, ref copyData);
        }

        public string GetWindowTitleText(IntPtr hWnd)
        {
            // Allocate correct string length first
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}
