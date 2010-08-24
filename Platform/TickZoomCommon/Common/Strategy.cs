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
using System.ComponentModel;
using System.Text;

using TickZoom.Api;
using TickZoom.Interceptors;
using TickZoom.Statistics;

namespace TickZoom.Common
{
	public class Strategy : Model, StrategyInterface
	{
		PositionInterface position;
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(Strategy));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		private readonly Log instanceLog;
		private readonly bool instanceDebug;
		private readonly bool instanceTrace;
		private Result result;
		private Dictionary<int,LogicalOrder> ordersHash = new Dictionary<int,LogicalOrder>();
		private ActiveList<LogicalOrder> allOrders = new ActiveList<LogicalOrder>();
		private ActiveList<LogicalOrder> activeOrders = new ActiveList<LogicalOrder>();
		private List<LogicalOrder> nextBarOrders = new List<LogicalOrder>();
		private bool isActiveOrdersChanged = false;
		
		OrderHandlers orders;
		ReverseCommon reverseActiveNow;
		ReverseCommon reverseNextBar;
		ExitCommon exitActiveNow;
		EnterCommon enterActiveNow;
		ExitCommon exitNextBar;
		EnterCommon enterNextBar;
		Performance performance;
		ExitStrategy exitStrategy;
		OrderManager preOrderManager;
		OrderManager postOrderManager;
		FillManager preFillManager;
		FillManager postFillManager;
		
		public Strategy()
		{
			instanceLog = Factory.SysLog.GetLogger(this.GetType()+"."+Name);
			instanceDebug = instanceLog.IsDebugEnabled;
			instanceTrace = instanceLog.IsTraceEnabled;
			position = new PositionCommon(this);
			if( trace) log.Trace("Constructor");
			Chain.Dependencies.Clear();
			isStrategy = true;
			result = new Result(this);
			
			exitActiveNow = new ExitCommon(this);
			enterActiveNow = new EnterCommon(this);
			reverseActiveNow = new ReverseCommon(this);
			reverseNextBar = new ReverseCommon(this);
			reverseNextBar.Orders = reverseActiveNow.Orders;
			reverseNextBar.IsNextBar = true;
			exitNextBar = new ExitCommon(this);
			exitNextBar.Orders = exitActiveNow.Orders;
			exitNextBar.IsNextBar = true;
			enterNextBar = new EnterCommon(this);
			enterNextBar.Orders = enterActiveNow.Orders;
			enterNextBar.IsNextBar = true;
			orders = new OrderHandlers(enterActiveNow,enterNextBar,
			                           exitActiveNow,exitNextBar,
			                           reverseActiveNow,reverseNextBar);
			
			// Interceptors.
			performance = new Performance(this);
		    exitStrategy = new ExitStrategy(this);
		    preOrderManager = Factory.Engine.OrderManager(this);
			postOrderManager = Factory.Engine.OrderManager(this);
			postOrderManager.PostProcess = true;
			postOrderManager.DoEntryOrders = false;
			postOrderManager.DoExitOrders = false;
			postOrderManager.DoExitStrategyOrders = true;
			
		    preFillManager = new FillManager(this);
			postFillManager = new FillManager(this);
			postFillManager.PostProcess = true;
			postFillManager.ChangePosition = exitStrategy.Position.Change;
			postFillManager.DoEntryOrders = false;
			postFillManager.DoExitOrders = false;
			postFillManager.DoExitStrategyOrders = true;
		}
		
		public override void OnConfigure()
		{
			reverseActiveNow.OnInitialize();
			reverseNextBar.OnInitialize();
			exitActiveNow.OnInitialize();
			enterActiveNow.OnInitialize();
			exitNextBar.OnInitialize();
			enterNextBar.OnInitialize();
			exitNextBar.OnInitialize();
			base.OnConfigure();

			BreakPoint.TrySetStrategy(this);
			AddInterceptor(preOrderManager);
			AddInterceptor(preFillManager);
			AddInterceptor(performance.Equity);
			AddInterceptor(performance);
			AddInterceptor(exitStrategy);
			AddInterceptor(postOrderManager);
			AddInterceptor(postFillManager);
		}
		
		public override void OnEvent(EventContext context, EventType eventType, object eventDetail)
		{
			base.OnEvent(context, eventType, eventDetail);
			if( context.Position == null) {
				context.Position = new PositionCommon(this);
			}
			context.Position.Copy(Position);
		}
		
		[Obsolete("Please, use OnGetOptimizeResult() instead.",true)]
		public virtual string ToStatistics() {
			throw new NotImplementedException();
		}
		
		public virtual string OnGetOptimizeHeader(Dictionary<string,object> optimizeValues)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Fitness");
			foreach( KeyValuePair<string,object> kvp in optimizeValues) {
				sb.Append(",");
				sb.Append(kvp.Key);
			}
			return sb.ToString();
		}
		
		public virtual string OnGetOptimizeResult(Dictionary<string,object> optimizeValues)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(OnGetFitness());
			foreach( KeyValuePair<string,object> kvp in optimizeValues) {
				sb.Append(",");
				sb.Append(kvp.Value);
			}
			return sb.ToString();
		}
		
		public override bool OnWriteReport(string folder)
		{
			return performance.WriteReport(Name,folder);
		}
		
		private void ActiveOrdersChanged(LogicalOrder order) {
			if( trace) {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Active Orders:");
				foreach( var item in activeOrders) {
					sb.Append("        ");
					sb.AppendLine( item.ToString());
				}
				sb.AppendLine("NextBar Orders:");
				foreach( var item in nextBarOrders) {
					sb.Append("        ");
					sb.AppendLine( item.ToString());
				}
				log.Trace("Order #" + order.Id + " was modified while position = " + position.Current + "\n" + sb);
				sb.AppendLine();
			}
		}
		
		public void OrderModified( LogicalOrder order) {
			if( order.Status == OrderStatus.Active) {
				// Any change to an active order even if only 
				// a price change means the list change.
				IsActiveOrdersChanged = true;
				if( !activeOrders.Contains(order)) {
					bool found = false;
					var next = activeOrders.First;
					for( var node = next; node != null; node = next) {
						next = node.Next;
						LogicalOrder other = node.Value;
						if( order.CompareTo(other) < 0) {
							activeOrders.AddBefore(node,order);
							found = true;
							break;
						}
					}
					if( !found) {
						activeOrders.AddLast(order);
					}
				}
			} else {
				if( activeOrders.Remove(order)) {
					// Since this order became inactive, it
					// means the active list changed.
					IsActiveOrdersChanged = true;
				}
			}
			if( order.IsNextBar) {
				if( !nextBarOrders.Contains(order)) {
					nextBarOrders.Add(order);
				}
			} else {
				if( nextBarOrders.Contains(order)) {
					nextBarOrders.Remove(order);
				}
			}
			ActiveOrdersChanged(order);
		}
		
		[Browsable(true)]
		[Category("Strategy Settings")]		
		public override Interval IntervalDefault {
			get { return base.IntervalDefault; }
			set { base.IntervalDefault = value; }
		}
		
		[Browsable(false)]
		public Strategy Next {
			get { return Chain.Next.Model as Strategy; }
		}
		
		[Browsable(false)]
		public Strategy Previous {
			get { return Chain.Previous.Model as Strategy; }
		}
		
		[Browsable(false)]
		public override string Name {
			get { return base.Name; }
			set { base.Name = value; }
		}

		public OrderHandlers Orders {
			get { return orders; }
		}
		
		[Obsolete("Please user Orders.Exit instead.",true)]
		public ExitCommon ExitActiveNow {
			get { return exitActiveNow; }
		}
		
		[Obsolete("Please user Orders.Enter instead.",true)]
		public EnterCommon EnterActiveNow {
			get { return enterActiveNow; }
		}
		
		[Category("Strategy Settings")]
		public ExitStrategy ExitStrategy {
			get { return exitStrategy; }
			set { exitStrategy = value; }
		}
		
		[Category("Strategy Settings")]
		public Performance Performance {
			get { return performance; }
			set { performance = value;}
		}
		
		
		/// <summary>
		/// Obsolete. Please use the Level2LotSize property in the symbol dictionary instead.
		/// </summary>
		[Obsolete("Obsolete. Please use the Level2LotSize property in the symbol dictionary instead.",true)]
		public PositionSize PositionSize {
			get { return new PositionSize(); }
			set { }
		}

		public virtual double OnGetFitness() {
			EquityStats stats = Performance.Equity.CalculateStatistics();
			return stats.Daily.SortinoRatio;
		}
		
		public Log Log {
			get { return instanceLog; }
		}
		
		public bool IsDebug {
			get { return instanceDebug; }
		}
		
		public bool IsTrace {
			get { return instanceTrace; }
		}
		
		public virtual void OnEnterTrade() {
			
		}
		
		public virtual void OnExitTrade() {
			
		}
		
		public PositionInterface Position {
			get { return position; }
			set { position = value; }
		}
		
		public ResultInterface Result {
			get { return result; }
		}

		[Diagram(AttributeExclude=true)]
		public void AddOrder(LogicalOrder order)
		{
			Context.AddOrder(order);
			allOrders.AddLast(order);
			ordersHash.Add(order.Id,order);
		}
		
		public Iterable<LogicalOrder> AllOrders {
			get {
				return allOrders;
			}
		}
		
		public bool TryGetOrderById(int id, out LogicalOrder order)
		{
			return ordersHash.TryGetValue(id,out order);
		}
		
		public Iterable<LogicalOrder> ActiveOrders {
			get {
				return activeOrders;
			}
		}
		
		public bool IsActiveOrdersChanged {
			get { return isActiveOrdersChanged; }
			set { isActiveOrdersChanged = value; }
		}
		
		public bool IsExitStrategyFlat {
			get { return exitStrategy.Position.IsFlat && position.HasPosition; }
		}
	}
	
	/// <summary>
	/// Obsolete. Please use the Level2LotSize property in the symbol dictionary instead.
	/// </summary>
	[Obsolete("Obsolete. Please use the Level2LotSize property in the symbol dictionary instead.",true)]
	public class PositionSize {
		/// <summary>
		/// Obsolete. Please use the Level2LotSize property in the symbol dictionary instead.
		/// </summary>
		[Obsolete("Obsolete. Please use the Level2LotSize property in the symbol dictionary instead.",true)]
		public int Size {
			get { return 0; } 
			set { }
		}
	}
		
	[Obsolete("Please user Strategy instead.",true)]
	public class StrategyCommon : Strategy {
		
	}
}