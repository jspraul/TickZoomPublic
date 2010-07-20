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
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using MBTCOMLib;
using MBTORDERSLib;
using MBTQUOTELib;
using TickZoom.Api;

namespace TickZoom.MBTrading
{
	/// <summary>
	/// Description of Instrument.
	/// </summary>
	public class InstrumentReader : IMbtQuotesNotify
    {
		private static readonly Log log = Factory.Log.GetLogger(typeof(InstrumentReader));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		public Log orderLog = Factory.Log.GetLogger(typeof(InstrumentReader));
        public int lastChangeTime;
        public object tsDataLocker = new object();
        public object quotesLocker = new object();
        public object level2Locker = new object();
        Level2Collection level2Bids;
        Level2Collection level2Asks; 
        private MbtOrderClient m_OrderClient = null;
        private MbtQuotes m_Quotes = null;
        private MbtAccount m_account = null;
        private MbtPosition m_position = null;
        private bool advised = false;
        private Thread quoteThread = null;
        private bool cancelThread = false;
        private Receiver receiver;
        private double lastBid;
        
		public double LastBid {
			get { return lastBid; }
		}
        private double lastAsk;
        
		public double LastAsk {
			get { return lastAsk; }
		}
        private int lastBidSize;
        private int lastAskSize;
        TickIO lastTick;
        TickIO tick;
		SymbolInfo symbol;
        Dictionary<string,MbtOpenOrder> m_orders = new Dictionary<string,MbtOpenOrder>();
        	
        public InstrumentReader(MbtOrderClient orderClient, MbtQuotes quotes, SymbolInfo instrument)
        {
        	m_OrderClient = orderClient;
        	m_Quotes = quotes;
        	this.symbol = instrument;
        	lastChangeTime = Environment.TickCount;
        }
        
        public void Initialize() {
        	tick = Factory.TickUtil.TickIO();
            lastTick = Factory.TickUtil.TickIO();
            tick.SetSymbol(symbol.BinaryIdentifier);
            this.level2Bids = new Level2Collection(this, symbol, enumMarketSide.msBid);
            this.level2Asks = new Level2Collection(this, symbol, enumMarketSide.msAsk);
        }
        
        public void ProcessTimeAndSales(ref TSRECORD pTimeAndSales) {
        	if( trace) log.Trace( "TimeAndSale: " + 
        	                     pTimeAndSales.bstrExchange + ", " +
        	                     pTimeAndSales.bstrSymbol + ", " +
        	                     pTimeAndSales.cond + ", " +
        	                     pTimeAndSales.dPrice + ", " +
        	                     pTimeAndSales.lSize + ", " +
        	                     pTimeAndSales.lType + ", " +
        	                     pTimeAndSales.status + ", " +
        	                     pTimeAndSales.tick + ", " +
        	                     pTimeAndSales.type + ", " +
        	                     pTimeAndSales.UTCDateTime );
			int InsideMarketHours = 30030;
			if( pTimeAndSales.status == enumTickStatus.tsNormal &&
			    pTimeAndSales.type == enumTickType.ttTradeTick &&
			    pTimeAndSales.cond == enumTickCondition.tcTrade_RegularSale &&
			    pTimeAndSales.lType == InsideMarketHours) {
				CreateTick();
				lastAsk = lastBid = pTimeAndSales.dPrice;
		    	CheckSignalSync();
				tick.SetTrade(pTimeAndSales.dPrice, pTimeAndSales.lSize);
				if( Symbol.QuoteType == QuoteType.Level1) {
					TryAddLevel1();
				} else if( Symbol.QuoteType == QuoteType.Level2) {
					TryAddLevel2();
				}
				SendTick();
			}
        }
        
        private void CreateTick() {
        	TimeStamp timeStamp = TimeStamp.UtcNow;
			tick.Initialize();
			tick.SetSymbol(symbol.BinaryIdentifier);
			tick.SetTime(timeStamp);
        }
        
        private void SendTick() {
    		lastTick.Copy(tick,tick.ContentMask);
    		TickBinary binary = new TickBinary();
    		binary = tick.Extract();
    		lastChangeTime = Environment.TickCount;
    		receiver.OnEvent(symbol,(int)EventType.Tick,binary);
        	if( debug) log.Debug( "Sent Tick: " + tick);
        }
        
        public void ProcessLevel1(ref QUOTERECORD pQuote) {
        	if( trace) log.Trace( "ProcessQuote: " + 
        	                     pQuote.bstrCompany + ", " +
        	                     pQuote.bstrMarket + ", " +
        	                     pQuote.bstrSymbol + ", " +
        	                     pQuote.bstrUnderlier + ", " +
        	                     pQuote.dAsk + ", " +
        	                     pQuote.dBid + ", " +
        	                     pQuote.dLast + ", " +
        	                     pQuote.dMarginMult + ", " +
        	                     pQuote.dStrikePrice + ", " +
        	                     pQuote.lAskSize + ", " +
        	                     pQuote.lBidSize + ", " +
        	                     pQuote.lContractSize + ", " +
        	                     pQuote.lExpMonth + ", " +
        	                     pQuote.lExpYear + ", " +
        	                     pQuote.lFlags + ", " +
        	                     pQuote.lLastSize + ", " +
        	                     pQuote.putcall + ", " +
        	                     pQuote.tick);
			lastBid = pQuote.dBid;
			lastAsk = pQuote.dAsk;
			lastBidSize = pQuote.lBidSize;
			lastAskSize = pQuote.lAskSize;
	    	CheckSignalSync();
			if( lastBid != lastTick.Bid || lastAsk != lastTick.Ask) {
	        	CreateTick();
	        	if( TryAddLevel1() ) {
	        		SendTick();
	        	}
			}
        }
        
        private bool TryAddLevel1() {
        	if( lastAsk == 0 || lastBid == 0 || lastAskSize == 0 || lastBidSize == 0) return false;
			tick.SetQuote(lastBid, lastAsk, (ushort) lastBidSize, (ushort) lastAskSize);
			if( trace) log.Trace("Level1 SetQuote()");
			return true;
		}

        public void ProcessLevel2(LEVEL2RECORD pRec)
        {       	
        	switch( pRec.side) {
        		case enumMarketSide.msAsk:
	        		level2Asks.Process(pRec);
        			break;
        		case enumMarketSide.msBid:
	        		level2Bids.Process(pRec);
        			break;
        		default:
        			throw new ApplicationException("Invalid Level II Side");
        	}
			lastBid = level2Bids.LastPrice;
			lastAsk = level2Asks.LastPrice;
	    	CheckSignalSync();
        	if( Environment.TickCount > lastChangeTime + 30) {
        		if( level2Bids.HasChanged || level2Asks.HasChanged ) {
	        		LogLevel2Change(false,TradeSide.Unknown,0,0);
	        	}
			}
        }
        
        public void Connect()
        {
        	Disconnect();
            try
            {
            	cancelThread = false;
	            quoteThread = new Thread(new ThreadStart(AdviseSymbols));
	            quoteThread.IsBackground = true;
	            quoteThread.Name = "AdviseSymbol "+symbol.Symbol;
	            quoteThread.Priority = ThreadPriority.BelowNormal;
	            quoteThread.Start();
            }
            catch (Exception e)
            {
                log.Notice(string.Format("Could not AdviseSymbol {0} - {1}!\n", Symbol.Symbol, e.Message));
            }
        }
        
        private void AdviseSymbols() {
            if (Level2Bids.Count > 0) Level2Bids.Clear();
            if (Level2Asks.Count > 0) Level2Asks.Clear();
            if( debug) log.Debug("Advising symbol for " + Symbol.Symbol + " with FeedType " + Symbol.QuoteType + " and TimeAndSales " + Symbol.TimeAndSales);
            switch( Symbol.QuoteType) {
            	case QuoteType.Level1:
					m_Quotes.AdviseSymbol(this, Symbol.Symbol, (int)enumQuoteServiceFlags.qsfLevelOne);
					break;
				case QuoteType.Level2:
					m_Quotes.AdviseSymbol(this, Symbol.Symbol, (int)enumQuoteServiceFlags.qsfLevelTwo);
					break;
				case QuoteType.None:
					break;
            }
            if( Symbol.TimeAndSales == TimeAndSales.ActualTrades) {
            	m_Quotes.AdviseSymbol(this, Symbol.Symbol, (int)enumQuoteServiceFlags.qsfTimeAndSales);
            }
            	
            advised = true;
            while(!cancelThread) {
            	Application.DoEvents();
            	Thread.Sleep(5);
    		}           
            m_Quotes.UnadviseSymbol(this, Symbol.Symbol, (int)enumQuoteServiceFlags.qsfLevelTwo);
            m_Quotes.UnadviseSymbol(this, Symbol.Symbol, (int)enumQuoteServiceFlags.qsfLevelOne);
            Level2Bids.Clear();
            Level2Asks.Clear();            
        }
        
        public void Close() {
        	Disconnect();
        }
        
        public void Disconnect()
        {
			cancelThread = true;
			if( quoteThread!=null) {
				if( quoteThread.IsAlive) {
		    		quoteThread.Join();
				}
		    	quoteThread=null;
			}
        }
        
        public void LogLevel2Change(bool recordTrade, TradeSide side, double price, int size) {
        	CreateTick();
			
			if( TryAddLevel2()) {
        		if( recordTrade && Symbol.TimeAndSales == TimeAndSales.Extrapolated) {
        			tick.SetTrade(side, price, size);
        		}
        		SendTick();
			}
		}
        
        private bool TryAddLevel2() {
    		// Update the last Dom total and time
    		// so we can log any changes if ticks don't
    		// come in timely due to a pause in the market.
    		level2Bids.UpdateTotalSize();
    		level2Asks.UpdateTotalSize();
    		
    		if( level2Bids.LastPrice > 0 && level2Asks.LastPrice > 0) {
    			tick.SetQuote(level2Bids.LastPrice, level2Asks.LastPrice);
    			tick.SetDepth(level2Bids.DepthSizes,level2Asks.DepthSizes);
    			if( trace) log.Trace("Level2 SetQuote(), SetDepth()");
    			return true;
    		} else {
    			return false;
    		}
        }
		
        object signalLock = new Object();
    	int orderStart = 0;
    	long followupSignal = 0;
    	bool orderSubmitted = false;
    	int ignoredTickCount = 0;
    	bool isStoredSize = false;
    	long storedSize = 0;
	    public void Signal( double _size) {
    		lock(signalLock) {

    			long size = (long) _size;
				storedSize = size;
    			int elapsed = Environment.TickCount - ignoredTickCount;
    			if( m_account == null ) {
	    			if( ignoredTickCount == 0 || elapsed > 5000) {
		    			orderLog.Notice( "Signal("+size+") storing. Account not loaded.");
						isStoredSize = true;
		    			ignoredTickCount = Environment.TickCount;
    				}
		    		return;
	    		}
    			if( LastAsk == 0D || LastBid == 0D) {
	    			if( ignoredTickCount == 0 || elapsed > 5000) {
		    			orderLog.Notice( "Signal("+size+") storing because LastBid = " + LastBid + " and LastAsk = " + LastAsk);
						isStoredSize = true;
		    			ignoredTickCount = Environment.TickCount;
    				}
		    		return;
	    		}
    			if( orderSubmitted || m_orders.Count > 0 ) {
	    			if( ignoredTickCount == 0 || elapsed > 5000) {
		    			orderLog.Notice( "Signal("+size+") ignored. Orders already pending.");
		    			ignoredTickCount = Environment.TickCount;
	    			} 
	    			return;
	    		}
    			ignoredTickCount = Environment.TickCount;
	    		InternalSignal(size);
    		}
    	}
    	
	    private void InternalSignal( long size) {
			// Check if already same as desired size	    	
    		int currentSize = GetPositionSize();
    		if( currentSize == size) {
    			return;
    		}
   			int beforeSignal = Environment.TickCount;
	    	orderStart = Environment.TickCount;
	    	orderLog.Notice( "Signal( " + size + "), " + 
	    	                      " position size: " + currentSize + ", " + 
	    	                      " position: " + Display(m_position));
	    	if( m_account.PermedForEquities &&
	    	   (size > 0 && currentSize < 0 ||
	    	    size < 0 && currentSize > 0)) {
	    		TrySubmitOrder(-currentSize);
	    		followupSignal = size;
	    	} else {
    			TrySubmitOrder(size - currentSize);
	    	}
    		int elapsed = Environment.TickCount - beforeSignal;
    		orderLog.Notice("InternalSignal() " + elapsed + "ms");
	    }

	    public int GetPositionSize() {
	    	int size = 0;
	    	try {
	    		MbtPosition position = m_position;
		    	if( position != null) {
			    	size += position.PendingBuyShares - position.PendingSellShares +
			    			position.IntradayPosition + position.OvernightPosition;
		    	}
	    	} catch(Exception ex) {
	    		orderLog.Notice( "Exception: " + ex);
	    	}
	    	return size;
	    }

		public void TrySubmitOrder(long size)
		{
			if( size == 0) return;
            orderLog.Notice("SubmitOrder("+size+")");
            long lVol = Math.Abs(size);
            int lBuySell = size > 0 ? MBConst.VALUE_BUY : MBConst.VALUE_SELL;
			string sSym = symbol.Symbol;
			double dPrice = size > 0 ? lastAsk : lastBid;
			int	lTIF = MBConst.VALUE_DAY;
            int lOrdType = MBConst.VALUE_MARKET;
			int lVolType = MBConst.VALUE_NORMAL;
			string sRoute = "MBTX";
			string bstrRetMsg = null;
			System.DateTime dDte = new System.DateTime(0);
			string sOrder = String.Format("{0} {1} {2} at {3:c} on {4}",
                  lBuySell == MBConst.VALUE_BUY ? "Buy" : "Sell",
                  lVol, sSym, dPrice, sRoute);
            orderLog.Notice(sOrder);
            bool bSubmit = m_OrderClient.Submit(lBuySell,(int)lVol,sSym,dPrice,0.0,lTIF,0,lOrdType,lVolType,0,m_account,sRoute,"", 0, 0, dDte, dDte, 0, 0, 0, 0, 0, ref bstrRetMsg);
			if( bSubmit )
			{
                orderLog.Notice(
					String.Format("Order submission successful! bstrRetMsg = [{0}]", bstrRetMsg)); // wide string!
				orderSubmitted = true;
			}
			else
			{
                orderLog.Notice(
					String.Format("Order submission failed! bstrRetMsg = [{0}]", bstrRetMsg)); // wide string!
			}
		}

        public void OnCancelPlaced(MbtOpenOrder order) {
	    	if( debug) orderLog.Debug( "OnCancelPlaced( " + order.OrderNumber + " )");
	    }
		
        public void OnReplacePlaced(MbtOpenOrder order) {
	    	if( debug) orderLog.Debug( "OnReplacePlaced( " + order.OrderNumber + " )");
	    }
		
        public void OnReplaceRejected(MbtOpenOrder order) {
	    	if( debug) orderLog.Debug( "OnReplaceRejected( " + order.OrderNumber + " )");
	    }
		
        public void OnCancelRejected(MbtOpenOrder order) {
	    	if( debug) orderLog.Debug( "OnCancelRejected( " + order.OrderNumber + " )");
	    }
	    
        public void OnClose(int x) {
			m_account = null;
	    	if( debug) orderLog.Debug( "Account: OnClose( " + x + " )");
	    }

		public void OnSubmit(MbtOpenOrder order) {
			m_orders[order.OrderNumber] = order;
    		int elapsed = Environment.TickCount - orderStart;
	    	orderLog.Notice( "OnSubmit( " + order.OrderNumber + " ) " + elapsed + "ms" );
			orderSubmitted = false;
	    }
        
        public void OnAcknowledge(MbtOpenOrder order) {
			m_orders[order.OrderNumber] = order;
    		int elapsed = Environment.TickCount - orderStart;
	    	orderLog.Notice( "OnAcknowledge( " + order.OrderNumber + " ) " + elapsed + "ms" );
	    }
        public void OnExecute(MbtOpenOrder order) { 
			m_orders[order.OrderNumber] = order;
    		int elapsed = Environment.TickCount - orderStart;
    		orderLog.Notice( "OnExecute( " + order.OrderNumber + " ), " + elapsed + "ms");
	    }
		
        public void OnRemove(MbtOpenOrder order) { 
    		int elapsed = Environment.TickCount - orderStart;
    		orderLog.Notice( "OnRemove( " + order.OrderNumber + " ), " + elapsed + "ms, position: " + Display(m_position));
    		if( followupSignal != 0) {
    			orderLog.Notice("Followup Signal = " + followupSignal );
    			InternalSignal( followupSignal);
    			followupSignal = 0;
    		} else {
	    		orderLog.Notice( "Account Status: " + Display(m_account));
    		}
			m_orders.Remove(order.OrderNumber);
		}
		
        public void OnHistoryAdded(MbtOrderHistory orderhistory) {
	    	SaveHistory(orderhistory);
	    }
		
        public void OnPositionAdded(MbtPosition position) {
	    	m_position = position;
	    	if( debug) orderLog.Debug( "OnPositionAdded( )");
	    }
        public void OnPositionUpdated(MbtPosition position) {
	    	m_position = position;
	    	if( debug) orderLog.Debug( "Account: OnPositionUpdated(  )");
	    }
		
        public void OnBalanceUpdate(MbtAccount account) {
	    	this.m_account = account;
	    	if( debug) orderLog.Debug( "OnBalanceUpdate( " + account.Account + "," + account.Customer + " )");
	    }
		
        public void OnDefaultAccountChanged(MbtAccount account) {
	    	this.m_account = account;
	    	if( debug) orderLog.Debug( "OnDefaultAccountChanged( " + account.Account + "," + account.Customer + " )");
	    }
		
        public void OnAccountUnavailable(MbtAccount account) {
	    	this.m_account = null;
	    	if( debug) orderLog.Debug( "OnAccountUnavailable( " + account.Account + "," + account.Customer + " )");
	    }
		
	    bool firstTime = true;
        public void OnAccountLoaded(MbtAccount account) {
	    	this.m_account = account;
	    	if( firstTime) {
	    		orderLog.Notice( "Starting account = " + Display(m_account));
	    		m_position = m_OrderClient.Positions.Find(m_account, symbol.Symbol);
	    		if( m_position != null) {
		    		orderLog.Notice( "Starting position = " + Display(m_position));
	    		}
	    		MbtOpenOrders orders = m_OrderClient.OpenOrders;
		    	orders.LockItems();
		    	for( int i=0; i< orders.Count; i++) {
		    		orderLog.Notice("Order " + i + ": " + Display(orders[i]));
		    		m_orders[orders[i].OrderNumber] = orders[i];
		    	}
		    	orders.UnlockItems();
		    	foreach( string orderNum in m_orders.Keys) {
		    		string bstrRetMsg = null;
		    		if( m_OrderClient.Cancel( orderNum, ref bstrRetMsg) == true) {
			    		orderLog.Notice("Order " + orderNum + ": Canceled. " + bstrRetMsg);
		    		} else {
			    		orderLog.Notice("Order " + orderNum + ": Cancel Failed: " + bstrRetMsg);
		    		}
		    	}
		    	CheckSignalSync();
	    	}
	    	firstTime = false;
	    }
	    
	    private void CheckSignalSync() {
	    	if( isStoredSize && LastBid > 0 && LastAsk > 0) {
	    		TrySubmitOrder(storedSize);
	    		isStoredSize = false;
	    	}
	    }

		bool firstHistory = true;
		void SaveHistory( MbtOrderHistory history) {
			if( firstHistory) {
				orderLog.Notice( DisplayHeader(history));
				firstHistory = false;
			}
			orderLog.Notice( DisplayValues(history));
		}
		string DisplayAccount( MbtAccount acct) {
//			TickConsole.Notice("Account: Old Account: " + Display(acct));
			string retVal = "";
			retVal += "Equity="+acct.CurrentEquity;
			retVal += ",Excess="+acct.CurrentExcess;
			retVal += ",MMRUsed="+acct.MMRUsed;
			retVal += ",MMRMultiplier="+acct.MMRMultiplier;
			return retVal;
		}
		
		string DisplayOrder( MbtOpenOrder order) {
//			TickConsole.Notice("Account: Old Order: " + Display(order));
			string retVal = "";
			retVal += "BuySell="+(order.BuySell==MBConst.VALUE_BUY?"Buy":"Sell");
			retVal += ",Symbol="+order.Symbol;
			retVal += ",Quantity="+order.Quantity;
			retVal += ",Price="+order.Price;
			retVal += ",OrderType="+order.OrderType;
			retVal += ",OrderNumber="+order.OrderNumber;
			retVal += ",SharesFilled="+order.SharesFilled;
			return retVal;
		}
		string DisplayHeader( Object obj) {
	    	string retVal = "";
	    	try {
				Type type = obj.GetType();
				PropertyInfo[] pis = type.GetProperties();
				for (int i=0; i<pis.Length; i++) {
			    	try {
						PropertyInfo pi = (PropertyInfo)pis[i];
						string name = pi.Name;
						if( name == "Account") {
							continue;
						}
						if( i!=0) { retVal += ","; }
						retVal += name;
			    	} catch(TargetParameterCountException) {
						// Ignore
			    	}
				}
	    	} catch(Exception ex) {
	    		orderLog.Notice( "Exception: " + ex);
	    	}
			return retVal;
		}	
		string DisplayValues( Object obj) {
	    	string retVal = "";
	    	try {
				Type type = obj.GetType();
				PropertyInfo[] pis = type.GetProperties();
				for (int i=0; i<pis.Length; i++) {
			    	try {
						PropertyInfo pi = (PropertyInfo)pis[i];
						string name = pi.Name;
						if( name == "Account") {
							continue;
						}
						string value = pi.GetValue(obj, new object[] {}).ToString();
						if( i!=0) { retVal += ","; }
						retVal += value;
			    	} catch(TargetParameterCountException) {
						// Ignore
					} catch(NullReferenceException) {
						if( debug) log.Debug("Null reference on " + pis[i].Name);
					}
				}
	    	} catch(Exception ex) {
	    		log.Notice( "Exception: " + ex);
	    	}
			return retVal;
		}	

		string Display( Object obj) {
			if( obj == null) {
				return "null";
			}
	    	string retVal = "";
	    	try {
				Type type = obj.GetType();
				PropertyInfo[] pis = type.GetProperties();
				bool first=true;
				for (int i=0; i<pis.Length; i++) {
			    	try {
						PropertyInfo pi = (PropertyInfo)pis[i];
						string name = pi.Name;
						if( name == "Account") {
							continue;
						}
						string value = pi.GetValue(obj, new object[] {}).ToString();
						if( !first) { retVal += ",";}
						first=false;
						retVal += name + "=";
						retVal += value;
			    	} catch(TargetParameterCountException) {
						// Ignore
			    	}
				}
	    	} catch(Exception ex) {
	    		log.Notice( "Exception: " + ex);
	    	}
			return retVal;
		}

    	#region IMbtQuotesNotify Members
        void MBTQUOTELib.IMbtQuotesNotify.OnOptionsData(ref OPTIONSRECORD pRec)
        {
        }
        void MBTQUOTELib.IMbtQuotesNotify.OnTSData(ref TSRECORD pRec)
        {
        	lock( tsDataLocker) {
        		try { 
	       			ProcessTimeAndSales(ref pRec);
        		} catch( Exception ex) {
        			log.Notice(ex.ToString());
        		}
        	}
        }
        void MBTQUOTELib.IMbtQuotesNotify.OnQuoteData(ref QUOTERECORD pQuote)
        {
        	lock( quotesLocker) {
        		try { 
       				ProcessLevel1(ref pQuote);
        		} catch( Exception ex) {
        			log.Notice(ex.ToString());
        		}
        	}
        }
        void MBTQUOTELib.IMbtQuotesNotify.OnLevel2Data(ref LEVEL2RECORD pRec)
        {
        	lock( level2Locker) {
        		try {
					if( pRec.dPrice < 100000)
		            	ProcessLevel2(pRec);
        		} catch( Exception e) {
        			log.Notice(e.ToString());
        		}
        	}
        }
        #endregion

		public Level2Collection Level2Bids {
			get { return level2Bids; }
		}
		public Level2Collection Level2Asks {
			get { return level2Asks; }
		}

        public string LogMarket() {
        	return level2Bids[0].dPrice + "/" + level2Asks[0].dPrice + "  " + 
        		level2Bids[0].lSize/symbol.Level2LotSize + "/" + level2Asks[0].lSize/symbol.Level2LotSize;
        }
         
		public SymbolInfo Symbol {
			get { return symbol; }
		}
        
		public MbtAccount Account {
			get { return m_account; }
			set { m_account = value; }
		}
        
		public bool Advised {
			get { return advised; }
		}
        
		public int LastChangeTime {
			get { return lastChangeTime; }
		}
		
		public override string ToString()
		{
			return "InstrumentReader for " + symbol.Symbol;
		}
        
		public Receiver Receiver {
			get { return receiver; }
			set { receiver = value; }
		}
    }
}
