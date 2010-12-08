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
		private long entrySerialNumber;
		private int exitOrderId;
		private long exitSerialNumber;
		private long entryTime;
		private long postedEntryTime;
		private long exitTime;
		private long postedExitTime;
		private int currentPosition;
		private int shortVolume;
		private int longVolume;
		private double entryPrice;
		private double averageEntryPrice;
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
			pair.currentPosition = int.Parse(fields[field++]);
			pair.entryOrderId = int.Parse(fields[field++]);
			pair.entrySerialNumber = long.Parse(fields[field++]);
			pair.entryBar = int.Parse(fields[field++]);
			pair.entryPrice = double.Parse(fields[field++]);
			pair.averageEntryPrice = double.Parse(fields[field++]);
			pair.entryTime = TimeStamp.Parse(fields[field++]).Internal;
			pair.postedEntryTime = TimeStamp.Parse(fields[field++]).Internal;
			pair.exitOrderId = int.Parse(fields[field++]);
			pair.exitSerialNumber = long.Parse(fields[field++]);
			pair.exitBar = int.Parse(fields[field++]);
			pair.exitPrice = double.Parse(fields[field++]);
			pair.exitTime = TimeStamp.Parse(fields[field++]).Internal;
			pair.postedExitTime = TimeStamp.Parse(fields[field++]).Internal;
			pair.maxPrice = double.Parse(fields[field++]);
			pair.minPrice = double.Parse(fields[field++]);
			pair.longVolume = int.Parse(fields[field++]);
			pair.shortVolume = int.Parse(fields[field++]);
			return pair;
		}
	
		public override string ToString() {
			return currentPosition + "," + entryOrderId + "," + entrySerialNumber + "," + entryBar + "," + entryPrice + "," + averageEntryPrice + "," + new TimeStamp(entryTime) + "," + new TimeStamp(postedEntryTime) + "," +
				exitOrderId + "," + exitSerialNumber + "," + exitBar + "," + exitPrice + "," + new TimeStamp(exitTime) + "," + new TimeStamp(postedExitTime) + "," + maxPrice + "," + minPrice + "," + longVolume + "," + shortVolume;
		}
		
		
		public bool Completed {
			get { return completed; }
		}
		
		public static TransactionPairBinary Create() {
			return new TransactionPairBinary();
		}
		
//		public void SetProperties(string parameters)
//		{
//			string[] strings = parameters.Split(new char[] {','});
//			currentPosition = Convert.ToInt32(strings[0]);
//			entryPrice = Convert.ToDouble(strings[1]);
//			averageEntryPrice = Convert.ToDouble(strings[2]);
//			entryTime = new TimeStamp(strings[3]).Internal;
//			exitPrice = Convert.ToDouble(strings[4]);
//			exitTime = new TimeStamp(strings[5]).Internal;
//		}
		
		public TransactionPairBinary(TransactionPairBinary other) {
			entryTime = other.entryTime;
			postedEntryTime = other.postedEntryTime;
			exitTime = other.exitTime;
			postedExitTime = other.postedExitTime;
			currentPosition = other.currentPosition;
			entryPrice = other.entryPrice;
			averageEntryPrice = other.averageEntryPrice;
			exitPrice = other.exitPrice;
			minPrice = other.minPrice;
			maxPrice = other.maxPrice;
			exitBar = other.exitBar;
			entryBar = other.entryBar;
			completed = other.completed;
			longVolume = other.longVolume;
			shortVolume = other.shortVolume;
			entryOrderId = other.entryOrderId;
			entrySerialNumber = other.entrySerialNumber;
			exitOrderId = other.exitOrderId;
			exitSerialNumber = other.entrySerialNumber;
		}
		
		public void TryUpdate(Tick tick) {
			if( !completed) {
				if( currentPosition > 0) {
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
		
		public void Enter( int direction, double price, TimeStamp time, TimeStamp postedTime, int bar, int entryOrderId, long entrySerialNumber) {
			this.currentPosition = direction;
			if( currentPosition > 0) {
				this.longVolume = Math.Abs(direction);
			} else {
				this.shortVolume = Math.Abs(direction);
			}
			if( trace) log.Trace("Enter long volume = " + this.longVolume + ", short volume = " + this.shortVolume);
			this.entryPrice = price;
			this.averageEntryPrice = price;
			this.maxPrice = this.minPrice = averageEntryPrice;
			this.entryTime = time.Internal;
			this.postedEntryTime = postedTime.Internal;
			this.entryBar = bar;
			this.entryOrderId = entryOrderId;
			this.entrySerialNumber = entrySerialNumber;
		}
		
		public void Exit( double price, TimeStamp time, TimeStamp postedTime, int bar, int exitOrderId, long exitSerialNumber) {
			if( currentPosition < 0) {
				this.longVolume += Math.Abs( currentPosition);
			} else {
				this.shortVolume += Math.Abs( currentPosition);
			}
			if( trace) log.Trace("Exit long volume = " + this.longVolume + ", short volume = " + shortVolume + ", Direction = " + this.Direction);
			this.exitPrice = price;
			this.exitTime = time.Internal;
			this.postedExitTime = postedTime.Internal;
			this.exitBar = bar;
			this.completed = true;
			this.exitOrderId = exitOrderId;
			this.exitSerialNumber = exitSerialNumber;
		}
		
		public void Update( double price, TimeStamp time, int bar) {
			this.exitPrice = price;
			this.exitTime = time.Internal;
			this.exitBar = bar;
		}
		
		public void ChangeSize( int newSize, double price) {
			var sum = averageEntryPrice.ToLong() * currentPosition; // 1951840000000000
			var sizeChange = newSize - currentPosition;      // -6666
			var sum2 = sizeChange * price.ToLong();    // -650394954000000
			var newPrice = ((sum + sum2) / newSize).ToDouble();     // 97603498275
			if( sizeChange > 0) {
				longVolume += Math.Abs(sizeChange);
			} else {
				shortVolume += Math.Abs(sizeChange);
			}
			averageEntryPrice = newPrice;
			currentPosition = newSize;
			if( trace) log.Trace("Price = " + price + ", averageEntryPrice = " + averageEntryPrice + ", CurrentPosition = " + currentPosition + ", NewSize = " + newSize + ", Direction = " + Direction + ", sizeChange = " + sizeChange + ", Long volume = " + this.longVolume + ", short volume = " + shortVolume);
		}
		
		public int Direction {
			get {
				return Math.Max(longVolume,shortVolume) * Math.Sign(currentPosition);
			}
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
			string hash = Direction + ":" + EntryPrice + EntryTime + ExitPrice + ExitTime;
			return hash.GetHashCode();
		}
		public override bool Equals(object obj) {
			if( obj.GetType() != typeof(TransactionPairBinary)) {
				return false;
			}
			TransactionPairBinary trade = (TransactionPairBinary) obj;
			var entryPriceMatch = this.currentPosition == trade.currentPosition ? this.averageEntryPrice == trade.averageEntryPrice : true;
			return this.longVolume == trade.longVolume &&
				this.shortVolume == trade.shortVolume &&
				entryPriceMatch &&
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
		
		public int Volume {
			get { return longVolume + shortVolume; }
		}
		
		public int EntryOrderId {
			get { return entryOrderId; }
		}
		
		public int ExitOrderId {
			get { return exitOrderId; }
		}
		
		public double AverageEntryPrice {
			get { return averageEntryPrice; }
		}
		
		public int CurrentPosition {
			get { return currentPosition; }
		}
	}
}
