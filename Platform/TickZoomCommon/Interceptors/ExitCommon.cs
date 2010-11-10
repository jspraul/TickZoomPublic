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
using System.Diagnostics;
using System.Drawing;

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Interceptors
{
	/// <summary>
	/// Description of StrategySupport.
	/// </summary>
	public class ExitCommon : StrategySupport
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(ExitCommon));
		[Diagram(AttributeExclude=true)]
		public class InternalOrders {
			public LogicalOrder buyMarket;
			public LogicalOrder sellMarket;
			public LogicalOrder buyStop;
			public LogicalOrder sellStop;
			public LogicalOrder buyLimit;
			public LogicalOrder sellLimit;
		}
		private PositionInterface position;
		private InternalOrders orders = new InternalOrders();
		
		private bool enableWrongSideOrders = false;
		private bool isNextBar = false;
		
		public ExitCommon(Strategy strategy) : base(strategy) {
		}
		
		public void OnInitialize()
		{
			if( IsTrace) Log.Trace(Strategy.FullName+".Initialize()");
			Strategy.Drawing.Color = Color.Black;
			orders.buyMarket = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.buyMarket.TradeDirection = TradeDirection.Exit;
			orders.buyMarket.Type = OrderType.BuyMarket;
			orders.sellMarket = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.sellMarket.TradeDirection = TradeDirection.Exit;
			orders.sellMarket.Type = OrderType.SellMarket;
			orders.buyStop = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.buyStop.Type = OrderType.BuyStop;
			orders.buyStop.TradeDirection = TradeDirection.Exit;
			orders.sellStop = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.sellStop.Type = OrderType.SellStop;
			orders.sellStop.TradeDirection = TradeDirection.Exit;
			orders.sellStop.Tag = "ExitCommon";
			orders.buyLimit = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.buyLimit.Type = OrderType.BuyLimit;
			orders.buyLimit.TradeDirection = TradeDirection.Exit;
			orders.sellLimit = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.sellLimit.Type = OrderType.SellLimit;
			orders.sellLimit.TradeDirection = TradeDirection.Exit;
			Strategy.AddOrder( orders.buyMarket);
			Strategy.AddOrder( orders.sellMarket);
			Strategy.AddOrder( orders.buyStop);
			Strategy.AddOrder( orders.sellStop);
			Strategy.AddOrder( orders.buyLimit);
			Strategy.AddOrder( orders.sellLimit);
			position = Strategy.Position;
		}

		private void FlattenSignal(double price) {
			Strategy.Position.Change(0,price,Strategy.Ticks[0].Time);
			CancelOrders();
		}
	
		public void CancelOrders() {
			orders.buyStop.Status = OrderStatus.AutoCancel;
			orders.sellStop.Status = OrderStatus.AutoCancel;
			orders.buyLimit.Status = OrderStatus.AutoCancel;
			orders.sellLimit.Status = OrderStatus.AutoCancel;
		}
		
        #region Orders

        public void GoFlat() {
        	if( Strategy.Position.IsLong) {
	        	orders.sellMarket.Price = 0;
	        	orders.sellMarket.Positions = Strategy.Position.Size;
	        	if( isNextBar) {
	    	    	orders.sellMarket.Status = OrderStatus.NextBar;
		       	} else {
		        	orders.sellMarket.Status = OrderStatus.Active;
	        	}
        	}
        	if( Strategy.Position.IsShort) {
	        	orders.buyMarket.Price = 0;
	        	orders.buyMarket.Positions = Strategy.Position.Size;
	        	if( isNextBar) {
	    	    	orders.buyMarket.Status = OrderStatus.NextBar;
		       	} else {
		        	orders.buyMarket.Status = OrderStatus.Active;
	        	}
        	}
		}
	
        public void BuyStop(double price) {
        	if( Strategy.Position.IsLong) {
        		throw new TickZoomException("Strategy must be short or flat before setting a buy stop to exit.");
        	} else if( Strategy.Position.IsFlat) {
        		if(!Strategy.Orders.Enter.ActiveNow.HasSellOrder) {
        			throw new TickZoomException("When flat, a sell order must be active before creating a buy order to exit.");
        		}
			}
    		orders.buyStop.Price = price;
        	if( isNextBar) {
    	    	orders.buyStop.Status = OrderStatus.NextBar;
	       	} else {
	        	orders.buyStop.Status = OrderStatus.Active;
        	}
        }
	
        public void SellStop( double price) {
        	if( Strategy.Position.IsShort) {
        		throw new TickZoomException("Strategy must be long or flat before setting a sell stop to exit.");
        	} else if( Strategy.Position.IsFlat) {
        		if(!Strategy.Orders.Enter.ActiveNow.HasBuyOrder) {
        			throw new TickZoomException("When flat, a buy order must be active before creating a sell order to exit.");
        		}
        	}
			orders.sellStop.Price = price;
        	if( isNextBar) {
    	    	orders.sellStop.Status = OrderStatus.NextBar;
	       	} else {
	        	orders.sellStop.Status = OrderStatus.Active;
        	}
		}
        
        public void BuyLimit(double price) {
        	if( Strategy.Position.IsLong) {
        		throw new TickZoomException("Strategy must be short or flat before setting a buy limit to exit.");
        	} else if( Strategy.Position.IsFlat) {
        		if(!Strategy.Orders.Enter.ActiveNow.HasSellOrder) {
        			throw new TickZoomException("When flat, a sell order must be active before creating a buy order to exit.");
        		}
			}
    		orders.buyLimit.Price = price;
        	if( isNextBar) {
    	    	orders.buyLimit.Status = OrderStatus.NextBar;
	       	} else {
	        	orders.buyLimit.Status = OrderStatus.Active;
        	}
		}
	
        public void SellLimit( double price) {
        	if( Strategy.Position.IsShort) {
        		throw new TickZoomException("Strategy must be long or flat before setting a sell limit to exit.");
        	} else if( Strategy.Position.IsFlat) {
        		if(!Strategy.Orders.Enter.ActiveNow.HasBuyOrder) {
        			throw new TickZoomException("When flat, a buy order must be active before creating a sell order to exit.");
        		}
			}
			orders.sellLimit.Price = price;
        	if( isNextBar) {
    	    	orders.sellLimit.Status = OrderStatus.NextBar;
	       	} else {
	        	orders.sellLimit.Status = OrderStatus.Active;
        	}
		}
        
		#endregion

		
		public override string ToString()
		{
			return Strategy.FullName;
		}
		
		public bool EnableWrongSideOrders {
			get { return enableWrongSideOrders; }
			set { enableWrongSideOrders = value; }
		}
		
		internal bool IsNextBar {
			get { return isNextBar; }
			set { isNextBar = value; }
		}
		
		internal InternalOrders Orders {
			get { return orders; }
			set { orders = value; }
		}
	}
}
