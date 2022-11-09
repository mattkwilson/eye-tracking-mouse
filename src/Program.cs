using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace EyeTrackingMouse
{
    static class Program
    {
        private const string Name = "EyeTrackingMouse";

        private static Mutex mutex;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew;
            mutex = new Mutex(true, Name, out createdNew);
            
            if (!createdNew)
            {                
                // kill any old application processes
                Process[] oldProcesses = Process.GetProcessesByName(Name);
                if (oldProcesses.Length > 1)
                {
                    foreach (Process process in oldProcesses)
                    {
                        if (process.Id != Process.GetCurrentProcess().Id)                        
                            process.Kill();                                                    
                    }                    
                }

            }            
            
            new EyeTrackingMouse();            
            Application.Run();

        }
    }
}

