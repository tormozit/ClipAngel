//Copyright 2017 Starykh Sergey (tormozit) tormozit@gmail.com
//GNU General Public License version 3.0 (GPLv3)

using System;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ClipAngel
{
    static class Program
    {
        static readonly string MyMutexName = "ClipAngelApplicationMutex";
        static Mutex MyMutex;
        [STAThread]
        static void Main(string[] args)
        {
            if (IsSingleInstance())
            {
                string UserSettingsPath = "";
                bool PortableMode = false;
                PortableMode = args.Contains("/p") || args.Contains("/portable");
                // http://stackoverflow.com/questions/1382617/how-to-make-designer-generated-net-application-settings-portable/2579399#2579399
                UserSettingsPath = MakePortable(Properties.Settings.Default, PortableMode);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Main Main = new Main();
                Main.StartMinimized = args.Contains("/m");
                Main.UserSettingsPath = UserSettingsPath;
                Main.PortableMode = PortableMode;
                Application.Run(Main);
            }
        }
        private static string MakePortable(ApplicationSettingsBase settings, bool PortableMode = true)
        {
            var portableSettingsProvider =
                new PortableSettingsProvider(settings.GetType().Name + ".settings", PortableMode);
            settings.Providers.Add(portableSettingsProvider);
            foreach (System.Configuration.SettingsProperty prop in settings.Properties)
                prop.Provider = portableSettingsProvider;
            settings.Reload();
            return portableSettingsProvider.GetAppSettingsPath();
        }
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        static bool IsSingleInstance()
        {
            try
            {
                Mutex.OpenExisting(MyMutexName);
            }
            catch
            {
                MyMutex = new Mutex(true, MyMutexName);
                return true;
            }
            System.Diagnostics.Process[] allProcess = System.Diagnostics.Process.GetProcesses();
            System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            foreach (System.Diagnostics.Process process in allProcess)
            {
                if (currentProcess.ProcessName == process.ProcessName && currentProcess.Id != process.Id)
                {
                    IntPtr hWnd = process.MainWindowHandle;
                    Application.DoEvents();
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, 1);
                        SetForegroundWindow(hWnd);
                    }

                }
            }
            return false;
        }
    }
}
