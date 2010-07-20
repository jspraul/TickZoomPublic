#region Copyright
/*
 * Software: TickZoom Trading Platform
 * Copyright 2009 M. Wayne Walter
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * Business use restricted to 30 days except as otherwise stated in
 * in your Service Level Agreement (SLA).
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
#endregion


using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using NUnit.Framework;
using TickZoom;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.TickUtil;
using ZedGraph;

namespace MiscTest
{
	public class GUIThread {
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private Form mainForm;
		public Thread thread;
		public Type type;
		public GUIThread(Type type) {
			this.type = type;
			log.Debug("Starting Chart Thread");
			ThreadStart job = new ThreadStart(Run);
			thread = new Thread(job);
			thread.Name = "ChartTest";
			thread.Start();
			while( mainForm == null) {
				Thread.Sleep(1);
			}
			log.Debug("Returning Chart Created by Thread");
		}
		
		private void Run() {
			try {
   				log.Debug("Chart Thread Started");
   				mainForm = (Form) Activator.CreateInstance(type);
   				mainForm.Show();
   				Thread.CurrentThread.IsBackground = true;
   				Application.Run();
			} catch( ThreadAbortException) {
				// Thread aborted.
			} catch( Exception ex) {
				log.Error("ERROR: Thread had an exception:",ex);
			}
		}
		
		public void Stop() {
			if(mainForm!=null) {
		   		mainForm.Invoke(new MethodInvoker(mainForm.Hide));
		   		mainForm=null;
			}
			if( thread!=null) {
				thread.Abort();
				thread=null;
			}
		}
		
		public Form MainForm {
			get { return mainForm; }
		}
	}
}
