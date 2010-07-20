using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace TickZoom
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
        	Thread.CurrentThread.Name = "UIThread";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(MainForm.Instance);
        }
    }
}