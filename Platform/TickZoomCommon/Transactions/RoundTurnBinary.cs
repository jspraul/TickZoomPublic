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

namespace TickZoom.Transactions
{
	/// <summary>
	/// Description of Trade.
	/// </summary>
	[Obsolete("Please use TransactionPairBinary instead.",true)]
	public struct RoundTurnBinary 
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(TransactionPair));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		public static readonly string TIMEFORMAT = "yyyy-MM-dd HH:mm:ss.fff";
		private TimeStamp entryTime;
		private TimeStamp exitTime;
		private double direction;
		private double entryPrice;
		private int entryBar;
		private double exitPrice;
		private int exitBar;
		private double maxPrice;
		private double minPrice;
		private bool completed;
		
		public bool Completed {
			get { return completed; }
			set { completed = value; }
		}
		
		public static RoundTurnBinary Create() {
			return new RoundTurnBinary();
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
		
		public RoundTurnBinary(RoundTurnBinary other) {
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
		
		public void UpdatePrice(Tick tick) {
			UpdatePrice(tick.Bid);
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
			double sizeChange = newSize - Direction;
			double sum = entryPrice * Direction;
			sum += sizeChange * price;
			entryPrice = sum / newSize;
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
			return "Direction,EntryPrice,EntryTime,ExitPrice,ExitTime,ProfitLoss";
		}
		public override string ToString() {
			return direction + "," + entryPrice + "," + EntryTime.ToString(TIMEFORMAT) + "," +
				exitPrice + "," + ExitTime.ToString(TIMEFORMAT);
		}
		public override int GetHashCode() {
			string hash = direction + ":" + entryPrice + EntryTime + exitPrice + ExitTime;
			return hash.GetHashCode();
		}
		public override bool Equals(object obj) {
			if( obj.GetType() != typeof(RoundTurnBinary)) {
				return false;
			}
			RoundTurnBinary trade = (RoundTurnBinary) obj;
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
			get { return entryTime; }
			set { entryTime = value; }
		}
		
		public TimeStamp ExitTime {
			get { return exitTime; }
			set { exitTime = value; }
		}
	}


}
