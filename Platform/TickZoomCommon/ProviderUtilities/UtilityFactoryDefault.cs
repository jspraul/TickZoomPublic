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
using TickZoom.Api;
using TickZoom.Interceptors;

namespace TickZoom.Common
{
	[Diagram(AttributeExclude=true)]
	public class UtilityFactoryDefault : UtilityFactory
	{
		public PhysicalOrder PhysicalOrder( OrderState orderState, SymbolInfo symbol, OrderSide side, OrderType type, double price, int size, int logicalOrderId, long logicalSerialNumber, object brokerOrder, object tag) {
			return new PhysicalOrderDefault(orderState, symbol,side,type,price,size,logicalOrderId,logicalSerialNumber,brokerOrder,tag);
		}
		
		public ProviderService CommandLineProcess() {
			return new CommandLineProcess();
		}
		public ProviderService WindowsService() {
			return new WindowsService();
		}
		public OrderAlgorithm OrderAlgorithm(string name, SymbolInfo symbol, PhysicalOrderHandler handler) {
			return new OrderAlgorithmDefault(name,symbol,handler);
		}
		public SymbolHandler SymbolHandler(SymbolInfo symbol, Receiver receiver) {
			return new SymbolHandlerDefault(symbol,receiver);
		}
		public VerifyFeed VerifyFeed() {
			return new VerifyFeedDefault();
		}
		public FillSimulator FillSimulator(string name, SymbolInfo symbol) {
			return new FillSimulatorPhysical(name, symbol);
		}
		public FillHandler FillHandler() {
			return new FillHandlerDefault();
		}
		public FillHandler FillHandler(StrategyInterface strategy) {
			return new FillHandlerDefault(strategy);
		}
		public BreakPointInterface BreakPoint() {
			return new BreakPoint();
		}
		[Diagram(AttributeExclude=true)]
		public PositionInterface Position(ModelInterface model) {
			return new PositionCommon(model);
		}
		
		public PhysicalFill PhysicalFill(int size, double price, TimeStamp time, TimeStamp utcTime, PhysicalOrder order) {
			return new PhysicalFillDefault(size,price,time,utcTime,order);
		}
	}
}
