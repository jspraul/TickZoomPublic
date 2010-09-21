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
using System.Threading;

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Interceptors
{
	public class FillSimulatorPhysical : PhysicalFillSimulator
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(FillSimulatorPhysical));
		private static readonly bool trace = log.IsTraceEnabled;
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool notice = log.IsNoticeEnabled;
		private Dictionary<long,PhysicalOrder> physicalOrders = new Dictionary<long,PhysicalOrder>();
		private List<PhysicalOrder> filledOrders = new List<PhysicalOrder>();
		private Action<LogicalFillBinary> createLogicalFill;
		private bool useSyntheticMarkets = true;
		private bool useSyntheticStops = true;
		private bool useSyntheticLimits = true;
		private SymbolInfo symbol;
		private double position = 0D;
		
		public FillSimulatorPhysical(SymbolInfo symbol)
		{
			this.symbol = symbol;
		}
	
		public FillSimulatorPhysical(Func<double> getActualPosition, StrategyInterface strategyInterface)
		{
			Strategy strategy = (Strategy) strategyInterface;
		}
		
		private long nextOrderId = 1000;
		public void OnChangeBrokerOrder(PhysicalOrder order)
		{
			if( debug) log.Debug("OnChangeBrokerOrder( " + order + ")");
			var orderId = (long) order.BrokerOrder;
			if( !physicalOrders.Remove(orderId)) {
				throw new ApplicationException("Order id " + orderId + " was not found to change order: " + order);
			} else {
				physicalOrders.Add( orderId, order);
			}
		}
		
		public void OnCreateBrokerOrder(PhysicalOrder order)
		{
			if( debug) log.Debug("OnCreateBrokerOrder( " + order + ")");
			var orderId = Interlocked.Increment(ref nextOrderId);
			order.BrokerOrder = orderId;
			physicalOrders.Add(orderId,order);
		}
		
		public void OnCancelBrokerOrder(PhysicalOrder order)
		{
			if( debug) log.Debug("OnCancelBrokerOrder( " + order + ")");
			var orderId = (long) order.BrokerOrder;
			if( !physicalOrders.Remove(orderId)) {
				throw new ApplicationException("Order id " + orderId + " was not found to change order: " + order);
			}
		}
		
		public bool ProcessOrders(Tick tick)
		{
			bool retVal = false;
			if( symbol == null) {
				throw new ApplicationException("Please set the Symbol property for the " + GetType().Name + ".");
			}
			filledOrders.Clear();
			foreach( var kvp in physicalOrders) {
				var order = kvp.Value;
				OnProcessOrder(order, tick);
			}
			foreach( var order in filledOrders) {
				physicalOrders.Remove((long) order.BrokerOrder);
			}
			return retVal;
		}
		
	#region ExitOrder
	
		private bool OnProcessOrder(PhysicalOrder order, Tick tick)
		{
			bool retVal = false;
			if (trace) log.Trace("OnProcessOrder()");
	
			switch (order.Type) {
				case OrderType.SellMarket:
					if( ProcessSellMarket(order, tick)) {
						retVal = true;
					}
					break;
				case OrderType.SellStop:
					if( ProcessSellStop(order, tick)) {
						retVal = true;
					}
					break;
				case OrderType.SellLimit:
					if( ProcessSellLimit(order, tick)) {
						retVal = true;
					}
					break;
				case OrderType.BuyMarket:
					if( ProcessBuyMarket(order, tick)) {
						retVal = true;
					}
					break;
				case OrderType.BuyStop:
					if( ProcessBuyStop(order, tick)) {
						retVal = true;
					}
					break;
				case OrderType.BuyLimit:
					if( ProcessBuyLimit(order, tick)) {
						retVal = true;
					}
					break;
			}
			return retVal;
		}
		
		private bool ProcessBuyStop(PhysicalOrder order, Tick tick)
		{
			bool retVal = true;
			double price = tick.IsTrade ? tick.Price : tick.Ask;
			if (price >= order.Price) {
				CreateLogicalFillHelper(order.Size, price, tick.Time, order);
				retVal = true;
			}
			return retVal;
		}

		private bool ProcessSellStop(PhysicalOrder order, Tick tick)
		{
			bool retVal = true;
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			if (price <= order.Price) {
				CreateLogicalFillHelper(-order.Size, price, tick.Time, order);
				retVal = true;
			}
			return retVal;
		}

		private bool ProcessBuyMarket(PhysicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			CreateLogicalFillHelper(order.Size, price, tick.Time, order);
			return true;
		}

		private bool ProcessBuyLimit(PhysicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Ask : tick.Price;
			bool isFilled = false;
			if (price <= order.Price) {
				isFilled = true;
			} else if (tick.IsTrade && tick.Price < order.Price) {
				price = order.Price;
				isFilled = true;
			}
			if (isFilled) {
				CreateLogicalFillHelper(order.Size, price, tick.Time, order);
			}
			return isFilled;
		}

		private bool ProcessSellMarket(PhysicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Bid : tick.Price;
			CreateLogicalFillHelper(-order.Size, price, tick.Time, order);
			return true;
		}

		private bool ProcessSellLimit(PhysicalOrder order, Tick tick)
		{
			double price = tick.IsQuote ? tick.Bid : tick.Price;
			bool isFilled = false;
			if (price >= order.Price) {
				isFilled = true;
			} else if (tick.IsTrade && tick.Price > order.Price) {
				price = order.Price;
				isFilled = true;
			}
			if (isFilled) {
				CreateLogicalFillHelper(-order.Size, price, tick.Time, order);
			}
			return isFilled;
		}
		
	#endregion
		
		private void CreateLogicalFillHelper(double position, double price, TimeStamp time, PhysicalOrder order) {
			if( debug) log.Debug("Filled: " + order);
			this.position += position;
			filledOrders.Add(order);
			LogicalFillBinary fill = new LogicalFillBinary(position,price,time, order.LogicalOrderId);
			if( debug) log.Debug("Fill price: " + fill);
			createLogicalFill(fill);
		}
		
		public bool UseSyntheticLimits {
			get { return useSyntheticLimits; }
			set { useSyntheticLimits = value; }
		}
		
		public bool UseSyntheticStops {
			get { return useSyntheticStops; }
			set { useSyntheticStops = value; }
		}
		
		public bool UseSyntheticMarkets {
			get { return useSyntheticMarkets; }
			set { useSyntheticMarkets = value; }
		}
		
		public Action<LogicalFillBinary> CreateLogicalFill {
			get { return createLogicalFill; }
			set { createLogicalFill = value; }
		}
		
		public Dictionary<long, PhysicalOrder> PhysicalOrders {
			get { return physicalOrders; }
		}
		
		public double Position {
			get { return position; }
		}
	}
}
