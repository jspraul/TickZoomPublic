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
using System.Runtime.InteropServices;

namespace TickZoom.Transactions
{
	public struct TransactionPairBinary 
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(TransactionPair));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		public static readonly string TIMEFORMAT = "yyyy-MM-dd HH:mm:ss.fff";
		private int entryOrderId;
		private int exitOrderId;
		private long entryTime;
		private long postedEntryTime;
		private long exitTime;
		private long postedExitTime;
		private double direction;
		private double volume;
		private double entryPrice;
		private double exitPrice;
		private double maxPrice;
		private double minPrice;
		private int entryBar;
		private int exitBar;
		private bool completed;
		
		public static TransactionPairBinary Parse(string value) {
			TransactionPairBinary pair = new TransactionPairBinary();
			string[] fields = value.Split(',');
			int field = 0;
			pair.direction = double.Parse(fields[field++]);
			pair.entryOrderId = int.Parse(fields[field++]);
			pair.entryBar = int.Parse(fields[field++]);
			pair.entryPrice = double.Parse(fields[field++]);
			pair.entryTime = TimeStamp.Parse(fields[field++]).Internal;
			pair.postedEntryTime = TimeStamp.Parse(fields[field++]).Internal;
			pair.exitOrderId = int.Parse(fields[field++]);
			pair.exitBar = int.Parse(fields[field++]);
			pair.exitPrice = double.Parse(fields[field++]);
			pair.exitTime = TimeStamp.Parse(fields[field++]).Internal;
			pair.postedExitTime = TimeStamp.Parse(fields[field++]).Internal;
			pair.maxPrice = double.Parse(fields[field++]);
			pair.minPrice = double.Parse(fields[field++]);
			return pair;
		}
	
		public override string ToString() {
			return direction + "," + entryOrderId + "," + entryBar + "," + entryPrice + "," + new TimeStamp(entryTime) + "," + new TimeStamp(postedEntryTime) + "," +
				exitOrderId + "," + exitBar + "," + exitPrice + "," + new TimeStamp(exitTime) + "," + new TimeStamp(postedExitTime) + "," + maxPrice + "," + minPrice;
		}
		
		
		public bool Completed {
			get { return completed; }
		}
		
		public static TransactionPairBinary Create() {
			return new TransactionPairBinary();
		}
		
		public void SetProperties(string parameters)
		{
			string[] strings = parameters.Split(new char[] {','});
			direction = Convert.ToInt32(strings[0]);
			entryPrice = Convert.ToDouble(strings[1]);
			entryTime = new TimeStamp(strings[2]).Internal;
			exitPrice = Convert.ToDouble(strings[3]);
			exitTime = new TimeStamp(strings[4]).Internal;
		}
		
		public TransactionPairBinary(TransactionPairBinary other) {
			entryTime = other.entryTime;
			postedEntryTime = other.postedEntryTime;
			exitTime = other.exitTime;
			postedExitTime = other.postedExitTime;
			direction = other.direction;
			entryPrice = other.entryPrice;
			exitPrice = other.exitPrice;
			minPrice = other.minPrice;
			maxPrice = other.maxPrice;
			exitBar = other.exitBar;
			entryBar = other.entryBar;
			completed = other.completed;
			volume = other.volume;
			entryOrderId = other.entryOrderId;
			exitOrderId = other.exitOrderId;
		}
		
		public void TryUpdate(Tick tick) {
			if( !completed) {
				if( direction > 0) {
					UpdatePrice(tick.IsQuote ? tick.Bid : tick.Price);
				} else {
					UpdatePrice(tick.IsQuote ? tick.Ask : tick.Price);
				}
				exitTime = tick.Time.Internal;
			}
		}
		
		
		public void UpdatePrice(double price) {
			if( trace ) log.Trace("UpdatePrice("+price+")");
			if( price > maxPrice) {
				maxPrice = price;
			}
			if( price < minPrice) {
				minPrice = price;
			}
			exitPrice = price;
		}
		
		public void Enter( double direction, double price, TimeStamp time, TimeStamp postedTime, int bar, int entryOrderId) {
			this.direction = direction;
			this.volume = Math.Abs(direction);
			this.entryPrice = price;
			this.maxPrice = this.minPrice = entryPrice;
			this.entryTime = time.Internal;
			this.postedEntryTime = postedTime.Internal;
			this.entryBar = bar;
			this.entryOrderId = entryOrderId;
		}
		
		public void Exit( double price, TimeStamp time, TimeStamp postedTime, int bar, int exitOrderId) {
			this.volume += Math.Abs( direction);
			this.exitPrice = price;
			this.exitTime = time.Internal;
			this.postedExitTime = postedTime.Internal;
			this.exitBar = bar;
			this.completed = true;
			this.exitOrderId = exitOrderId;
		}
		
		public void Update( double price, TimeStamp time, int bar) {
			this.exitPrice = price;
			this.exitTime = time.Internal;
			this.exitBar = bar;
		}
		
		public void ChangeSize( double newSize, double price) {
			double sum = entryPrice * Direction;
			double sizeChange = newSize - Direction;
			volume += Math.Abs(sizeChange);
			double sum2 = sizeChange * price;
			double newPrice = (sum + sum2) / newSize;
			entryPrice = newPrice;
			direction = newSize;
		}
		
		public double Direction {
			get { return direction; }
		}
		
		public double EntryPrice {
			get { return entryPrice; }
		}
		
		public double ExitPrice {
			get { return exitPrice; }
		}
		
		[Obsolete("Please use TransactionPairs.GetProfitLoss() instead.",true)]
		public double ProfitLoss {
			get { return 0.0; }
		}
		
		public string ToStringHeader() {
			return "Direction,EntryBar,EntryPrice,EntryTime,ExitPrice,ExitBar,ExitTime,MaxPrice,MinPrice,ProfitLoss";
		}
		
		public override int GetHashCode() {
			string hash = direction + ":" + entryPrice + EntryTime + exitPrice + ExitTime;
			return hash.GetHashCode();
		}
		public override bool Equals(object obj) {
			if( obj.GetType() != typeof(TransactionPairBinary)) {
				return false;
			}
			TransactionPairBinary trade = (TransactionPairBinary) obj;
			return this.direction == trade.direction &&
				this.entryPrice == trade.entryPrice &&
				this.EntryTime == trade.EntryTime &&
				this.exitPrice == trade.exitPrice &&
				this.ExitTime == trade.ExitTime;
		}
		
		public double MaxPrice {
			get { return maxPrice; }
		}
		
		public double MinPrice {
			get { return - minPrice; }
		}
		
		public int EntryBar {
			get { return entryBar; }
		}
		
		public int ExitBar {
			get { return exitBar; }
		}
		
		public TimeStamp EntryTime {
			get { return new TimeStamp(entryTime); }
		}
		
		public TimeStamp ExitTime {
			get { return new TimeStamp(exitTime); }
		}
		
		public double Volume {
			get { return volume; }
		}
		
		public int EntryOrderId {
			get { return entryOrderId; }
		}
		
		public int ExitOrderId {
			get { return exitOrderId; }
		}
	}
}
