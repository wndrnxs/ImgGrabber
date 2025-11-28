using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace ImgGrabber
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName.ToUpper()).Length > 1)
            {
                MessageBox.Show($"{Process.GetCurrentProcess().ProcessName} Program is already running.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
