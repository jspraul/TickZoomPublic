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
using System.Diagnostics;
using System.Drawing;

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Interceptors
{
	public class ReverseCommon : StrategySupport
	{
		[Diagram(AttributeExclude=true)]
		public class InternalOrders {
			public LogicalOrder buyMarket;
			public LogicalOrder sellMarket;
			public LogicalOrder buyStop;
			public LogicalOrder sellStop;
			public LogicalOrder buyLimit;
			public LogicalOrder sellLimit;
		}
		private InternalOrders orders = new InternalOrders();
		
		private bool enableWrongSideOrders = false;
		private bool isNextBar = false;
		private int lotSize;
		
		public ReverseCommon(Strategy strategy) : base(strategy) {
		}
		
		public void OnInitialize()
		{
			if( IsDebug) Log.Debug("OnInitialize()");
			lotSize = Strategy.Data.SymbolInfo.Level2LotSize;
			Strategy.Drawing.Color = Color.Black;
			orders.buyMarket = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.buyMarket.Type = OrderType.BuyMarket;
			orders.buyMarket.TradeDirection = TradeDirection.Reverse;
			orders.sellMarket = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.sellMarket.Type = OrderType.SellMarket;
			orders.sellMarket.TradeDirection = TradeDirection.Reverse;
			orders.buyStop = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.buyStop.Type = OrderType.BuyStop;
			orders.buyStop.TradeDirection = TradeDirection.Reverse;
			orders.sellStop = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.sellStop.Type = OrderType.SellStop;
			orders.sellStop.TradeDirection = TradeDirection.Reverse;
			orders.buyLimit = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.buyLimit.Type = OrderType.BuyLimit;
			orders.buyLimit.TradeDirection = TradeDirection.Reverse;
			orders.sellLimit = Factory.Engine.LogicalOrder(Strategy.Data.SymbolInfo,Strategy);
			orders.sellLimit.Type = OrderType.SellLimit;
			orders.sellLimit.TradeDirection = TradeDirection.Reverse;
			Strategy.AddOrder( orders.buyMarket);
			Strategy.AddOrder( orders.sellMarket);
			Strategy.AddOrder( orders.buyStop);
			Strategy.AddOrder( orders.sellStop);
			Strategy.AddOrder( orders.buyLimit);
			Strategy.AddOrder( orders.sellLimit);
		}
		
	        public void CancelOrders()
	        {
	        	orders.buyMarket.IsActive = false;
	            orders.sellMarket.IsActive = false;
	        	orders.buyStop.IsActive = false;
	            orders.sellStop.IsActive = false;
	            orders.buyLimit.IsActive = false;
	            orders.sellLimit.IsActive = false;
	            
	        	orders.buyMarket.IsNextBar = false;
	            orders.sellMarket.IsNextBar = false;
	        	orders.buyStop.IsNextBar = false;
	            orders.sellStop.IsNextBar = false;
	            orders.buyLimit.IsNextBar = false;
	            orders.sellLimit.IsNextBar = false;
	        }
		
			private void LogEntry(string description) {
				if( Strategy.Chart.IsDynamicUpdate) {
		        		if( IsNotice) Log.Notice("Bar="+Strategy.Chart.DisplayBars.CurrentBar+", " + description);
				} else {
		        		if( IsDebug) Log.Debug("Bar="+Strategy.Chart.DisplayBars.CurrentBar+", " + description);
				}
			}
		
	        #region Properties		
	        public void SellMarket() {
	        	SellMarket(1);
	        }
	        
	        public void SellMarket( double lots) {
	        	orders.sellMarket.Price = 0;
	        	orders.sellMarket.Positions = lots * lotSize;
	        	if( isNextBar) {
	        	orders.sellMarket.IsNextBar = true;
	        	} else {
	        		orders.sellMarket.IsActive = true;
	        	}
	        }
	        
	        public void BuyMarket() {
	        	BuyMarket( 1);
	        }
	        
	        public void BuyMarket(double lots) {
	        	orders.buyMarket.Price = 0;
	        	orders.buyMarket.Positions = lots * lotSize;
	        	if( isNextBar) {
	        	orders.buyMarket.IsNextBar = true;
	        	} else {
	        		orders.buyMarket.IsActive = true;
	        	}
	        }
	        
	        public void BuyLimit( double price) {
	        	BuyLimit( price, 1);
	        }
	        	
	        /// <summary>
	        /// Create a active buy limit order.
	        /// </summary>
	        /// <param name="price">Order price.</param>
	        /// <param name="positions">Number of positions as in 1, 2, 3, etc. To set the size of a single position, 
	        ///  use PositionSize.Size.</param>
	
	        public void BuyLimit( double price, double lots) {
	        	orders.buyLimit.Price = price;
	        	orders.buyLimit.Positions = lots * lotSize;
	        	if( isNextBar) {
	        	orders.buyLimit.IsNextBar = true;
	        	} else {
	        		orders.buyLimit.IsActive = true;
	        	}
			}
	        
	        public void SellLimit( double price) {
	        	SellLimit( price, 1);
	        }
	        	
	        /// <summary>
	        /// Create a active sell limit order.
	        /// </summary>
	        /// <param name="price">Order price.</param>
	        /// <param name="positions">Number of positions as in 1, 2, 3, etc. To set the size of a single position, 
	        ///  use PositionSize.Size.</param>
	
	        public void SellLimit( double price, double lots) {
	        	orders.sellLimit.Price = price;
	        	orders.sellLimit.Positions = lots * lotSize;
	        	if( isNextBar) {
	        	orders.sellLimit.IsNextBar = true;
	        	} else {
	        		orders.sellLimit.IsActive = true;
	        	}
			}
	        
	        public void BuyStop( double price) {
	        	BuyStop( price, 1);
	        }
	        
	        /// <summary>
	        /// Create a active buy stop order.
	        /// </summary>
	        /// <param name="price">Order price.</param>
	        /// <param name="positions">Number of positions as in 1, 2, 3, etc. To set the size of a single position, 
	        ///  use PositionSize.Size.</param>
	
	        public void BuyStop( double price, double lots) {
	        	orders.buyStop.Price = price;
	        	orders.buyStop.Positions = lots * lotSize;
	        	if( isNextBar) {
	        	orders.buyStop.IsNextBar = true;
	        	} else {
	        		orders.buyStop.IsActive = true;
	        	}
			}
	
	        public void SellStop( double price) {
	        	SellStop( price, 1);
	        }
	        
	        /// <summary>
	        /// Create a active sell stop order.
	        /// </summary>
	        /// <param name="price">Order price.</param>
	        /// <param name="positions">Number of positions as in 1, 2, 3, etc. To set the size of a single position, 
	        ///  use PositionSize.Size.</param>
	        
	        public void SellStop( double price, double lots) {
	        	orders.sellStop.Price = price;
	        	orders.sellStop.Positions = lots * lotSize;
	        	if( isNextBar) {
	        	orders.sellStop.IsNextBar = true;
	        	} else {
	        		orders.sellStop.IsActive = true;
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
	
		public bool HasBuyOrder {
			get {
				return orders.buyStop.IsActive || orders.buyStop.IsNextBar || 
					orders.buyLimit.IsActive || orders.buyLimit.IsNextBar ||
					orders.buyMarket.IsActive || orders.buyMarket.IsNextBar;
			}
		}
		
		public bool HasSellOrder {
			get {
				return orders.sellStop.IsActive || orders.sellStop.IsNextBar || 
					orders.sellLimit.IsActive || orders.sellLimit.IsNextBar || 
					orders.sellMarket.IsActive || orders.sellMarket.IsNextBar;
			}
		}
		
		internal InternalOrders Orders {
			get { return orders; }
			set { orders = value; }
		}
		
		internal bool IsNextBar {
			get { return isNextBar; }
			set { isNextBar = value; }
		}
	}
}
