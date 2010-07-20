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
		private static readonly Log log = Factory.Log.GetLogger(typeof(TransactionPair));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		public static readonly string TIMEFORMAT = "yyyy-MM-dd HH:mm:ss.fff";
		private long entryTime;
		private long exitTime;
		private double direction;
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
			pair.entryBar = int.Parse(fields[field++]);
			pair.entryPrice = double.Parse(fields[field++]);
			pair.entryTime = TimeStamp.Parse(fields[field++]).Internal;
			pair.exitBar = int.Parse(fields[field++]);
			pair.exitPrice = double.Parse(fields[field++]);
			pair.exitTime = TimeStamp.Parse(fields[field++]).Internal;
			pair.maxPrice = double.Parse(fields[field++]);
			pair.minPrice = double.Parse(fields[field++]);
			return pair;
		}
	
		
		public bool Completed {
			get { return completed; }
			set { completed = value; }
		}
		
		public static TransactionPairBinary Create() {
			return new TransactionPairBinary();
		}
		
		public void SetProperties(string parameters)
		{
			string[] strings = parameters.Split(new char[] {','});
			Direction = Convert.ToInt32(strings[0]);
			EntryPrice = Convert.ToDouble(strings[1]);
			EntryTime = new TimeStamp(strings[2]);
			ExitPrice = Convert.ToDouble(strings[3]);
			ExitTime = new TimeStamp(strings[4]);
		}
		
		public TransactionPairBinary(TransactionPairBinary other) {
			entryTime = other.entryTime;
			exitTime = other.exitTime;
			direction = other.direction;
			entryPrice = other.entryPrice;
			exitPrice = other.exitPrice;
			minPrice = other.minPrice;
			maxPrice = other.maxPrice;
			exitBar = other.exitBar;
			entryBar = other.entryBar;
			completed = other.completed;
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
		
		public void ChangeSize( double newSize, double price) {
			double sum = entryPrice * Direction;
			double sizeChange = newSize - Direction;
			double sum2 = sizeChange * price;
			double newPrice = (sum + sum2) / newSize;
			entryPrice = newPrice;
			Direction = newSize;
		}
		
		public double Direction {
			get { return direction; }
			set { direction = value; }
		}
		
		public double EntryPrice {
			get { return entryPrice; }
			set { entryPrice = value; }
		}
		
		public double ExitPrice {
			get { return exitPrice; }
			set { UpdatePrice(value); }
		}
		
		[Obsolete("Please use TransactionPairs.GetProfitLoss() instead.",true)]
		public double ProfitLoss {
			get { return 0.0; }
		}
		
		public string ToStringHeader() {
			return "Direction,EntryBar,EntryPrice,EntryTime,ExitPrice,ExitBar,ExitTime,MaxPrice,MinPrice,ProfitLoss";
		}
		
		public override string ToString() {
			return direction + "," + entryBar + "," + entryPrice + "," + EntryTime.ToString(TIMEFORMAT) + "," +
				exitBar + "," + exitPrice + "," + ExitTime.ToString(TIMEFORMAT) + "," + maxPrice + "," + minPrice;
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
			set { entryBar = value; }
		}
		
		public int ExitBar {
			get { return exitBar; }
			set { exitBar = value; }
		}
		
		public TimeStamp EntryTime {
			get { return new TimeStamp(entryTime); }
			set { entryTime = value.Internal; }
		}
		
		public TimeStamp ExitTime {
			get { return new TimeStamp(exitTime); }
			set { exitTime = value.Internal; }
		}
	}
}
