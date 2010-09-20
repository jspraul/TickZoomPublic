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
using TickZoom.Statistics;

namespace TickZoom.Common
{

		
	public class Portfolio : Model, PortfolioInterface
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(Portfolio));
		private static readonly bool debug = log.IsDebugEnabled;
		private List<Strategy> strategies = new List<Strategy>();
		private List<Portfolio> portfolios = new List<Portfolio>();
		private Dictionary<ModelInterface,StrategyWatcher> watchers = new Dictionary<ModelInterface,StrategyWatcher>();
		private List<StrategyWatcher> activeWatchers = new List<StrategyWatcher>();
		private PortfolioType portfolioType = PortfolioType.None;
		private double closedEquity = 0;
		private Result result;
		private PositionCommon position;
		private Performance performance;
		private bool isActiveOrdersChanged;
		private Action<StrategyInterface> onActiveOrdersChange;
		
		public Portfolio()
		{
			result = new Result(this);
			position = new PositionCommon(this);
			performance = new Performance(this);
			FullName = this.GetType().Name;
			Performance.GraphTrades = false;
			RequestEvent(EventType.Tick);
		}
		
		public sealed override void OnConfigure() {
			BreakPoint.TrySetStrategy(this);
			base.OnConfigure();
			AddInterceptor(performance.Equity);
			AddInterceptor(performance);
			do {
				// Count all the unique symbols used by dependencies and
				// get a list of all the strategies.
				strategies = new List<Strategy>();
				portfolios = new List<Portfolio>();
				Dictionary<string,List<Model>> symbolMap = new Dictionary<string,List<Model>>();
				for( int i=0; i<Chain.Dependencies.Count; i++) {
					Chain chain = Chain.Dependencies[i];
					if( chain.Model is Strategy || chain.Model is Portfolio) {
						Model model = (Model) chain.Model;
						List<Model> tempModels;
						if( symbolMap.TryGetValue(model.SymbolDefault, out tempModels)) {
							tempModels.Add(model);
						} else {
							tempModels = new List<Model>();
							tempModels.Add(model);
							symbolMap[model.SymbolDefault] = tempModels;
						}
						if( model is Strategy) {
							strategies.Add((Strategy)model);
						} else if( model is Portfolio) {
							portfolios.Add((Portfolio)model);
						}
					}
				}
				if(symbolMap.Count == 1) {
					portfolioType = PortfolioType.SingleSymbol;
				} else if( symbolMap.Count == (strategies.Count+portfolios.Count)) {
					portfolioType = PortfolioType.MultiSymbol;
				} else {
					// Remove all dependencies which have more than one obect.
					for( int i=Chain.Dependencies.Count-1; i>=0; i--) {
						Chain chain = Chain.Dependencies[i];
						Chain.Dependencies.RemoveAt(i);
					}
					// There is a mixture of multi symbols and multi strategies per symbol.
					// Insert additional Portfolios for each symbol.
					foreach( var kvp in symbolMap) {
						string symbol = kvp.Key;
						List<Model> tempStrategies = kvp.Value;
						if( tempStrategies.Count > 1) {
							Portfolio portfolio = new Portfolio();
							portfolio.Name = "AutoGen-Portfolio-"+symbol;
							portfolio.SymbolDefault = symbol;
							portfolio.IntervalDefault = IntervalDefault;
							portfolio.Performance.Equity.EnableDailyStats = Performance.Equity.EnableDailyStats;
							portfolio.Performance.Equity.EnableWeeklyStats = Performance.Equity.EnableWeeklyStats;
							portfolio.Performance.Equity.EnableMonthlyStats = Performance.Equity.EnableMonthlyStats;
							portfolio.Performance.Equity.EnableYearlyStats = Performance.Equity.EnableYearlyStats;
							
							foreach( var strategy in tempStrategies) {
								portfolio.Chain.Dependencies.Add(strategy.Chain);
							}
							Chain.Dependencies.Add( portfolio.Chain);
						} else {
							Model model = tempStrategies[0];
							Chain.Dependencies.Add( model.Chain);
						}
					}
				}
			} while( portfolioType == PortfolioType.None);
			
			// Create strategy watchers
			foreach( var strategy in strategies) {
				strategy.OnActiveChange += HandleActiveChange;
				StrategyWatcher watcher = new StrategyWatcher(strategy);
				watchers.Add( strategy, watcher);
				if( strategy.IsActive) {
					activeWatchers.Add( watcher);
				}
			}
			
			// Listen for fill events.
			foreach( var strategy in strategies) {
				strategy.AddEventListener(EventType.LogicalFill,this);
			}
			foreach( var portfolio in portfolios) {
				activeWatchers.Add( new StrategyWatcher(portfolio));
			}
		}
		
		public void HandleActiveChange(ModelInterface model) {
			StrategyWatcher watcher;
			if( watchers.TryGetValue(model,out watcher)) {
				if( watcher.IsActive && !activeWatchers.Contains(watcher)) {
					activeWatchers.Add(watcher);
				}
				if( !watcher.IsActive && activeWatchers.Contains(watcher)) {
					activeWatchers.Remove(watcher);
				}
			} else {
				throw new ApplicationException("ActiveChange event occured but watcher was not found for the strategy.");
			}
		}
		
		public override void OnEvent(EventContext context, EventType eventType, object eventDetail)
		{
			base.OnEvent(context, eventType, eventDetail);
			
			if( eventType == EventType.LogicalFill) {
				TryMergeSingleSymbolPositions();
				TryMergeSingleSymbolOrders();
				TryMergeMultiSymbolEquity();
			}
			
			if( eventType == EventType.Tick ) {
				TryMergeSingleSymbolOrders();
				TryMergeMultiSymbolEquity();
			}
		}

		private void TryMergeSingleSymbolPositions()
		{
			if( portfolioType != PortfolioType.SingleSymbol) return;
			double internalSignal = 0;
			double totalPrice = 0;
			int changeCount = 0;
			int count = activeWatchers.Count;
			for(int i=0; i<count; i++) {
				var watcher = activeWatchers[i];
				if( !watcher.IsActive) continue;
				internalSignal += watcher.Position.Current;
				if (watcher.PositionChanged) {
					totalPrice += watcher.Position.Price;
					changeCount++;
					watcher.Refresh();
				}
			}
			if (changeCount > 0) {
				double averagePrice = (totalPrice / changeCount).Round();
				Position.Change(internalSignal, averagePrice, Ticks[0].Time);
				Result.Position.Copy(Position);
			}
		}
		
		private void TryMergeSingleSymbolOrders()
		{
			if( portfolioType != PortfolioType.SingleSymbol) return;
			int count = activeWatchers.Count;
			for(int i=0; i<count; i++) {
				var watcher = activeWatchers[i];
				if( !watcher.IsActive) continue;
				if (watcher.IsActiveOrdersChanged) {
					isActiveOrdersChanged = true;
				}
			}
			if( isActiveOrdersChanged ) { 
				activeOrders.Clear();
				for(int i=0; i<count; i++) {
					var watcher = activeWatchers[i];
					if( !watcher.IsActive) continue;
					activeOrders.AddLast(watcher.ActiveOrders);
				}
			}
		}
		
		public void TryMergeMultiSymbolEquity() {
			if( portfolioType != PortfolioType.MultiSymbol) return;
			double tempClosedEquity = 0;
			double tempOpenEquity = 0;
			int count = strategies.Count;
			for(int i=0; i<count; i++) {
				var strategy = strategies[i];
				tempOpenEquity += strategy.Performance.Equity.OpenEquity;
				tempClosedEquity += strategy.Performance.Equity.ClosedEquity;
				tempClosedEquity -= strategy.Performance.Equity.StartingEquity;
			}
			count = portfolios.Count;
			for(int i=0; i<count; i++) {
				var portfolio = portfolios[i];
				tempOpenEquity += portfolio.Performance.Equity.OpenEquity;
				tempClosedEquity += portfolio.Performance.Equity.ClosedEquity;
				tempClosedEquity -= portfolio.Performance.Equity.StartingEquity;
			}
			if (tempClosedEquity != closedEquity) {
				double change = tempClosedEquity - closedEquity;
				Performance.Equity.OnChangeClosedEquity(change);
				closedEquity = tempClosedEquity;
			}
			Performance.Equity.OnUpdateOpenEquity(tempOpenEquity);
		}
		
		public double GetOpenEquity() {
			if( portfolioType == PortfolioType.SingleSymbol) {
				return performance.Equity.OpenEquity;
			} else if( portfolioType == PortfolioType.MultiSymbol) {
				double tempOpenEquity = 0;
				foreach( var strategy in strategies) {
					tempOpenEquity += strategy.Performance.Equity.OpenEquity;
				}
				foreach( var portfolio in portfolios) {
					tempOpenEquity += portfolio.GetOpenEquity();
				}
				return tempOpenEquity;
			} else {
				throw new ApplicationException("PortfolioType was never set.");
			}
		}		
		
		/// <summary>
		/// Shortcut to look at the data of and control any dependant strategies.
		/// </summary>
		public List<Strategy> Strategies {
			get { return strategies; }
		}
		
		public List<Portfolio> Portfolios {
			get { return portfolios; }
		}

		[Obsolete("Please use Strategies instead.",true)]
		public List<Strategy> Markets {
			get { return strategies; }
		}
		
		public PortfolioType PortfolioType {
			get { return portfolioType; }
			set { portfolioType = value; }
		}
		
		public ResultInterface Result {
			get { return result; }
		}
		
		public PositionInterface Position {
			get { return position; }
		}
		
		public override bool OnWriteReport(string folder)
		{
			return performance.WriteReport(Name,folder);
		}

		public virtual double OnGetFitness()
		{
			EquityStats stats = Performance.Equity.CalculateStatistics();
			return stats.Daily.SortinoRatio;
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
		
		public Performance Performance {
			get { return performance; }
		}
		
		ActiveList<LogicalOrder> allOrders = new ActiveList<LogicalOrder>();
		public Iterable<LogicalOrder> AllOrders {
			get {
				return allOrders;
			}
		}
		
		ActiveList<LogicalOrder> activeOrders = new ActiveList<LogicalOrder>();
		public Iterable<LogicalOrder> ActiveOrders {
			get {
				return activeOrders;
			}
		}
		
		private NodePool<LogicalOrder> nodePool = new NodePool<LogicalOrder>();
		
		public void OrderModified( LogicalOrder order) {
			return;
		}
		
		public bool IsActiveOrdersChanged {
			get { return isActiveOrdersChanged; }
			set { if( isActiveOrdersChanged != value) {
					isActiveOrdersChanged = value; 
					foreach( var watcher in activeWatchers) {
						watcher.IsActiveOrdersChanged = value;
					}
				}
			}
		}
		
		public void RefreshActiveOrders() {
			
		}
		
		public bool IsExitStrategyFlat {
			get { return false; }
		}
		
		public Action<StrategyInterface> OnActiveOrdersChange {
			get { return onActiveOrdersChange; }
			set { onActiveOrdersChange = value; }
		}
		
		public bool TryGetOrderById(int id, out LogicalOrder order)
		{
			throw new NotImplementedException();
		}
	}

	[Obsolete("Please use Portfolio instead.",true)]
	public class PortfolioCommon : Portfolio {
		
	}
}
