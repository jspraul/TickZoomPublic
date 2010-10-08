﻿#region Copyright
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

	public class FillSimulatorPhysical : FillSimulator
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(FillSimulatorPhysical));
		private static readonly bool trace = log.IsTraceEnabled;
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool notice = log.IsNoticeEnabled;

		private Dictionary<string,PhysicalOrder> orderMap = new Dictionary<string, PhysicalOrder>();
		private ActiveList<PhysicalOrder> increaseOrders = new ActiveList<PhysicalOrder>();
		private ActiveList<PhysicalOrder> decreaseOrders = new ActiveList<PhysicalOrder>();
		private ActiveList<PhysicalOrder> marketOrders = new ActiveList<PhysicalOrder>();
		private NodePool<PhysicalOrder> nodePool = new NodePool<PhysicalOrder>();

		private Action<PhysicalFill> onPhysicalFill;
		private Action<double> onPositionChange;
		private bool useSyntheticMarkets = true;
		private bool useSyntheticStops = true;
		private bool useSyntheticLimits = true;
		private SymbolInfo symbol;
		private double actualPosition = 0D;
		private bool isChanged = false;
		private TickSync tickSync;
		private TickIO lastTick = Factory.TickUtil.TickIO();
		
		public FillSimulatorPhysical(SymbolInfo symbol)
		{
			this.symbol = symbol;
			this.tickSync = SyncTicks.GetTickSync(symbol.BinaryIdentifier);
		}
		
		public Iterable<PhysicalOrder> GetActiveOrders(SymbolInfo symbol) {
			ActiveList<PhysicalOrder> activeOrders = new ActiveList<PhysicalOrder>();
			activeOrders.AddLast(increaseOrders);
			activeOrders.AddLast(decreaseOrders);
			activeOrders.AddLast(marketOrders);
			return activeOrders;
		}
	
		public void OnChangeBrokerOrder(PhysicalOrder order, object origBrokerOrder)
		{
			if( debug) log.Debug("OnChangeBrokerOrder( " + order + ")");
			CancelBrokerOrder( origBrokerOrder);
			CreateBrokerOrder( order);
			LogOpenOrders();
		}
		
		public PhysicalOrder GetOrderById( string orderId) {
			LogOpenOrders();
			PhysicalOrder order;
			if( !orderMap.TryGetValue( orderId, out order)) {
				throw new ApplicationException( symbol + ": Cannot find physical order by id: " + orderId);
			}
			return order;
		}
		
		private void CancelBrokerOrder(object origBrokerOrder) {
			IsChanged = true;
			var oldOrderId = (string) origBrokerOrder;
			var oldOrder = GetOrderById(oldOrderId);
			RemoveActive( oldOrder);
			orderMap.Remove( oldOrderId);
			LogOpenOrders();
		}
		
		private void CreateBrokerOrder(PhysicalOrder order) {
			isChanged = true;
			orderMap.Add((string)order.BrokerOrder,order);
			SortAdjust(order);
		}
		
		public void OnCreateBrokerOrder(PhysicalOrder order)
		{
			if( debug) log.Debug("OnCreateBrokerOrder( " + order + ")");
			CreateBrokerOrder(order);
		}
		
		public void OnCancelBrokerOrder(object origBrokerOrder)
		{
			if( debug) log.Debug("OnCancelBrokerOrder( " + origBrokerOrder + ")");
			CancelBrokerOrder(origBrokerOrder);
		}
		
		public void ReprocessOrders() {
			ProcessOrdersInternal( lastTick);
		}
		
		public void ProcessOrders(Tick tick)
		{
			lastTick.Inject( ((TickIO) tick).Extract());
			ProcessOrdersInternal( tick);
		}

		private void ProcessOrdersInternal(Tick tick) {
			var result = false;
			if( trace) log.Trace( "ProcessOrders( " + symbol + ", " + tick + " )") ;
			if( symbol == null) {
				throw new ApplicationException("Please set the Symbol property for the " + GetType().Name + ".");
			}
			var next = marketOrders.First;
			for( var node = next; node != null; node = node.Next) {
				var order = node.Value;
				if( OnProcessOrder(order, tick)) {
					result = true;
				}
			}
			next = increaseOrders.First;
			for( var node = next; node != null; node = node.Next) {
				var order = node.Value;
				if( OnProcessOrder(order, tick)) {
					result = true;
				}
			}
			next = decreaseOrders.First;
			for( var node = next; node != null; node = node.Next) {
				var order = node.Value;
				if( OnProcessOrder(order, tick)) {
					result = true;
				}
			}
			tickSync.SentFills = result;
			
		}
		
		private void LogOpenOrders() {
			if( debug) {
				log.Debug( "Found " + orderMap.Count + " open orders for " + symbol + ":");
				foreach( var kvp in orderMap) {
					var order = kvp.Value;
					log.Debug( order.ToString());
				}
			}
		}
		
		private void SortAdjust(PhysicalOrder order) {
			switch( order.Type) {
				case OrderType.BuyLimit:					
				case OrderType.SellStop:
					SortAdjust( decreaseOrders, order, (x,y) => y.Price - x.Price);
					break;
				case OrderType.SellLimit:
				case OrderType.BuyStop:
					SortAdjust( increaseOrders, order, (x,y) => x.Price - y.Price);
					break;
				case OrderType.BuyMarket:
				case OrderType.SellMarket:
					Adjust( marketOrders, order);
					break;
				default:
					throw new ApplicationException("Unexpected order type: " + order.Type);
			}
		}
		
		private void RemoveActive(PhysicalOrder order) {
			Remove( increaseOrders, order);
			Remove( decreaseOrders, order);
			Remove( marketOrders, order);
		}
		
		private void Adjust(ActiveList<PhysicalOrder> list, PhysicalOrder order) {
			if( !list.Contains(order)) {
				var node = nodePool.Create(order);
				list.AddLast(node);
			}
		}
		
		private void Remove(ActiveList<PhysicalOrder> list, PhysicalOrder order) {
			var node = list.Find(order);
			if( node != null) {
				list.Remove(node);
				nodePool.Free(node);
			}
		}
		
		private void SortAdjust(ActiveList<PhysicalOrder> list, PhysicalOrder order, Func<PhysicalOrder,PhysicalOrder,double> compare) {
			if( !list.Contains(order)) {
				var newNode = nodePool.Create(order);
				bool found = false;
				var next = list.First;
				for( var node = next; node != null; node = next) {
					next = node.Next;
					var other = node.Value;
					if( object.ReferenceEquals(order,other)) {
						found = true;
						break;
					} else {
						var result = compare(order,other);
						if( result < 0) {
							list.AddBefore(node,newNode);
							found = true;
							break;
						}
					}
				}
				if( !found) {
					list.AddLast(newNode);
				}
			}
		}
		
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
			bool retVal = false;
			double price = tick.IsTrade ? tick.Price : tick.Ask;
			if (price >= order.Price) {
				CreateLogicalFillHelper(order.Size, price, tick.Time, order);
				retVal = true;
			}
			return retVal;
		}

		private bool ProcessSellStop(PhysicalOrder order, Tick tick)
		{
			bool retVal = false;
			double price;
			price = tick.IsQuote ? tick.Bid : tick.Price;
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
		
		private void CreateLogicalFillHelper(double size, double price, TimeStamp time, PhysicalOrder order) {
			this.actualPosition += size;
			if( onPositionChange != null) {
				onPositionChange( actualPosition);
			}
			if( debug) log.Debug("Filled order: " + order );
			CancelBrokerOrder(order.BrokerOrder);
			var fill = new PhysicalFillDefault(size,price,time,order);
			if( debug) log.Debug("Fill: " + fill );
			if( onPhysicalFill == null) {
				throw new ApplicationException("Please set the OnPhysicalFill property.");
			} else {
				onPhysicalFill(fill);
			}
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
		
		public Action<PhysicalFill> OnPhysicalFill {
			get { return onPhysicalFill; }
			set { onPhysicalFill = value; }
		}
		
		public double GetActualPosition(SymbolInfo symbol) {
			return actualPosition;
		}
		
		public bool IsChanged {
			get { return isChanged; }
			set { isChanged = value; }
		}
		
		public double ActualPosition {
			get { return actualPosition; }
			set { actualPosition = value; }
		}
		
		public Action<double> OnPositionChange {
			get { return onPositionChange; }
			set { onPositionChange = value; }
		}
	}
}
