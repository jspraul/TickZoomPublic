#region Copyright
/*
 * Copyright 2008 M. Wayne Walter
 * Software: TickZoom Trading Platform
 * User: Wayne Walter
 * 
 * You can use and modify this software under the terms of the
 * TickZOOM General Public License Version 1.0 or (at your option)
 * any later version.
 * 
 * Businesses are restricted to 30 days of use.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * TickZOOM General Public License for more details.
 *
 * You should have received a copy of the TickZOOM General Public
 * License along with this program.  If not, see
 * 
 * 
 *
 * User: Wayne Walter
 * Date: 5/18/2009
 * Time: 12:54 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Collections.Generic;
using TickZoom.Api;

namespace TickZoom.Interceptors
{
	internal class FillManager : StrategyInterceptor
	{
		private static readonly Log log = Factory.SysLog.GetLogger("OrderManager");
		private static readonly bool trace = log.IsTraceEnabled;
		private bool postProcess = false;
		private FillHandler fillHandler;
		private StrategyInterface Strategy;
		private bool doEntryOrders = true;
		private bool doExitOrders = true;
		private bool doExitStrategyOrders = false;
		private Action<SymbolInfo,LogicalFill> changePosition;
		
		internal FillManager(StrategyInterface strategy) {
			this.Strategy = strategy;
			this.changePosition = strategy.Position.Change;
		}
		
		public override void Intercept(EventContext context, EventType eventType, object eventDetail)
		{
			if( eventType == EventType.Initialize) {
				OnInitialize();
			}
			if( postProcess) context.Invoke();
			
			if( eventType == EventType.LogicalFill) {
				fillHandler.ProcessFill(Strategy,(LogicalFill)eventDetail);
			}
			
			if( !postProcess) context.Invoke();
			
		}
		
		private void OnInitialize() {
			Strategy.AddInterceptor(EventType.LogicalFill, this);
			
			fillHandler = Factory.Utility.FillHandler(Strategy);
			fillHandler.ChangePosition = changePosition;
			fillHandler.DrawTrade = Strategy.Chart.DrawTrade;
			fillHandler.DoEntryOrders = doEntryOrders;
			fillHandler.DoExitOrders = doExitOrders;
			fillHandler.DoExitStrategyOrders = doExitStrategyOrders;
		}
		
		public bool DoEntryOrders {
			get { return doEntryOrders; }
			set { doEntryOrders = value; }
		}
		
		public bool DoExitOrders {
			get { return doExitOrders; }
			set { doExitOrders = value; }
		}
		
		public bool DoExitStrategyOrders {
			get { return doExitStrategyOrders; }
			set { doExitStrategyOrders = value; }
		}
		
		public Action<SymbolInfo, LogicalFill> ChangePosition {
			get { return changePosition; }
			set { changePosition = value; }
		}
		
		public bool PostProcess {
			get { return postProcess; }
			set { postProcess = value; }
		}
		
	}
}
