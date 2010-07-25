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
using System.Threading;
using System.Windows.Forms;

using TickZoom;
using TickZoom.Api;

namespace Loaders
{

	
	public class ChartThread {
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private PortfolioDoc portfolioDoc;
		public Thread thread;
		public ChartThread() {
			log.Debug("Starting Chart Thread");
			ThreadStart job = new ThreadStart(Run);
			thread = new Thread(job);
			thread.IsBackground = true;
			thread.Name = "ChartTest";
			thread.Start();
			while( portfolioDoc == null) {
				Thread.Sleep(0);
			}
			log.Debug("Returning Chart Created by Thread");
		}
		
		private void ClosedEvent(object sender, EventArgs args) {
			stop = true;
		}
		
		public void Run() {
			try {
   				log.Debug("Chart Thread Started");
   				portfolioDoc = new PortfolioDoc();
   				portfolioDoc.Closed += ClosedEvent;
   				while( !stop ) {
   					Application.DoEvents();
   					Thread.Sleep(10);
   				}
   				stop = true;
			} catch( Exception ex) {
				log.Error("ERROR: Thread had an exception:",ex);
			}
		}
		
		private bool stop = false;
		
		public void Stop() {
			if(portfolioDoc!=null) {
		   		portfolioDoc.Invoke(new MethodInvoker(portfolioDoc.Hide));
		   		portfolioDoc=null;
			}
			if( thread!=null) {
				stop = true;
				thread.Join();
			}
		}
		
		public bool IsAlive {
			get { return !stop; }
		}
		
		public PortfolioDoc PortfolioDoc {
			get { return portfolioDoc; }
		}
	}
}
