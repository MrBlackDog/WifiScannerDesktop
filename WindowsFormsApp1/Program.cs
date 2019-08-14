using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiFi.Scanner;

namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var current = Process.GetCurrentProcess();
            var otherInstance =
                Process.GetProcessesByName(current.ProcessName).Where(p => p.Id != current.Id).FirstOrDefault();

            AppDomain.CurrentDomain.UnhandledException +=
                    new UnhandledExceptionEventHandler(Unhandled_Exception);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormScanner());
        }
        private static void Unhandled_Exception(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;

            MessageBox.Show(ex.Message, "Error...",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
        }
    }
}
