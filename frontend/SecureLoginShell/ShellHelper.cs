using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace HospitalLoginApp.Helpers
{
    public static class ShellHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        public static void LaunchWindowsShellIfNeeded()
        {
            // Check if shell (desktop/taskbar) is already present
            var shellWindow = GetShellWindow();
            if (shellWindow != IntPtr.Zero && IsWindowVisible(shellWindow))
            {
                Console.WriteLine("[INFO] Windows shell already running.");
                return;
            }

            Console.WriteLine("[INFO] No visible shell detected. Attempting to launch explorer.exe...");

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = "", // no path
                    UseShellExecute = true, // <== Important!
                    Verb = "open"
                };

                Process.Start(startInfo);
                Console.WriteLine("[INFO] explorer.exe started as shell.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] Failed to launch shell: " + ex.Message);
            }
        }
    }
}
