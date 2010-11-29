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
using System.Text;
using TickZoom.Api;

namespace TickZoom.FIX
{
	public class FIXMessage4_4 : FIXTMessage1_1 {
		public FIXMessage4_4(string sender,string destination) : base("FIX.4.4",sender,destination) {
		}
		/// <summary>
		/// 1 Account mnemonic as agreed between buy and sell sides, e.g. broker and institution or investor/intermediary and fund manager.
		/// </summary>
		public void SetAccount(string value) {
			Append(1,value);
		}
		/// <summary>
		/// 710	Unique identifier for the position maintenance request as assigned by the submitter
		/// </summary>
		public void SetPositionRequestId(int value) {
			Append(710,value);
		}
		/// <summary>
		/// 724 Unique identifier for the position maintenance request as assigned by the submitter
		///					0	=	Positions
		///					1	=	Trades
		///					2	=	Exercises
		///					3	=	Assignments
		/// </summary>
		public void SetPositionRequestType(int value) {
			Append(724,value);
		}
		
		/// <summary>
		/// 453 Number of repeated parties.
		/// </summary>
		
		public void SetPartiesRepeat(int value) {
			Append(453,value);
		}
		
		/// <summary>
		/// 581 int Type of account associated with an order
		///					1	=	AccountCustomer
		///					2	=	AccountNonCustomer
		///					3	=	HouseTrader
		///					4	=	FloorTrader
		///					6	=	AccountNonCustomerCross
		///					7	=	HouseTraderCross
		///					8	=	JointBOAcct
		/// </summary>
		public void AccountType(int value) {
			Append(581,value);
		}
		/// <summary>
		/// 263		@SubReqTyp	charSubscription Request Type
		///			0	=	Snapshot
		///			1	=	SnapshotUpdate
		///			2	=	Unsubscribe		
		/// </summary>
		public void SetSubscriptionRequestType(int value) {
			Append(263,value);
		}
		/// <summary>
		/// 584	String	Value assigned by issuer of Mass Status Request to uniquely identify the request
		/// </summary>
		public void SetMassStatusRequestID(string value) {
			Append(584,value);
		}
		/// <summary>
		/// 584	String	Value assigned by issuer of Mass Status Request to uniquely identify the request
		/// </summary>
		public void SetMassStatusRequestID(TimeStamp value) {
			Append(584,value);
		}
		/// <summary>
		/// 585	 int	Mass Status Request Type
		///			1	=	StatusSecurity
		///			2	=	StatusUnderlyingSecurity
		///			3	=	StatusProduct
		///			4	=	StatusCFICode
		///			5	=	StatusSecurityType
		///			6	=	StatusTrdSession
		///			7	=	StatusAllOrders
		///			8	=	StatusPartyID		
		/// </summary>
		public void SetMassStatusRequestType(int value) {
			Append(585,value);
		}
		/// <summary>
		/// 11 A unique identifier for an order within the trading day		
		/// </summary>
		public void SetClientOrderId(string value) {
			Append(11,value);
		}
		
		/// <summary>
		///	14 Cumulative Quantity
		/// </summary>
		public void SetCumulativeQuantity(int value ) {
			Append(14,value);
		}
		
		/// <summary>
		/// 41 Original client order id to be canceled or cancel/replaced.
		/// </summary>
		public void SetOriginalClientOrderId(string value) {
			Append(41,value);
		}
		/// <summary>
		/// 21 Instructions on how to handle this order; the only valid value is 1 (automated execution)		
		/// </summary>
		public void SetHandlingInstructions(int value) {
			Append(21,value);
		}

		/// <summary>
		///	31 Last Price
		/// </summary>
		public void SetLastPrice(double value ) {
			Append(31,value);
		}
		
		/// <summary>
		///	32 Last Quantity
		/// </summary>
		public void SetLastQuantity(int value ) {
			Append(32,value);
		}

		/// <summary>
		/// 37 The order id from the FIX server.
		/// </summary>
		public void SetOrderId(string value) {
			Append(37, value);
		}
		
		/// <summary>
		/// 38 Y The total number of shares to be traded	
		/// </summary>
		public void SetOrderQuantity(int value) {
			Append(38, value);
		}
		
		/// <summary>
		/// 39 The status of this execution order.
		/// </summary>
		public void SetOrderStatus(string value) {
			Append(39, value);
		}
		
		/// <summary>
		/// 40 The type of order which is placed; valid values are:
		/// 1=Market, 2=Limit, 3=Stop,4=StopLimit, P=TrailingStop, T=TTO		
		/// </summary>
		public void SetOrderType(string value) {
			Append(40, value);
		}
		/// <summary>
		/// 40 The type of order which is placed; valid values are:
		/// 1=Market, 2=Limit, 3=Stop,4=StopLimit, P=TrailingStop, T=TTO		
		/// </summary>
		public void SetOrderType(int value) {
			Append(40, value);
		}
		
		/// <summary>
		/// 52 The time this order was sent
		/// </summary>
		public void SetSendingTime(string value) {
			Append(52, value);
		}
		
		/// <summary>
		/// 55 Stock Symbol, or Currency Pair
		/// </summary>
		public void SetSymbol(string value) {
			Append(55, value);
		}
		
		///<summary>
		/// 54 The action to take on the order. Valid entries are: 
		/// 1=Buy, 2=Sell, 5=Sell Short, and 6=Sell Short Exempt
		/// </summary>
		public void SetSide(int value ) {
			Append(54, value);
		}
		
		/// <summary>
		///	58 Error or other message text.
		/// </summary>
		public void SetText(string value ) {
			Append(58,value);
		}
		
		/// <summary>
		///	59 Orders are assumed to be Day orders unless specified
		///	otherwise. Valid values are: 0= Day , 1= GTC (req'd for
		///	all Forex orders), 2= At the open, 3= Immediate or
		///	Cancel (IOC), 4= Fill or Kill (FOK), 6= Good Till Date
		///	(GTD), 9= Day + (only available for ARCA route
		///	touching pre-market, intraday regular market hours and
		///	after hours trading		
		/// </summary>
		public void SetTimeInForce(int value ) {
			Append(59,value);
		}
		
		/// <summary>
		///	77 The effect on the current position.
		/// </summary>
		public void SetPositionEffect(string value) {
			Append(77,value);
		}
		
		/// <summary>
		///	100 MBTX or ECN when alternate destinations are permitted		
		/// </summary>
		public void SetDestination(string value ) {
			Append(100,value);
		}
		
		/// <summary>
		///	150 Execution type.
		/// </summary>
		public void SetExecutionType(string value ) {
			Append(150,value);
		}
		
		/// <summary>
		///	151 Leaves Quantity
		/// </summary>
		public void SetLeavesQuantity(double value ) {
			Append(151,value);
		}
		
		/// <summary>
		///	60 Transaction time.
		/// </summary>
		public void SetTransactTime(TimeStamp value ) {
			Append(60,value);
		}
		
		/// <summary>
		///	553 end user who entered the trade should have their username specified here	
		/// This method uses the "sender" field name as the username here.
		/// </summary>
		public void SetUserName() {
			Append(553,Sender);
		}
		
		///<summary>
		/// 704 Short quantity
		/// </summary>
		public void SetShortQty(int value ) {
			Append(704, value);
		}
		
		
		///<summary>
		/// 705 Long quantity
		/// </summary>
		public void SetLongQty(int value ) {
			Append(705, value);
		}
		
		/// <summary>
		///	114	Boolean	Indicates whether the broker is to locate the stock in conjunction with a short sell order.
		/// </summary>
		/// <param name="value">
		///	N	=	Indicates the broker is not required to locate
		///	Y	=	Indicates the broker is responsible for locating the stock
		/// </param>
		public void SetLocateRequired(string value ) {
			Append(114,value);
		}
		/// <summary>
		///	44	Price per unit of quantity (e.g. per share)	}
		/// </summary>
		public void SetPrice(double value) {
			Append(44,value);
		}
		
		/// <summary>
		///	99	Required for OrdType = "Stop" or OrdType = "Stop limit".
		/// </summary>
		public void SetStopPrice(double value) {
			Append(99,value);
		}
		
		/// <summary>
		///	47	char	Note that the name of this field is changing to 'OrderCapacity' as Rule80A is a very US market-specific term. Other world markets need to convey similar information, however, often a subset of the US values. See the 'Rule80A (aka OrderCapacity) Usage by Market' appendix for market-specific usage of this field.	FIX.4.3	
		///	A	=	Agency single order
		///	B	=	Short exempt transaction (refer to A type)
		///	C	=	Proprietary, Non-Algorithmic Program Trade (non-index arbitrage)
		///	D	=	Program order, index arb, for Member firm/org
		///	E	=	Short Exempt Transaction for Principal (was incorrectly identified in the FIX spec as "Registered Equity Market Maker trades")
		///	F	=	Short exempt transaction (refer to W type)
		///	H	=	Short exempt transaction (refer to I type)
		///	I	=	Individual Investor, single order
		///	J	=	Proprietary, Algorithmic Program Trading (non-index arbitrage)
		///	K	=	Agency, Algorithmic Program Trading (non-index arbitrage)
		///	L	=	Short exempt transaction for member competing market-maker affliated with the firm clearing the trade (refer to P and O types)
		///	M	=	Program Order, index arb, for other member
		///	N	=	Agent for other Member, Non-Algorithmic Program Trade (non-index arbitrage)
		///	O	=	Proprietary transactions for competing market-maker that is affiliated with the clearing member (was incorrectly identified in the FIX spec as "Competing dealer trades")
		///	P	=	Principal
		///	R	=	Transactions for the account of a non-member compting market-maker (was incorrectly identified in the FIX spec as "Competing dealer trades")
		///	S	=	Specialist trades
		///	T	=	Transactions for the account of an unaffiliated member's competing market-maker (was incorrectly identified in the FIX spec as "Competing dealer trades")
		///	U	=	Agency, Index Arbitrage (includes Individual, Index Arbitrage trades)
		///	W	=	All other orders as agent for other member
		///	X	=	Short exempt transaction for member competing market-maker not affiliated with the firm clearing the trade (refer to W and T types)
		///	Y	=	Agency, Non-Algorithmic Program Trade (non-index arbitrage)
		///	Z	=	Short exempt transaction for non-member competing market-maker (refer to A and R types)
		/// </summary>
		public void SetOrderCapacity(string value) {
			Append(47,value);
		}
	}
}
