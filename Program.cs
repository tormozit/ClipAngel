using System;
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
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Main Main = new Main();
                Main.StartMinimized = args.Length > 0 && args[0] == "/m";
                Application.Run(Main);
            }
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
