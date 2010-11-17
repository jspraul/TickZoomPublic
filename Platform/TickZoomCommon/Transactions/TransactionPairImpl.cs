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

namespace TickZoom.Transactions
{
	/// <summary>
	/// Description of RoundTurnImpl.
	/// </summary>
	public class TransactionPairImpl : TransactionPair
	{
		TransactionPairBinary binary;
		
		public TransactionPairImpl()
		{
		}
		
		public TransactionPairImpl(TransactionPairBinary binary)
		{
			this.binary = binary;
		}
		
		public void init( TransactionPairBinary binary) {
			this.binary = binary;
		}
		
		public double Direction {
			get {
				return binary.Direction;
			}
		}
		
		public double Volume {
			get {
				return binary.Volume;
			}
		}
		
		public double EntryPrice {
			get {
				return binary.EntryPrice;
			}
		}
		
		public double ExitPrice {
			get {
				return binary.ExitPrice;
			}
		}
		
		[Obsolete("Please use TransactionPairs.GetProfitLoss() instead.",true)]
		public double ProfitLoss {
			get { return 0.0; }
		}
		
		public double MaxFavorable {
			get {
				return binary.MaxPrice;
			}
		}
		
		public double MaxAdverse {
			get {
				return binary.MinPrice;
			}
		}
		
		public int EntryBar {
			get {
				return binary.EntryBar;
			}
		}
		
		public int ExitBar {
			get {
				return binary.ExitBar;
			}
		}
		
		public TickZoom.Api.TimeStamp EntryTime {
			get {
				return binary.EntryTime;
			}
		}
		
		public TickZoom.Api.TimeStamp ExitTime {
			get {
				return binary.ExitTime;
			}
		}
		
		public bool Completed {
			get {
				return binary.Completed;
			}
		}
		
		public void SetProperties(string parameters)
		{
			binary.SetProperties(parameters);
		}
		
		public void ChangeSize(int newSize, double price)
		{
			binary.ChangeSize(newSize, price);
		}
		
		public string ToStringHeader()
		{
			return binary.ToStringHeader();
		}
		
		public override string ToString()
		{
			return binary.ToString();
		}
	}
}
