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
using System.IO;
using TickZoom.Api;

namespace TickZoom.TickUtil
{

	/// <summary>
	/// Description of TickDOM.
	/// </summary>
	public class TickBox : TickIO
	{
		private TickImpl tick;
		
		public void Copy(TickIO tickIO, byte contentMask){
			tick.Copy(tickIO,contentMask);
		}
		
		public void Copy(TickIO tickIO){
			tick.Copy(tickIO);
		}
		
		public void Initialize() {
			tick.Initialize();
		}
		
		public void SetTime(TimeStamp utcTime)
		{
			tick.SetTime(utcTime);
		}
		
		public void SetQuote(double dBid, double dAsk)
		{
			tick.SetQuote(dBid,dAsk);
		}
		
		public void SetQuote(double dBid, double dAsk, ushort bidSize, ushort askSize)
		{
			tick.SetQuote(dBid,dAsk,bidSize,askSize);
		}
		
		public void SetTrade(double price, int size)
		{
			tick.SetTrade(price,size);
		}
		
		public void SetTrade(TradeSide side, double price, int size)
		{
			tick.SetTrade(side,price,size);
		}
		
		public void SetDepth(ushort[] bidSize, ushort[] askSize) {
			tick.SetDepth(bidSize,askSize);
		}
		
		public void SetSymbol( ulong lSymbol) {
			tick.SetSymbol(lSymbol);
		}
		
		public void init(TickIO tickIO, byte contentMask){
			tick.Copy(tickIO,contentMask);
		}
		
		[Obsolete("Please use multiple init methods instead of this one.",true)]
		public void init(TimeStamp utcTime, double dBid, double dAsk) {
			tick.init(utcTime,dBid,dAsk);
		}

		[Obsolete("Please use multiple init methods instead of this one.",true)]
		public void init(TimeStamp utcTime, double price, int size) {
			tick.init(utcTime,price,size);
		}

		[Obsolete("Please use multiple init methods instead of this one.",true)]
		public void init(TimeStamp utcTime, byte side, double price, int size) {
			init(utcTime, side, price, size);
		}
		
		[Obsolete("Please use multiple init methods instead of this one.",true)]
		public void init(TimeStamp utcTime, byte side, double dPrice, int size, double dBid, double dAsk) {
			init(utcTime, side, dPrice, size, dBid, dAsk);
		}

		[Obsolete("Please use multiple init methods instead of this one.",true)]
		public void init(TimeStamp utcTime, byte side, double price, int size, double dBid, double dAsk, ushort[] bidSize, ushort[] askSize) {
			init(utcTime, side, price, size, dBid, dAsk, bidSize, askSize);
		}

		public TickImpl Tick {
			get { return tick; }
			set { tick = value; }
		}

		public int BidDepth {
			get { return tick.BidDepth; }
		}
		
		public int AskDepth {
			get { return tick.AskDepth; }
		}
		
		public override string ToString() {
			return tick.ToString();
		}
		
		public void ToWriter(MemoryStream writer) {
			tick.ToWriter(writer);
		}
		
		public byte DataVersion {
			get { return tick.DataVersion; }
		}
		
		public double Bid {
			get { return tick.Bid; }
		}
		
		public double Ask {
			get { return tick.Ask; }
		}
		
		public TradeSide Side {
			get { return tick.Side; }
		}
		
		public double Price {
			get { return tick.Price; }
		}
		
		public int Size {
			get { return tick.Size; }
		}
		
		public int Sentiment {
			get { return tick.Sentiment; }
		}
		
		public int Volume {
			get { return tick.Size; }
		}
		
		public ushort AskLevel(int level) {
			return tick.AskLevel(level);
		}
		
		public ushort BidLevel(int level) {
			return tick.BidLevel(level);
		}
		
		public TimeStamp Time {
			get { return tick.Time; }
		}
		
		public int CompareTo(TickBox other)
		{
			return tick.CompareTo(ref other.tick);
		}
		
		public override int GetHashCode()
		{
			return tick.GetHashCode();
		}
		
		public override bool Equals(object other)
		{
			TickBox box = other as TickBox;
			return CompareTo(box) == 0;
		}
		
		public bool Equals(TickBox other)
		{
			return CompareTo(other) == 0;
		}
		
		public int FromReader(byte version, BinaryReader reader)
		{
			return tick.FromReader(version,reader);
		}
		
		public void FromReader(MemoryStream reader)
		{
			tick.FromReader(reader);
		}
		
		public TickBinary Extract() {
			return tick.Extract();
		}
		
		public void Inject(TickBinary tickBinary) {
			tick.Inject(tickBinary);
		}
		
		
		public void Inject(TickIO tickIO) {
			tick.Copy(tickIO,tickIO.ContentMask);
		}
		
		public byte ContentMask {
			get { return tick.ContentMask; }
		}
		
		public long lBid {
			get { return tick.lBid; }
		}
		
		public long lAsk {
			get { return tick.lAsk; }
		}
		
		public long lPrice {
			get { return tick.lPrice; }
		}
		
		public bool IsRealTime {
			get { return tick.IsRealTime; }
		}
		
		public TimeStamp UtcTime {
			get { return tick.UtcTime; }
		}
		
		public ulong lSymbol {
			get { return tick.lSymbol; }
		}
		
		public string Symbol {
			get { return tick.Symbol; }
		}
		
		public int DomLevels {
			get { return tick.DomLevels; }
		}
		
		public bool IsTrade {
			get { return tick.IsTrade; }
		}
		
		public bool IsQuote{
			get { return tick.IsQuote; }
		}
		
		public bool IsSimulateTicks {
			get { return tick.IsSimulateTicks; }
			set { tick.IsSimulateTicks = value; }
		}
		
		public object ToPosition() {
			return tick.Time;
		}
		
//		public int Length {
//			get { return tick.Length; }
//		}
		

	}
}
