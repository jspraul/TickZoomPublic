using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using TickZoom.Api;
using WeifenLuo.WinFormsUI.Docking;

namespace TickZoom
{
    public partial class DummyOutputWindow : ToolWindow
    {
    	Log log;
        Thread thread_ProcessMessages;

        public DummyOutputWindow()
        {
            InitializeComponent();
        }

		void DummyOutputWindowLoad(object sender, EventArgs e)
		{
			if( !DesignMode) {
				log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	            thread_ProcessMessages = new Thread(new ThreadStart(ProcessMessages));
	            thread_ProcessMessages.Name = "ProcessMessages";
	            thread_ProcessMessages.Priority = ThreadPriority.Lowest;
	            thread_ProcessMessages.Start();
			}
        }
        
        private void ProcessMessages()
        {
        	try {
	            while (true)
	            {
	            	try {
		            	Echo(log.ReadLine());
		   			} catch( CollectionTerminatedException) {
		   				break;
	            	}
	            }
			} catch( Exception ex) {
				log.Error("ERROR: Thread had an exception:",ex);
			}
        }
   
   		delegate void SetTextCallback(string msg);
   		
        private void Echo(string msg)
        {
            if (this.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(Echo);
                try {
                	this.Invoke(d, new object[] { msg });
                } catch( ObjectDisposedException) {
                	// Any error here can't be logged.
                }
            }
            else
            {
//                projectDoc.Output.Text += msg + "\r\n";
//                projectDoc.Output.SelectionStart = projectDoc.Output.Text.Length;
//                projectDoc.Output.ScrollToCaret();
            }
        }
        
		void DummyOutputWindowClosing(object sender, FormClosingEventArgs e)
		{
            thread_ProcessMessages.Abort();
		}
    }
}