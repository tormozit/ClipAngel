using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Ionic.Zip;
using System.Diagnostics;

namespace ExternalUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("This is internal Clip Angel application for update function.");
                return;
            }
            string ZipFileName = args[0];
            string TargetPath = args[1];
            string ExeName = args[2];
            string ExeParam = args[3];
            int processId = Int32.Parse(args[4]);
            Process MainProcess = null;
            try
            {
                MainProcess = Process.GetProcessById(processId);
            }
            catch
            {
                // If process not found
            }
            if (MainProcess != null)
                MainProcess.WaitForExit();
            ZipFile zip = new ZipFile(ZipFileName);
            zip.ExtractAll(TargetPath, ExtractExistingFileAction.OverwriteSilently);
            zip.Dispose();
            Process.Start(ExeName, ExeParam);
            //Directory.Delete(Path.GetDirectoryName(ZipFileName), true); // Impossible to delete self files
        }
    }
}
