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
using TickZoom.Common;

namespace TickZoom.Interceptors
{
	public class FillHandlerDefault : FillHandler
	{
		private static readonly Log Log = Factory.SysLog.GetLogger(typeof(FillHandlerDefault));
		private static readonly bool trace = Log.IsTraceEnabled;
		private static readonly bool debug = Log.IsDebugEnabled;
		private static readonly bool notice = Log.IsNoticeEnabled;
		private Action<SymbolInfo, LogicalFill> changePosition;
		private Func<LogicalOrder, double, double, int> drawTrade;
		private SymbolInfo symbol;
		private bool graphTrades = false;
		private bool doEntryOrders = true;
		private bool doExitOrders = true;
		private bool doExitStrategyOrders = false;
		
		public FillHandlerDefault()
		{
		}
	
		public FillHandlerDefault(StrategyInterface strategyInterface)
		{
			Strategy strategy = (Strategy) strategyInterface;
			graphTrades = strategy.Performance.GraphTrades;
		}
	
		private void TryDrawTrade(LogicalOrder order, double price, double position) {
			if (drawTrade != null && graphTrades == true) {
				drawTrade(order, price, position);
			}
		}
	
		public void ProcessFill(StrategyInterface strategyInterface, LogicalFill fill) {
			if( debug) Log.Debug( "Considering fill: " + fill + " for strategy " + strategyInterface);
			Strategy strategy = (Strategy) strategyInterface;
			bool cancelAllEntries = false;
			bool cancelAllExits = false;
			bool cancelAllExitStrategies = false;
			int orderId = fill.OrderId;
			LogicalOrder filledOrder = null;
			if( strategyInterface.TryGetOrderById( fill.OrderId, out filledOrder)) {
				if( debug) Log.Debug( "Matched fill with orderId: " + orderId);
				if( filledOrder.TradeDirection == TradeDirection.Entry && !doEntryOrders) {
					if( debug) Log.Debug( "Skipping fill, entry order fills disabled.");
					return;
				}
				if( filledOrder.TradeDirection == TradeDirection.Exit && !doExitOrders) {
					if( debug) Log.Debug( "Skipping fill, exit order fills disabled.");
					return;
				}
				if( filledOrder.TradeDirection == TradeDirection.Reverse && !doExitOrders) {
					if( debug) Log.Debug( "Skipping fill, reverse order fills disabled.");
					return;
				}
				if( filledOrder.TradeDirection == TradeDirection.ExitStrategy && !doExitStrategyOrders) {
					if( debug) Log.Debug( "Skipping fill, exit strategy orders fills disabled.");
					return;
				}
				TryDrawTrade(filledOrder, fill.Price, fill.Position);
				if( debug) Log.Debug( "Changing position because of fill");
				changePosition(strategy.Data.SymbolInfo,fill);
	
				bool clean = false;
				if( filledOrder.TradeDirection == TradeDirection.Entry &&
				   doEntryOrders ) {
					cancelAllEntries = true;
					clean = true;
				}
				if( filledOrder.TradeDirection == TradeDirection.Exit &&
				   doExitOrders ) {
					cancelAllExits = true;
					clean = true;
				}
				if( filledOrder.TradeDirection == TradeDirection.ExitStrategy &&
				   doExitStrategyOrders ) {
					cancelAllExitStrategies = true;
					clean = true;
				}
				if( clean) {
					var next = strategy.ActiveOrders.First;
					for( var node = next; node != null; node = next) {
						next = node.Next;
						LogicalOrder order = node.Value;
						if( order.TradeDirection == TradeDirection.Entry && cancelAllEntries) {
							order.Status = OrderStatus.Inactive;
						}
						if( order.TradeDirection == TradeDirection.Exit && cancelAllExits) {
							order.Status = OrderStatus.Inactive;
						}
						if( order.TradeDirection == TradeDirection.ExitStrategy && cancelAllExitStrategies) {
							order.Status = OrderStatus.Inactive;
						}
					}
				}
			} else {
				throw new ApplicationException("A fill for order id: " + orderId + " was incorrectly routed to: " + strategyInterface.Name);
			}
		}
		
		public Func<LogicalOrder, double, double, int> DrawTrade {
			get { return drawTrade; }
			set { drawTrade = value; }
		}
		
		public Action<SymbolInfo, LogicalFill> ChangePosition {
			get { return changePosition; }
			set { changePosition = value; }
		}
		
		public SymbolInfo Symbol {
			get { return symbol; }
			set { symbol = value; }
		}
		
		public bool GraphTrades {
			get { return graphTrades; }
			set { graphTrades = value; }
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
		
	}
}
