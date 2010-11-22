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
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;

using TickZoom.Api;

namespace TickZoom.Common
{
	public struct PhysicalOrderDefault : PhysicalOrder {
		private OrderState orderState;
		private SymbolInfo symbol;
		private OrderType type;
		private double price;
		private int size;
		private OrderSide side;
		private int logicalOrderId;
		private long logicalSerialNumber;
		private object brokerOrder;
		private object tag;
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(orderState);
			sb.Append(" ");
			sb.Append(side);
			sb.Append(" ");
			sb.Append(size);
			sb.Append(" ");
			sb.Append(type);
			sb.Append(" ");
			sb.Append(symbol);
			if( type != OrderType.BuyMarket && type != OrderType.SellMarket) {
				sb.Append(" at ");
				sb.Append(price);
			}
			sb.Append(" and logical id: ");
			sb.Append( logicalOrderId);
			sb.Append( "-");
			sb.Append( logicalSerialNumber);
			if( brokerOrder != null) {
				sb.Append(" broker: ");
				sb.Append( brokerOrder);
			}
			if( tag != null) {
				sb.Append(" ");
				sb.Append(tag);
			}
			return sb.ToString();
		}
		
		public PhysicalOrderDefault(OrderState orderState, SymbolInfo symbol, LogicalOrder logical, OrderSide side, int size) {
			this.orderState = orderState;
			this.symbol = symbol;
			this.side = side;
			this.type = logical.Type;
			this.price = logical.Price;
			this.size = size;
			this.logicalOrderId = logical.Id;
			this.logicalSerialNumber = logical.SerialNumber;
			this.tag = logical.Tag;
			this.brokerOrder = CreateBrokerOrderId(logicalOrderId);
		}
		
		public PhysicalOrderDefault(OrderState orderState, SymbolInfo symbol, OrderSide side, OrderType type, double price, int size, int logicalOrderId, long logicalSerialNumber, object brokerOrder, object tag) {
			this.orderState = orderState;
			this.symbol = symbol;
			this.side = side;
			this.type = type;
			this.price = price;
			this.size = size;
			this.logicalOrderId = logicalOrderId;
			this.logicalSerialNumber = logicalSerialNumber;
			this.tag = tag;
			this.brokerOrder = brokerOrder;
			if( this.brokerOrder == null) {
				this.brokerOrder = CreateBrokerOrderId(logicalOrderId);
			}
		}

//		private static long nextOrderId = 1000000000L;
		public static void ResetOrderId() {
//			nextOrderId = 1000000000L;
		}
		private static string CreateBrokerOrderId(int logicalId) {
			var longId = 0L;
		    longId = TimeStamp.UtcNow.Internal;
			return logicalId + "." + longId;
		}
		
		public OrderType Type {
			get { return type; }
		}
		
		public double Price {
			get { return price; }
		}
		
		public int Size {
			get { return size; }
			set { size = value; }
		}
		
		public object BrokerOrder {
			get { return brokerOrder; }
			set { brokerOrder = value; }
		}
		
		public SymbolInfo Symbol {
			get { return symbol; }
		}
		
		public int LogicalOrderId {
			get { return logicalOrderId; }
		}
		
		public OrderSide Side {
			get { return side; }
		}
		
		public OrderState OrderState {
			get { return orderState; }
			set { orderState = value; }
		}
		
		public object Tag {
			get { return tag; }
		}
		
		public long LogicalSerialNumber {
			get { return logicalSerialNumber; }
		}
	}
}