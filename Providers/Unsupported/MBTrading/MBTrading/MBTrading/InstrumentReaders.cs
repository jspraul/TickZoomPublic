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

using MBTORDERSLib;
using MBTQUOTELib;
using TickZoom.Api;

namespace TickZoom.MBTrading
{
	/// <summary>
	/// Description of Instruments.
	/// </summary>
	public class InstrumentReaders
    {
		private static readonly Log log = Factory.Log.GetLogger(typeof(InstrumentReaders));
		private static readonly bool debug = log.IsDebugEnabled;
        private MbtQuotes m_Quotes;
        private MbtOrderClient m_OrderClient;
        private Dictionary<string, MbtPosition> positions = new Dictionary<string,MbtPosition>();
        private Collection<InstrumentReader> readers = new Collection<InstrumentReader>();
        private Receiver receiver;

        public InstrumentReaders(Receiver providerClient, MbtOrderClient orderClient, MbtQuotes quotes)
		{
        	this.receiver = providerClient;
        	m_OrderClient = orderClient;
            m_Quotes = quotes;
		}
        
        public int GetIndex(string Symbol)
        {
            for(int i=0;i<readers.Count;i++)
            {
                if(readers[i].Symbol.Symbol==Symbol) return i;
            }
            return -1;
        }
        
        public void Signal(string symbol, double signal) {
            int instrumentIndex = GetIndex(symbol);
            if (instrumentIndex != -1)
            {
            	readers[instrumentIndex].Signal(signal);
            }
        }
        
        public void AddDepth(SymbolInfo symbol)
        {
        	int i = GetIndex(symbol.Symbol);
            if (i == -1)
            {
            	NewReader(symbol);
            }
        }
        
        private void NewReader(SymbolInfo symbol) {
           	if( debug) log.Debug("NewReader for " + symbol + " with quote type " + symbol.QuoteType);
        	InstrumentReader reader = new InstrumentReader(m_OrderClient, m_Quotes, symbol);
        	reader.Receiver = receiver;
        	reader.Initialize();
            readers.Add(reader);
            if (m_Quotes.ConnectionState == enumConnectionState.csLoggedIn) {
                reader.Connect();
            }
        }
        
        public void Remove(string Symbol)
        {
            int instrumentIndex = GetIndex(Symbol);
            if (instrumentIndex != -1)
            {
            	readers[instrumentIndex].Close();
                readers.RemoveAt(instrumentIndex);
            }
        }
        
        public void Clear()
        {
            for (int i = readers.Count-1; i >= 0; i--)
            {
            	readers[i].Close();
                readers.RemoveAt(i);
            }
        }
        public void Close()
        { 
            for (int i = readers.Count-1; i >= 0; i--)
            {
            	readers[i].Close();
                readers.RemoveAt(i);
            }
        }
        
        public void AdviseAll()
        {
            for (int i = 0; i < readers.Count; i++)
            {
            	readers[i].Connect();
            	if( debug) log.Debug("AdviseAll() advised symbol :" + readers[i].Symbol.Symbol);
            }
        }
        
        private int lastConnectTime = 0;
        
        public void UpdateLastConnectTime() {
        	lastConnectTime = Environment.TickCount;
        }
        
        public int GetLastChangeTime()
        {
        	int lastChangeTime = 0;
            for (int i = 0; i < readers.Count; i++)
            {
            	if( readers[i].LastChangeTime > lastChangeTime) {
            		lastChangeTime = readers[i].LastChangeTime;
            	}
            }
            if( lastConnectTime > lastChangeTime) {
            	lastChangeTime = lastConnectTime;
            }
            return lastChangeTime;
        }
        
        public void UnadviseAll()
        {
            for (int i = 0; i < readers.Count; i++)
            {
            	readers[i].Disconnect();
            }
        }
        
        void OnCancelPlaced(MbtOpenOrder order) {
        	for(int i=0; i<readers.Count; i++) {
        		if( readers[i].Symbol.Symbol == order.Symbol) {
        			readers[i].OnCancelPlaced(order);
        		}
        	}
	    }
        void OnReplacePlaced(MbtOpenOrder order) {
        	for(int i=0; i<readers.Count; i++) {
        		if( readers[i].Symbol.Symbol == order.Symbol) {
        			readers[i].OnReplacePlaced(order);
        		}
        	}
	    }
        void OnReplaceRejected(MbtOpenOrder order) {
        	for(int i=0; i<readers.Count; i++) {
        		if( readers[i].Symbol.Symbol == order.Symbol) {
        			readers[i].OnReplaceRejected(order);
        		}
        	}
	    }
        void OnCancelRejected(MbtOpenOrder order) {
        	for(int i=0; i<readers.Count; i++) {
        		if( readers[i].Symbol.Symbol == order.Symbol) {
        			readers[i].OnCancelRejected(order);
        		}
        	}
	    }
	    
        void OnClose(int x) {
        	for(int i=0; i<readers.Count; i++) {
       			readers[i].OnClose(x);
        	}
	    }

		void OnSubmit(MbtOpenOrder order) {
        	for(int i=0; i<readers.Count; i++) {
        		if( readers[i].Symbol.Symbol == order.Symbol) {
        			readers[i].OnSubmit(order);
        		}
        	}
	    }
        
        void OnAcknowledge(MbtOpenOrder order) {
        	for(int i=0; i<readers.Count; i++) {
        		if( readers[i].Symbol.Symbol == order.Symbol) {
        			readers[i].OnAcknowledge(order);
        		}
        	}
	    }
        void OnExecute(MbtOpenOrder order) { 
        	for(int i=0; i<readers.Count; i++) {
        		if( readers[i].Symbol.Symbol == order.Symbol) {
        			readers[i].OnExecute(order);
        		}
        	}
	    }
        void OnRemove(MbtOpenOrder order) { 
        	for(int i=0; i<readers.Count; i++) {
        		if( readers[i].Symbol.Symbol == order.Symbol) {
        			readers[i].OnRemove(order);
        		}
        	}
	    }
        void OnHistoryAdded(MBTORDERSLib.MbtOrderHistory orderhistory) {
        	for(int i=0; i<readers.Count; i++) {
        		if( readers[i].Symbol.Symbol == orderhistory.Symbol) {
        			readers[i].OnHistoryAdded(orderhistory);
        		}
        	}
	    }
        void OnPositionAdded(MBTORDERSLib.MbtPosition position) {
        	for(int i=0; i<readers.Count; i++) {
        		if( readers[i].Symbol.Symbol == position.Symbol) {
        			readers[i].OnPositionAdded(position);
        		}
        	}
	    }
        void OnPositionUpdated(MBTORDERSLib.MbtPosition position) {
        	for(int i=0; i<readers.Count; i++) {
        		if( readers[i].Symbol.Symbol == position.Symbol) {
        			readers[i].OnPositionUpdated(position);
        		}
        	}
	    }
        void OnBalanceUpdate(MBTORDERSLib.MbtAccount account) {
        	for(int i=0; i<readers.Count; i++) {
        		readers[i].OnBalanceUpdate(account);
        	}
	    }
        void OnDefaultAccountChanged(MBTORDERSLib.MbtAccount account) {
        	for(int i=0; i<readers.Count; i++) {
        		readers[i].OnDefaultAccountChanged(account);
        	}
	    }
        
        void OnAccountLoaded(MBTORDERSLib.MbtAccount account) {
        	for(int i=0; i<readers.Count; i++) {
        		readers[i].OnAccountLoaded(account);
        	}
	    }

		
		#region EventHandlers
	    public void AssignEventHandlers()
	    {
	        m_OrderClient.OnAccountLoaded += OnAccountLoaded;
	        m_OrderClient.OnAcknowledge += OnAcknowledge;
	        m_OrderClient.OnBalanceUpdate += OnAccountLoaded;
	        m_OrderClient.OnCancelPlaced += OnCancelPlaced;
	        m_OrderClient.OnCancelRejected += OnCancelRejected;
	        m_OrderClient.OnClose += OnClose;
	        m_OrderClient.OnSubmit += OnSubmit;
	        m_OrderClient.OnDefaultAccountChanged += OnDefaultAccountChanged;
	        m_OrderClient.OnExecute += OnExecute;
	        m_OrderClient.OnRemove += OnRemove;
	        m_OrderClient.OnHistoryAdded += OnHistoryAdded;
	        m_OrderClient.OnPositionAdded += OnPositionAdded;
	        m_OrderClient.OnPositionUpdated += OnPositionUpdated;
	        m_OrderClient.OnReplacePlaced += OnReplacePlaced;
	        m_OrderClient.OnReplaceRejected += OnReplaceRejected;
	    }
	    
	    public void RemoveEventHandlers()
	    {
	        m_OrderClient.OnAccountLoaded -= OnAccountLoaded;
	        m_OrderClient.OnAcknowledge -= OnAcknowledge;
	        m_OrderClient.OnBalanceUpdate -= OnAccountLoaded;
	        m_OrderClient.OnCancelPlaced -= OnCancelPlaced;
	        m_OrderClient.OnCancelRejected -= OnCancelRejected;
	        m_OrderClient.OnClose -= OnClose;
	        m_OrderClient.OnSubmit -= OnSubmit;
	        m_OrderClient.OnDefaultAccountChanged -= OnDefaultAccountChanged;
	        m_OrderClient.OnExecute -= OnExecute;
	        m_OrderClient.OnRemove -= OnRemove;
	        m_OrderClient.OnHistoryAdded -= OnHistoryAdded;
	        m_OrderClient.OnPositionAdded -= OnPositionAdded;
	        m_OrderClient.OnPositionUpdated -= OnPositionUpdated;
	        m_OrderClient.OnReplacePlaced -= OnReplacePlaced;
	        m_OrderClient.OnReplaceRejected -= OnReplaceRejected;
	    }
	    #endregion
	    
	    public void Add(string Symbol)
        {
            if (GetIndex(Symbol) == -1)
            {
	        	SymbolInfo instrument = Factory.Symbol.LookupSymbol(Symbol);
	        	InstrumentReader reader = new InstrumentReader(m_OrderClient, m_Quotes, instrument);
                readers.Add(reader);
                if (m_Quotes.ConnectionState == enumConnectionState.csLoggedIn) {
                	reader.Connect();
                }
            }
        }
        
		public Collection<InstrumentReader> Readers {
			get { return readers; }
		}
        
    }
}
