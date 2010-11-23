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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using TickZoom.Api;

namespace TickZoom
{
	/// <summary>
	/// Description of PortfolioDoc.
	/// </summary>
	public partial class PortfolioDoc : Form
	{
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SynchronizationContext context;
        private Action<Action> callbackAction = (action) => { action(); };
        
		public Action<Action> CallbackAction {
			get { return callbackAction; }
			set { callbackAction = value; }
		}
        
		public PortfolioDoc()
		{
			context = SynchronizationContext.Current;
            if(context == null)
            {
                context = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(context);
            }
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		public ChartControl ChartControl {
			get { return chartControl1; } 
		}
		
		void PortfolioDocLoad(object sender, System.EventArgs e)
		{
//			chartControl1.ChartLoad(sender,e);
		}
		
		void PortfolioDocResize(object sender, EventArgs e)
		{
			chartControl1.Size = new Size( ClientRectangle.Width, ClientRectangle.Height);
		}
		

		public new void Show() {
			ShowInvoke();
		}
		
//			if( InvokeRequired) {
//				BeginInvoke(new MethodInvoker(Show));
//			} else {
//				base.Show();
//			}

		public void ShowInvoke() {
       		context.Send(new SendOrPostCallback(
       		delegate(object state)
       	    {
				base.Show();       		
       		}), null);
		}
		
//			if( InvokeRequired) {
//				BeginInvoke(new MethodInvoker(Hide));
//			} else {
//				Hide();
//			}
		
		public void HideInvoke() {
       		context.Send(new SendOrPostCallback(
       		delegate(object state)
       	    {
				base.Hide();       		
       		}), null);
		}
		
	    public delegate void ShowDelegate();
	    public delegate void HideDelegate();
	}
}
