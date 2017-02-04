//Copyright 2017 Starykh Sergey (tormozit)

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//  http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

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
        static string MyMutexName = "ClipAngelApplicationMutex";
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
                //Проверяем на наличие мутекса в системе
                Mutex.OpenExisting(MyMutexName);
            }
            catch
            {
                //Если получили исключение значит такого мутекса нет, и его нужно создать
                MyMutex = new Mutex(true, MyMutexName);
                return true;
            }
            //Если исключения не было, то процесс с таким мутексом уже запущен
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
