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
            string ZipFileName = args[0];
            string TargetPath = args[1];
            string ExeName = args[2];
            int processId = Int32.Parse(args[3]);
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
            Process.Start(ExeName);
            //Directory.Delete(Path.GetDirectoryName(ZipFileName), true); // Impossible to delete self files
        }
    }
}
