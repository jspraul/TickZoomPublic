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

namespace TickZoom.Api
{
	public interface SymbolInfo : IEquatable<SymbolInfo>
	{
		/// <summary>
		/// The actual text value of the symbol.
		/// </summary>
		string Symbol {
			get;
		}
	
		/// <summary>
		/// The universal symbol ties different symbols together from different
		/// data providers and brokers which actually represent the same instrument.
		/// In that case, they will all have the same UnivesalSymbol.
		/// </summary>
		SymbolInfo UniversalSymbol {
			get;
		}
	
		/// <summary>
		/// The binary identifier is a unique number assigned to every symbol
		/// each time the system starts. Therefore this number can change any time
		/// a symbol is added or removed from the dictionary upon the next time it
		/// restarts. For that reason it is useless to use this in user strategy code.
		/// It's purpose internally to TickZoom is to greatly increase performance
		/// by refering to a native number rather than a string to represet symbols.
		/// </summary>
		long BinaryIdentifier {
			get;
		}
		
		/// <summary>
		/// The time of day that the primary session for this symbol starts.
		/// </summary>
		Elapsed SessionStart {
			get;
			set;
		}
		
		/// <summary>
		/// The time of day that the primary session for this symbol ends.
		/// </summary>
		Elapsed SessionEnd {
			get;
			set;
		}
		
		/// <summary>
		/// Returns a fractional value for the minimum price move for the symbol.
		/// For example: And U.S. stock's minimum tick will be 0.01
		/// </summary>
		double MinimumTick {
			get;
		}
		
		/// <summary>
		/// The currency value of a full point of a symbol in its denominated currency.
		/// You can simply multiple FullPointValue by the any price of the symbol to get the
		/// full currency value of that price.
		/// </summary>
		double FullPointValue {
			get;
		}
		
		void CopyProperties(object obj);
	
		[Obsolete("Please create your data with the IsSimulateTicks flag set to true instead of this property.",true)]
		bool SimulateTicks {
			get;
		}
		
		/// <summary>
		/// Sets the divisor for lots when collecting Depth of Market data for
		/// every tick. This helps to standardize across different markets which 
		/// use different sizes of orders to indicate 1 full lot. So for stocks this
		/// should ordinarily be set to 100 whereas for Forex the standard lot size
		/// is 10000.
		/// </summary>
		int Level2LotSize {
			get;
		}
		
		/// <summary>
		/// This increment in price between each level of Depth of Market
		/// which you wish to collect.
		/// </summary>
		double Level2Increment {
			get;
		}
		
 		/// <summary>
 		/// Eliminate reporting of orders on Level II less
		/// than a the lot size minimum.
 		/// </summary>
 		int Level2LotSizeMinimum {
			get;
		}
 		
 		/// <summary>
 		/// With which other symbols does this one get drawn on a chart? Returns
 		/// a group number where 0 means never draw this symbol on any chart.
 		/// All symbols with that same ChartGroup number will appear on the same
 		/// chart. You can only set this property inside your Loader before
 		/// the engine initializes the portfolios and strategies.
 		/// </summary>
 		int ChartGroup {
 			get;
 		}
 		
 		/// <summary>
 		/// Determines whether Level1 or Level2 or both types of data should
 		/// be used to build the data for this symbol.
 		/// </summary>
 		QuoteType QuoteType {
 			get;
 		}
 		
 		/// <summary>
 		/// What type of time and sales data to capture and stream.
 		/// </summary>
 		TimeAndSales TimeAndSales {
 			get;
 		}
 		
 		/// <summary>
 		/// What time zone to use for displaying of date for this symbol.
 		/// Optional values are "Exchange", "UTC", or "Local"
 		/// "Exchange" means to use the time zone set by the TimeZone property
 		/// and represents the time zone of the exchange where the symbol is traded.
 		/// "Local" means to use the time zone set on the current PC.
 		/// "UTC" means to use the UTC time zone.
 		/// </summary>
 		string DisplayTimeZone {
 			get;
 		}
 		
 		/// <summary>
 		/// The time zone of the exchange where the symbol is traded.
 		/// </summary>
 		string TimeZone {
 			get;
 		}
 		
 		/// <summary>
 		/// Determines whether limit orders will get held and converted
 		/// to market orders or else sent to the broker for execution.
 		/// </summary>
 		bool UseSyntheticLimits {
 			get;
 		}
 		
 		/// <summary>
 		/// Determines whether market orders will get held and converted
 		/// to market orders or else sent to the broker for execution.
 		/// </summary>
 		bool UseSyntheticMarkets {
 			get;
 		}
 		
 		/// <summary>
 		/// Determines whether stop orders will get held and converted
 		/// to market orders or else sent to the broker for execution.
 		/// </summary>
 		bool UseSyntheticStops {
 			get;
 		}
 		
 		/// <summary>
 		/// Returns the ProfitLoss calculation for this symbols. It's used
 		/// primarily to correctly calculate transaction costs for trades.
 		/// </summary>
 		ProfitLoss ProfitLoss {
 			get;
 		}
 		
 		/// <summary>
 		/// Defines the formula for calculating commission for this symbol.
 		/// This property is a text type which can be used either as a name
 		/// for the "type" of commission calculation to use or a specific
 		/// numeric value of commission to use.
 		/// </summary>
 		string Commission {
 			get;
 		}
 		
 		/// <summary>
 		/// Serves as a parameter into the formula for calculating commission
 		/// specified by Commission.  This property is a text type which can
 		/// be used either as a name for the "type" of fees or a numerical
 		/// value of the fees.
 		/// </summary>
 		string Fees {
 			get;
 		}
 		
 		/// <summary>
 		/// Serves as a parameter into the formula for calculating commission
 		/// specified by Commission.  This property is a text type which can
 		/// be used either as a name for the "type" of slippage calculation
 		/// or else a numerical	value to use for slippage.
 		/// </summary>
 		string Slippage {
 			get;
 		}
 		
 		/// <summary>
 		/// Used by brokers to determine how to route the orders. Is a text
 		/// property which may be used differently by different brokers.
 		/// </summary>
 		string Destination {
 			get;
 		}
 		
 		/// <summary>
 		/// Controls the maximum possible position size for the FIX Pre-Trade Risk 
 		/// Management filter.  If any individual order quantity will be sufficient
 		/// to push the position size beyond this maximum, then the filter will block
 		/// that order and shutdown trading. So this scenario is for a fail-safe protection
 		/// of from runaway order looping.
 		/// </summary>
 		double MaxPositionSize {
 			get;
 		}
 		
 		/// <summary>
 		/// Controls the maximum possible order size for the FIX Pre-Trade Risk 
 		/// Management filter.  If any individual order quantity exceed this setting,
 		/// then the filter blocks that order and shuts down trading.
 		/// </summary>
 		double MaxOrderSize {
 			get;
 		}
 		
 		/// <summary>
 		/// Used by validation and loading of data to ensure parsing of prices yields
 		/// appropriate values.
 		/// </summary>
 		double MaxValidPrice {
 			get;
 		}
	}
}