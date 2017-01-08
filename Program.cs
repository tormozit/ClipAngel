using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipAngel
{
    static class Program
    {
        static string MyMutexName = "ClipAngelApplicationMutex";
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
                Mutex mutex = new Mutex(true, MyMutexName);
                return true;
            }
            //Если исключения не было, то процесс с таким мутексом уже запущен
            return false;
        }
    }
}
