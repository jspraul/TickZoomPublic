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
using TickZoom.Api;

namespace TickZoom.Interceptors
{

	internal class FillManager : StrategyInterceptor
	{
		private static readonly Log log = Factory.SysLog.GetLogger("FillManager");
		private readonly bool trace = log.IsTraceEnabled;
		private bool postProcess = false;
		private FillHandler fillHandler;
		private StrategyInterface Strategy;
		private bool doStrategyOrders = true;
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
				fillHandler.ProcessFill(Strategy,(LogicalFill) eventDetail);
			}
			
			if( !postProcess) context.Invoke();
			
		}
		
		private void OnInitialize() {
			Strategy.AddInterceptor(EventType.LogicalFill, this);
			
			fillHandler = Factory.Utility.FillHandler(Strategy);
			fillHandler.ChangePosition = changePosition;
			fillHandler.DrawTrade = Strategy.Chart.DrawTrade;
			fillHandler.DoStrategyOrders = doStrategyOrders;
			fillHandler.DoExitStrategyOrders = doExitStrategyOrders;
		}
		
		public bool DoStrategyOrders {
			get { return doStrategyOrders; }
			set { doStrategyOrders = value; }
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
