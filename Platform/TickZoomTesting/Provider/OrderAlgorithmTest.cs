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

using NUnit.Framework;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Interceptors;

namespace Orders
{
	public class MockContext : Context {
		int modelId = 0;
		int logicalOrderId = 0;
		long logicalOrderSerialNumber = 0;
		public BinaryStore TradeData {
			get { throw new NotImplementedException(); }
		}
		public void AddOrder(LogicalOrder order)
		{ throw new NotImplementedException(); }
		public int IncrementOrderId() {
			return Interlocked.Increment(ref logicalOrderId);
		}
		public long IncrementOrderSerialNumber() {
			return Interlocked.Increment(ref logicalOrderSerialNumber);
		}
		public int IncrementModelId() {
			return Interlocked.Increment(ref modelId);
		}
	}
	[TestFixture]
	public class OrderAlgorithmTest {
		SymbolInfo symbol = Factory.Symbol.LookupSymbol("CSCO");
		ActiveList<LogicalOrder> orders = new ActiveList<LogicalOrder>();
		TestOrderAlgorithm handler;
		Strategy strategy;
		
		public OrderAlgorithmTest() {
			strategy = new Strategy();
			strategy.Context = new MockContext();
			handler = new TestOrderAlgorithm(symbol,strategy);
		}
		
		[SetUp]
		public void Setup() {
			orders.Clear();
		}
		
		public int CreateLogicalEntry(OrderType type, double price, int size) {
			LogicalOrder logical = Factory.Engine.LogicalOrder(symbol,strategy);
			logical.Status = OrderStatus.Active;
			logical.TradeDirection = TradeDirection.Entry;
			logical.Type = type;
			logical.Price = price;
			logical.Position = size;
			orders.AddLast(logical);
			return logical.Id;
		}
		
		public int CreateLogicalExit(OrderType type, double price) {
			LogicalOrder logical = Factory.Engine.LogicalOrder(symbol,strategy);
			logical.Status = OrderStatus.Active;
			logical.TradeDirection = TradeDirection.Exit;
			logical.Type = type;
			logical.Price = price;
			orders.AddLast(logical);
			return logical.Id;
		}
		
		[Test]
		public void Test01FlatZeroOrders() {
			handler.Clear();
			
			int buyLimitId = CreateLogicalEntry(OrderType.BuyLimit,234.12,1000);
			int buyStopId = CreateLogicalEntry(OrderType.SellStop,154.12,1000);
			CreateLogicalExit(OrderType.SellLimit,334.12);
			CreateLogicalExit(OrderType.SellStop,134.12);
			CreateLogicalExit(OrderType.BuyLimit,124.12);
			CreateLogicalExit(OrderType.BuyStop,194.12);
			
			var position = 0;
			handler.SetActualPosition(position);
			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(2,handler.Orders.CreatedOrders.Count);
			
			PhysicalOrder order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.BuyLimit,order.Type);
			Assert.AreEqual(234.12,order.Price);
			Assert.AreEqual(1000,order.Size);
			Assert.AreEqual(buyLimitId,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);

			order = handler.Orders.CreatedOrders[1];
			Assert.AreEqual(OrderType.SellStop,order.Type);
			Assert.AreEqual(154.12,order.Price);
			Assert.AreEqual(1000,order.Size);
			Assert.AreEqual(buyStopId,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
		}
		
		private void AssertBrokerOrder( object brokerOrder) {
			Assert.True( brokerOrder is string, "is string");
			var brokerOrderId = (string) brokerOrder;
			Assert.True( brokerOrderId.Contains("."));
		}
		
		[Test]
		public void Test02FlatTwoOrders() {
			handler.Clear();
			
			int buyLimitId = CreateLogicalEntry(OrderType.BuyLimit,234.12,1000);
			int sellStopId = CreateLogicalEntry(OrderType.SellStop,154.12,1000);
			CreateLogicalExit(OrderType.SellLimit,334.12);
			CreateLogicalExit(OrderType.SellStop,134.12);
			CreateLogicalExit(OrderType.BuyLimit,124.12);
			CreateLogicalExit(OrderType.BuyStop,194.12);
			
			object buyOrder = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyLimit,234.12,1000,buyLimitId,buyOrder);
			object sellOrder = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellStop,154.12,1000,sellStopId,sellOrder);
			
			var position = 0;
			handler.SetActualPosition(position);
			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
		}
		
		[Test]
		public void Test03LongEntryFilled() {
			handler.Clear();
			
			
			CreateLogicalEntry(OrderType.BuyLimit,234.12,1000);
			int sellStopId = CreateLogicalEntry(OrderType.SellStop,154.12,1000);
			int sellLimitId = CreateLogicalExit(OrderType.SellLimit,334.12);
			int sellStop2Id = CreateLogicalExit(OrderType.SellStop,134.12);
			CreateLogicalExit(OrderType.BuyLimit,124.12);
			CreateLogicalExit(OrderType.BuyStop,194.12);
			
			object sellOrder = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Sell,OrderType.SellStop,154.12,1000,sellStopId,sellOrder);
			
			var position = 1000;
			handler.SetActualPosition(position);
			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(1,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(2,handler.Orders.CreatedOrders.Count);
			
			var brokerOrder = handler.Orders.CanceledOrders[0];
			Assert.AreEqual(sellOrder,brokerOrder);
			
			var order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.SellLimit,order.Type);
			Assert.AreEqual(334.12,order.Price);
			Assert.AreEqual(1000,order.Size);
			Assert.AreEqual(sellLimitId,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
			
			order = handler.Orders.CreatedOrders[1];
			Assert.AreEqual(OrderType.SellStop,order.Type);
			Assert.AreEqual(134.12,order.Price);
			Assert.AreEqual(1000,order.Size);
			Assert.AreEqual(sellStop2Id,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
		}
		
		[Test]
		public void Test04LongTwoOrders() {
			handler.Clear();
			
			
			CreateLogicalEntry(OrderType.BuyLimit,234.12,1000);
			CreateLogicalEntry(OrderType.SellStop,154.12,1000);
			int sellLimitId = CreateLogicalExit(OrderType.SellLimit,334.12);
			int sellStopId = CreateLogicalExit(OrderType.SellStop,134.12);
			CreateLogicalExit(OrderType.BuyLimit,124.12);
			CreateLogicalExit(OrderType.BuyStop,194.12);
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Sell,OrderType.SellStop,134.12,1000,sellStopId,sellOrder1);
			object sellOrder2 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Sell,OrderType.SellLimit,334.12,1000,sellLimitId,sellOrder2);
			
			var position = 1000;
			handler.SetActualPosition(position);
			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
		}
		
		[Test]
		public void Test04SyncLongTwoOrders() {
			handler.Clear();
			
			int sellLimitId = CreateLogicalExit(OrderType.SellLimit,334.12);
			int sellStopId = CreateLogicalExit(OrderType.SellStop,134.12);
			CreateLogicalExit(OrderType.BuyLimit,124.12);
			CreateLogicalExit(OrderType.BuyStop,194.12);
			
			var position = 1000;
			handler.SetActualPosition(0);
			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CreatedOrders.Count);
			
			PhysicalOrder order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.BuyMarket,order.Type);
			Assert.AreEqual(1000,order.Size);
			Assert.AreEqual(0,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
			
			handler.Clear();
			handler.SetActualPosition(1000);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(2,handler.Orders.CreatedOrders.Count);
			
			order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.SellLimit,order.Type);
			Assert.AreEqual(334.12,order.Price);
			Assert.AreEqual(1000,order.Size);
			Assert.AreEqual(sellLimitId,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
			
			order = handler.Orders.CreatedOrders[1];
			Assert.AreEqual(OrderType.SellStop,order.Type);
			Assert.AreEqual(134.12,order.Price);
			Assert.AreEqual(1000,order.Size);
			Assert.AreEqual(sellStopId,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
		}
		
		[Test]
		public void Test05LongPartialEntry() {
			handler.Clear();

			// Position now long but an entry order is still working at
			// only part of the size.
			// So size is 500 but order is still 500 due to original order 1000;
			
			int buyLimitId = CreateLogicalEntry(OrderType.BuyLimit,234.12,1000);
			int sellStopId = CreateLogicalEntry(OrderType.SellStop,154.12,1000);
			int sellLimitId = CreateLogicalExit(OrderType.SellLimit,334.12);
			int sellStop2Id = CreateLogicalExit(OrderType.SellStop,134.12);
			CreateLogicalExit(OrderType.BuyLimit,124.12);
			CreateLogicalExit(OrderType.BuyStop,194.12);
			
			object buyOrder = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyLimit,234.12,500,buyLimitId,buyOrder);
			object sellOrder = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Sell,OrderType.SellStop,154.12,1000,sellStopId,sellOrder);
			
			var position = 500;
			handler.SetActualPosition(position);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(1,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(2,handler.Orders.CreatedOrders.Count);
			
			var brokerOrder = handler.Orders.CanceledOrders[0];
			Assert.AreEqual(sellOrder,brokerOrder);
			
			var order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.SellLimit,order.Type);
			Assert.AreEqual(334.12,order.Price);
			Assert.AreEqual(500,order.Size);
			Assert.AreEqual(sellLimitId,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
			
			order = handler.Orders.CreatedOrders[1];
			Assert.AreEqual(OrderType.SellStop,order.Type);
			Assert.AreEqual(134.12,order.Price);
			Assert.AreEqual(500,order.Size);
			Assert.AreEqual(sellStop2Id,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
		}
		
		[Test]
		public void Test06LongPartialExit() {
			handler.Clear();
			
			int sellLimitId = CreateLogicalExit(OrderType.SellLimit,334.12);
			int sellStopId = CreateLogicalExit(OrderType.SellStop,134.12);
			CreateLogicalExit(OrderType.BuyLimit,124.12);
			CreateLogicalExit(OrderType.BuyStop,194.12);
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Sell,OrderType.SellStop,134.12,1000,sellStopId,sellOrder1);
			object sellOrder2 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Sell,OrderType.SellLimit,334.12,500,sellLimitId,sellOrder2);
			
			var position = 500;
			handler.SetActualPosition(position);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(1,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			
			var change = handler.Orders.ChangedOrders[0];
			Assert.AreEqual(OrderType.SellStop,change.Order.Type);
			Assert.AreEqual(134.12,change.Order.Price);
			Assert.AreEqual(500,change.Order.Size);
			Assert.AreEqual(sellStopId,change.Order.LogicalOrderId);
			Assert.AreEqual(sellOrder1,change.OrigBrokerOrder);
		}
		
		[Test]
		public void Test07ShortEntryFilled() {
			handler.Clear();
			
			int buyLimitId = CreateLogicalEntry(OrderType.BuyLimit,234.12,1000);
			CreateLogicalEntry(OrderType.SellStop,154.12,1000);
			CreateLogicalExit(OrderType.SellLimit,334.12);
			CreateLogicalExit(OrderType.SellStop,134.12);
			int buyLimit2Id = CreateLogicalExit(OrderType.BuyLimit,124.12);
			int buyStopId = CreateLogicalExit(OrderType.BuyStop,194.12);
			
			object buyOrder = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyLimit,234.12,1000,buyLimitId,buyOrder);
			
			var position = -1000;
			handler.SetActualPosition(position);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(1,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(2,handler.Orders.CreatedOrders.Count);
			
			var brokerOrder = handler.Orders.CanceledOrders[0];
			Assert.AreEqual(buyOrder,brokerOrder);
			
			var order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.BuyLimit,order.Type);
			Assert.AreEqual(124.12,order.Price);
			Assert.AreEqual(1000,order.Size);
			Assert.AreEqual(buyLimit2Id,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
			
			order = handler.Orders.CreatedOrders[1];
			Assert.AreEqual(OrderType.BuyStop,order.Type);
			Assert.AreEqual(194.12,order.Price);
			Assert.AreEqual(1000,order.Size);
			Assert.AreEqual(buyStopId,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
		}
		
		[Test]
		public void Test08ShortTwoOrders() {
			handler.Clear();
			
			CreateLogicalEntry(OrderType.BuyLimit,234.12,1000);
			CreateLogicalEntry(OrderType.SellStop,154.12,1000);
			CreateLogicalExit(OrderType.SellLimit,334.12);
			CreateLogicalExit(OrderType.SellStop,134.12);
			int buyLimitId = CreateLogicalExit(OrderType.BuyLimit,124.12);
			int buyStopId = CreateLogicalExit(OrderType.BuyStop,194.12);
			
			object buyOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyLimit,124.12,1000,buyLimitId,buyOrder1);
			object buyOrder2 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyStop,194.12,1000,buyStopId,buyOrder2);
			
			var position = -1000;
			handler.SetActualPosition(position);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
		}
		
		[Test]
		public void Test09ShortPartialEntry() {
			handler.Clear();

			// Position now long but an entry order is still working at
			// only part of the size.
			// So size is 500 but order is still 500 due to original order 1000;
			
			int buyLimitId = CreateLogicalEntry(OrderType.BuyLimit,234.12,1000);
			int sellStopId = CreateLogicalEntry(OrderType.SellStop,154.12,1000);
			CreateLogicalExit(OrderType.SellLimit,334.12);
			CreateLogicalExit(OrderType.SellStop,134.12);
			int buyLimit2Id = CreateLogicalExit(OrderType.BuyLimit,124.12);
			int buyStopId = CreateLogicalExit(OrderType.BuyStop,194.12);
			
			object buyOrder = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyLimit,234.12,1000,buyLimitId,buyOrder);
			object sellOrder = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellStop,154.12,500,sellStopId,sellOrder);
			
			var position = -500;
			handler.SetActualPosition(position);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(1,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(2,handler.Orders.CreatedOrders.Count);
			
			var brokerOrder = handler.Orders.CanceledOrders[0];
			Assert.AreEqual(buyOrder,brokerOrder);
			
			var order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.BuyLimit,order.Type);
			Assert.AreEqual(124.12,order.Price);
			Assert.AreEqual(500,order.Size);
			Assert.AreEqual(buyLimit2Id,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
			
			order = handler.Orders.CreatedOrders[1];
			Assert.AreEqual(OrderType.BuyStop,order.Type);
			Assert.AreEqual(194.12,order.Price);
			Assert.AreEqual(500,order.Size);
			Assert.AreEqual(buyStopId,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
		}
		
		[Test]
		public void Test10ShortPartialExit() {
			handler.Clear();
			
			CreateLogicalExit(OrderType.SellLimit,334.12);
			CreateLogicalExit(OrderType.SellStop,134.12);
			int buyLimitId = CreateLogicalExit(OrderType.BuyLimit,124.12);
			int buyStopId = CreateLogicalExit(OrderType.BuyStop,194.12);
			
			object buyOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyLimit,124.12,1000,buyLimitId,buyOrder1);
			object buyOrder2 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyStop,194.12,1000,buyStopId,buyOrder2);
			
			var position = -500;
			handler.SetActualPosition(position);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(2,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			
			var change = handler.Orders.ChangedOrders[0];
			Assert.AreEqual(OrderType.BuyLimit,change.Order.Type);
			Assert.AreEqual(124.12,change.Order.Price);
			Assert.AreEqual(500,change.Order.Size);
			Assert.AreEqual(buyLimitId,change.Order.LogicalOrderId);
			Assert.AreEqual(buyOrder1,change.OrigBrokerOrder);
			
			change = handler.Orders.ChangedOrders[1];
			Assert.AreEqual(OrderType.BuyStop,change.Order.Type);
			Assert.AreEqual(194.12,change.Order.Price);
			Assert.AreEqual(500,change.Order.Size);
			Assert.AreEqual(buyStopId,change.Order.LogicalOrderId);
			Assert.AreEqual(buyOrder2,change.OrigBrokerOrder);
		}
		
		[Test]
		public void Test11FlatChangeSizes() {
			handler.Clear();
			
			int buyLimitId = CreateLogicalEntry(OrderType.BuyLimit,234.12,700);
			int sellStopId = CreateLogicalEntry(OrderType.SellStop,154.12,800);
			CreateLogicalExit(OrderType.SellLimit,334.12);
			CreateLogicalExit(OrderType.SellStop,134.12);
			CreateLogicalExit(OrderType.BuyLimit,124.12);
			CreateLogicalExit(OrderType.BuyStop,194.12);
			
			object buyOrder = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyLimit,234.12,1000,buyLimitId,buyOrder);
			object sellOrder = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellStop,154.12,1000,sellStopId,sellOrder);
			
			var position = 0; // Pretend we're flat.
			handler.SetActualPosition(position);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(2,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			
			var change = handler.Orders.ChangedOrders[0];
			Assert.AreEqual(OrderType.BuyLimit,change.Order.Type);
			Assert.AreEqual(234.12,change.Order.Price);
			Assert.AreEqual(700,change.Order.Size);
			Assert.AreEqual(buyLimitId,change.Order.LogicalOrderId);
			Assert.AreEqual(buyOrder,change.OrigBrokerOrder);
			
			change = handler.Orders.ChangedOrders[1];
			Assert.AreEqual(OrderType.SellStop,change.Order.Type);
			Assert.AreEqual(154.12,change.Order.Price);
			Assert.AreEqual(800,change.Order.Size);
			Assert.AreEqual(sellStopId,change.Order.LogicalOrderId);
			Assert.AreEqual(sellOrder,change.OrigBrokerOrder);
			
		}
		
		[Test]
		public void Test12FlatChangePrices() {
			handler.Clear();
			
			int buyLimitId = CreateLogicalEntry(OrderType.BuyLimit,244.12,1000);
			int sellStopId = CreateLogicalEntry(OrderType.SellStop,164.12,1000);
			CreateLogicalExit(OrderType.SellLimit,374.12);
			CreateLogicalExit(OrderType.SellStop,184.12);
			CreateLogicalExit(OrderType.BuyLimit,194.12);
			CreateLogicalExit(OrderType.BuyStop,104.12);
			
			object buyOrder = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyLimit,234.12,1000,buyLimitId,buyOrder);
			object sellOrder = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellStop,154.12,1000,sellStopId,sellOrder);
			
			var position = 0;
			handler.SetActualPosition(position);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(2,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			
			var change = handler.Orders.ChangedOrders[0];
			Assert.AreEqual(OrderType.BuyLimit,change.Order.Type);
			Assert.AreEqual(244.12,change.Order.Price);
			Assert.AreEqual(1000,change.Order.Size);
			Assert.AreEqual(buyLimitId,change.Order.LogicalOrderId);
			Assert.AreEqual(buyOrder,change.OrigBrokerOrder);
			
			change = handler.Orders.ChangedOrders[1];
			Assert.AreEqual(OrderType.SellStop,change.Order.Type);
			Assert.AreEqual(164.12,change.Order.Price);
			Assert.AreEqual(1000,change.Order.Size);
			Assert.AreEqual(sellStopId,change.Order.LogicalOrderId);
			Assert.AreEqual(sellOrder,change.OrigBrokerOrder);
			
		}
		
		[Test]
		public void Test13LongChangePrices() {
			handler.Clear();
			
			CreateLogicalEntry(OrderType.BuyLimit,244.12,1000);
			CreateLogicalEntry(OrderType.SellStop,164.12,1000);
			int sellLimitId = CreateLogicalExit(OrderType.SellLimit,374.12);
			int sellStopId = CreateLogicalExit(OrderType.SellStop,184.12);
			CreateLogicalExit(OrderType.BuyLimit,194.12);
			CreateLogicalExit(OrderType.BuyStop,104.12);
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Sell,OrderType.SellStop,134.12,1000,sellStopId,sellOrder1);
			object sellOrder2 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Sell,OrderType.SellLimit,334.12,1000,sellLimitId,sellOrder2);
			
			var position = 1000;
			handler.SetActualPosition(position);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(2,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			
			var change = handler.Orders.ChangedOrders[0];
			Assert.AreEqual(OrderType.SellLimit,change.Order.Type);
			Assert.AreEqual(374.12,change.Order.Price);
			Assert.AreEqual(1000,change.Order.Size);
			Assert.AreEqual(sellLimitId,change.Order.LogicalOrderId);
			Assert.AreEqual(sellOrder2,change.OrigBrokerOrder);
			
			change = handler.Orders.ChangedOrders[1];
			Assert.AreEqual(OrderType.SellStop,change.Order.Type);
			Assert.AreEqual(184.12,change.Order.Price);
			Assert.AreEqual(1000,change.Order.Size);
			Assert.AreEqual(sellStopId,change.Order.LogicalOrderId);
			Assert.AreEqual(sellOrder1,change.OrigBrokerOrder);
			
		}
		
		[Test]
		public void Test13LongChangeSizes() {
			handler.Clear();
			
			CreateLogicalEntry(OrderType.BuyLimit,244.12,1000);
			CreateLogicalEntry(OrderType.SellStop,164.12,1000);
			int sellLimitId = CreateLogicalExit(OrderType.SellLimit,374.12);
			int sellStopId = CreateLogicalExit(OrderType.SellStop,184.12);
			CreateLogicalExit(OrderType.BuyLimit,194.12);
			CreateLogicalExit(OrderType.BuyStop,104.12);
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Sell,OrderType.SellStop,184.12,700,sellStopId,sellOrder1);
			object sellOrder2 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Sell,OrderType.SellLimit,374.12,800,sellLimitId,sellOrder2);
			
			var position = 1000;
			handler.SetActualPosition(position);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(2,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			
			var change = handler.Orders.ChangedOrders[0];
			Assert.AreEqual(OrderType.SellLimit,change.Order.Type);
			Assert.AreEqual(374.12,change.Order.Price);
			Assert.AreEqual(1000,change.Order.Size);
			Assert.AreEqual(sellLimitId,change.Order.LogicalOrderId);
			Assert.AreEqual(sellOrder2,change.OrigBrokerOrder);
			
			change = handler.Orders.ChangedOrders[1];
			Assert.AreEqual(OrderType.SellStop,change.Order.Type);
			Assert.AreEqual(184.12,change.Order.Price);
			Assert.AreEqual(1000,change.Order.Size);
			Assert.AreEqual(sellStopId,change.Order.LogicalOrderId);
			Assert.AreEqual(sellOrder1,change.OrigBrokerOrder);
			
		}
		
		[Test]
		public void Test14ShortToFlat() {
			handler.Clear();
			
			var position = -1000;
			handler.SetActualPosition(0); // Actual and desired differ!!!

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CreatedOrders.Count);
			
			PhysicalOrder order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.SellMarket,order.Type);
			Assert.AreEqual(0,order.Price);
			Assert.AreEqual(1000,order.Size);
			Assert.AreEqual(0,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
			
		}
		
		[Test]
		public void Test14AddToShort() {
			handler.Clear();
			
			var position = -4;
			handler.SetActualPosition(-2);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CreatedOrders.Count);
			
			PhysicalOrder order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.SellMarket,order.Type);
			Assert.AreEqual(OrderSide.SellShort,order.Side);
			Assert.AreEqual(0,order.Price);
			Assert.AreEqual(2,order.Size);
			Assert.AreEqual(0,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
		}
		
		[Test]
		public void Test14ReverseFromLong() {
			handler.Clear();
			
			var position = -2;
			handler.SetActualPosition(2);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CreatedOrders.Count);
			
			var order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.SellMarket,order.Type);
			Assert.AreEqual(OrderSide.Sell,order.Side);
			Assert.AreEqual(0,order.Price);
			Assert.AreEqual(2,order.Size);
			Assert.AreEqual(0,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
			
			handler.Clear();
			handler.SetActualPosition( 0);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CreatedOrders.Count);
			
			order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.SellMarket,order.Type);
			Assert.AreEqual(OrderSide.SellShort,order.Side);
			Assert.AreEqual(0,order.Price);
			Assert.AreEqual(2,order.Size);
			Assert.AreEqual(0,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
		}
		
		[Test]
		public void Test14ReduceFromLong() {
			handler.Clear();
			
			var desiredPosition = 1;
			handler.SetActualPosition(2);

			handler.SetDesiredPosition(desiredPosition);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CreatedOrders.Count);
			
			PhysicalOrder order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.SellMarket,order.Type);
			Assert.AreEqual(OrderSide.Sell,order.Side);
			Assert.AreEqual(0,order.Price);
			Assert.AreEqual(1,order.Size);
			Assert.AreEqual(0,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
			
		}
		
		[Test]
		public void Test15LongToFlat() {
			handler.Clear();
			
			var position = 1000;
			handler.SetActualPosition(0); // Actual and desired differ!!!

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CreatedOrders.Count);
			
			PhysicalOrder order = handler.Orders.CreatedOrders[0];
			Assert.AreEqual(OrderType.BuyMarket,order.Type);
			Assert.AreEqual(0,order.Price);
			Assert.AreEqual(1000,order.Size);
			Assert.AreEqual(0,order.LogicalOrderId);
			AssertBrokerOrder(order.BrokerOrder);
			
		}
		
		[Test]
		public void Test16ActiveSellMarket() {
			handler.Clear();
			
			var position = -10;
			handler.SetActualPosition(0); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellMarket,134.12,10,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
		}
		
		[Test]
		public void Test17ActiveBuyMarket() {
			handler.Clear();
			
			var position = 10;
			handler.SetActualPosition(-5); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
		}
		
		[Test]
		public void Test18ActiveExtraBuyMarket() {
			handler.Clear();
			
			var position = 10;
			handler.SetActualPosition(-5); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyMarket,134.12,15,0,sellOrder1);
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test19ActiveExtraSellMarket() {
			handler.Clear();
			
			var position = -10;
			handler.SetActualPosition(5); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellMarket,134.12,15,0,sellOrder1);
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test20ActiveUnneededSellMarket() {
			handler.Clear();
			
			var position = 0;
			handler.SetActualPosition(0); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test21ActiveUnneededBuyMarket() {
			handler.Clear();
			
			var position = 0;
			handler.SetActualPosition(0); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test22ActiveWrongSideSellMarket() {
			handler.Clear();
			
			var position = 10;
			handler.SetActualPosition(-5); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test23ActiveWrongSideBuyMarket() {
			handler.Clear();
			
			var position = -10;
			handler.SetActualPosition(5); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test24ActiveBuyLimit() {
			handler.Clear();
			
			var position = 10;
			handler.SetActualPosition(-5);
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyLimit,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CanceledOrders.Count);
			
		}
		
		[Test]
		public void Test25ActiveSellLimit() {
			handler.Clear();
			
			var position = -10;
			handler.SetActualPosition(5); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellLimit,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CanceledOrders.Count);
			
		}
		
		[Test]
		public void Test26ActiveBuyAndSellLimit() {
			handler.Clear();
			
			CreateLogicalEntry(OrderType.BuyMarket,0,2);
			
			var position = 0;
			handler.SetActualPosition(0);
			
			object sellOrder1 = new object();
			object buyOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyLimit,15.12,3,0,buyOrder1);
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellLimit,34.12,3,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(2,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test27PendingSellMarket() {
			handler.Clear();
			
			var position = -10;
			handler.SetActualPosition(0); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Pending,OrderSide.SellShort,OrderType.SellMarket,134.12,10,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
		}
		
		[Test]
		public void Test28PendingBuyMarket() {
			handler.Clear();
			
			var position = 10;
			handler.SetActualPosition(-5); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Pending,OrderSide.Buy,OrderType.BuyMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
		}
		
		[Test]
		public void Test29PendingExtraBuyMarket() {
			handler.Clear();
			
			var position = 10;
			handler.SetActualPosition(-5); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Pending,OrderSide.Buy,OrderType.BuyMarket,134.12,15,0,sellOrder1);
			handler.Orders.AddPhysicalOrder(OrderState.Pending,OrderSide.Buy,OrderType.BuyMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test30PendingExtraSellMarket() {
			handler.Clear();
			
			var position = -10;
			handler.SetActualPosition(5); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Pending,OrderSide.SellShort,OrderType.SellMarket,134.12,15,0,sellOrder1);
			handler.Orders.AddPhysicalOrder(OrderState.Pending,OrderSide.SellShort,OrderType.SellMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test31PendingUnneededSellMarket() {
			handler.Clear();
			
			var position = 0;
			handler.SetActualPosition(0); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Pending,OrderSide.SellShort,OrderType.SellMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test32PendingUnneededBuyMarket() {
			handler.Clear();
			
			var position = 0;
			handler.SetActualPosition(0); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Pending,OrderSide.Buy,OrderType.BuyMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test33PendingWrongSideSellMarket() {
			handler.Clear();
			
			var position = 10;
			handler.SetActualPosition(-5); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Pending,OrderSide.SellShort,OrderType.SellMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test34PendingWrongSideBuyMarket() {
			handler.Clear();
			
			var position = -10;
			handler.SetActualPosition(5); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Pending,OrderSide.Buy,OrderType.BuyMarket,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test35PendingBuyLimit() {
			handler.Clear();
			
			var position = 10;
			handler.SetActualPosition(-5);
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Pending,OrderSide.Buy,OrderType.BuyLimit,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(0,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test36PendingSellLimit() {
			handler.Clear();
			
			var position = -10;
			handler.SetActualPosition(5); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellLimit,134.12,15,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(null);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CanceledOrders.Count);
		}
		
		[Test]
		public void Test37PendingBuyAndSellLimit() {
			handler.Clear();
			
			CreateLogicalEntry(OrderType.BuyMarket,0,2);
			
			var position = 0;
			handler.SetActualPosition(0); // Actual and desired differ!!!
			
			object sellOrder1 = new object();
			object buyOrder1 = new object();
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.Buy,OrderType.BuyLimit,15.12,3,0,buyOrder1);
			handler.Orders.AddPhysicalOrder(OrderState.Active,OrderSide.SellShort,OrderType.SellLimit,34.12,3,0,sellOrder1);

			handler.SetDesiredPosition(position);
			handler.SetLogicalOrders(orders);
			handler.PerformCompare();
			
			Assert.AreEqual(0,handler.Orders.ChangedOrders.Count);
			Assert.AreEqual(1,handler.Orders.CreatedOrders.Count);
			Assert.AreEqual(2,handler.Orders.CanceledOrders.Count);
		}
		
		public class Change {
			public PhysicalOrder Order;
			public object OrigBrokerOrder;
			public Change( PhysicalOrder order, object origBrokerOrder) {
				this.Order = order;
				this.OrigBrokerOrder = origBrokerOrder;
			}
		}
		
		public class MockPhysicalOrderHandler : PhysicalOrderHandler {
			public List<object> CanceledOrders = new List<object>();
			public List<Change> ChangedOrders = new List<Change>();
			public List<PhysicalOrder> CreatedOrders = new List<PhysicalOrder>();
			public List<PhysicalOrder> inputOrders = new List<PhysicalOrder>();
			private PhysicalOrderHandler confirmOrders;
			
			private SymbolInfo symbol;
			public MockPhysicalOrderHandler(SymbolInfo symbol) {
				this.symbol = symbol;
			}
			public void OnCancelBrokerOrder(SymbolInfo symbol, object brokerOrder)
			{
				CanceledOrders.Add(brokerOrder);
				RemoveByBrokerOrder(brokerOrder);
				if( confirmOrders != null) {
					confirmOrders.OnCancelBrokerOrder(symbol, brokerOrder);
				}
			}
			private void RemoveByBrokerOrder(object brokerOrder) {
				for( int i=0; i<inputOrders.Count; i++) {
					var order = inputOrders[i];
					if( order.BrokerOrder == brokerOrder) {
						inputOrders.Remove(order);
					}
				}
			}
			public void OnChangeBrokerOrder(PhysicalOrder order, object origBrokerOrder)
			{
				ChangedOrders.Add(new Change( order, origBrokerOrder));
				RemoveByBrokerOrder(origBrokerOrder);
				inputOrders.Add( order);
				if( confirmOrders != null) {
					confirmOrders.OnChangeBrokerOrder(order, origBrokerOrder);
				}
			}
			public void OnCreateBrokerOrder(PhysicalOrder order)
			{
				CreatedOrders.Add(order);
				inputOrders.Add(order);
				if( confirmOrders != null) {
					confirmOrders.OnCreateBrokerOrder(order);
				}
			}
			public void ClearPhysicalOrders()
			{
				CanceledOrders.Clear();
				ChangedOrders.Clear();
				CreatedOrders.Clear();
				inputOrders.Clear();
			}
			public void AddPhysicalOrder(PhysicalOrder order)
			{
				inputOrders.Add(order);
			}
			
			public void AddPhysicalOrder(OrderState orderState, OrderSide side, OrderType type, double price, int size, int logicalOrderId, object brokerOrder)
			{
				var order = Factory.Utility.PhysicalOrder( orderState, symbol, side, type, price, size, logicalOrderId, 0, brokerOrder, null);
				inputOrders.Add(order);
				
			}
			
			public Iterable<PhysicalOrder> GetActiveOrders(SymbolInfo symbol)
			{
				var result = new ActiveList<PhysicalOrder>();
				foreach( var order in inputOrders) {
					result.AddLast( order);
				}
				return result;
			}
			
			public PhysicalOrderHandler ConfirmOrders {
				get { return confirmOrders; }
				set { confirmOrders = value; }
			}
		}

		public class TestOrderAlgorithm {
			private OrderAlgorithm orderAlgorithm;
			private MockPhysicalOrderHandler orders;
			private SymbolInfo symbol;
			private Strategy strategy;
			public TestOrderAlgorithm(SymbolInfo symbol, Strategy strategy) {
				this.symbol = symbol;
				this.strategy = strategy;
				orders = new MockPhysicalOrderHandler(symbol);
				orderAlgorithm = Factory.Utility.OrderAlgorithm("test",symbol,orders);
				orders.ConfirmOrders = orderAlgorithm;
			}
			public void Clear() {
				orders.ClearPhysicalOrders();
//				strategy.Position.Change(0,100.00,TimeStamp.UtcNow);
			}
			public void SetActualPosition( int position) {
				orderAlgorithm.SetActualPosition(position);
			}
			public void SetDesiredPosition( int position) {
				strategy.Position.Change(position,100.00,TimeStamp.UtcNow);
				orderAlgorithm.SetDesiredPosition(position);
			}
			public void SetLogicalOrders(Iterable<LogicalOrder> logicalOrders) {
				orderAlgorithm.SetLogicalOrders(logicalOrders);
			}
			public void PerformCompare()
			{
				orderAlgorithm.PerformCompare();
			}
			
			public double ActualPosition {
				get { return orderAlgorithm.ActualPosition; }
			}
			
			public void ProcessFill(PhysicalFill fill,int totalSize, int cumulativeSize, int remainingSize)
			{
				orderAlgorithm.ProcessFill(fill,totalSize,cumulativeSize,remainingSize);
			}
			
			public MockPhysicalOrderHandler Orders {
				get { return orders; }
			}
		}
	}
}
