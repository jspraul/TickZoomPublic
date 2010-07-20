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
using System.IO;
using System.Threading;
using System.Windows.Forms;
using TickZoom.Api;
using IESignal;
using System.Data.SqlClient;

//using System.Data;
namespace TickZoom.eSignal
{

	public class eSInterface : Provider
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(eSInterface));
		private static readonly bool debug = log.IsDebugEnabled;
        private readonly object readersLock = new object();
	    private readonly static object listLock = new object();

        HooksClass esignal;

        private TickIO tickIO = Factory.TickUtil.TickIO();
        
        private List<SymbolHandler> symbolHandlers = new List<SymbolHandler>();
        private Dictionary<string, int> symHandlookUp = new Dictionary<string, int>();

        public eSInterface()
		{
        }
        
        private void Initialize() {

        	string appDataFolder = Factory.Settings["AppDataFolder"];
			if( appDataFolder == null) {
				throw new ApplicationException("Sorry, AppDataFolder must be set in the app.config file.");
			}
			string configFile = appDataFolder+@"/Providers/eSignalProviderService.config";
			
			LoadProperties(configFile);
			
            esignal = new HooksClass();
            esignal.OnQuoteChanged+=new _IHooksEvents_OnQuoteChangedEventHandler(esignal_OnQuoteChanged);

            //attempt to login to esignal
            try
            {
            	string pw = GetProperty("esignalaccount");
                esignal.SetApplication(pw);
            }
            catch (Exception e)
            {
                //MessageBox.Show("Not Entitled! Check User Name.")
                Console.WriteLine(e.ToString());
            }

            //just to be sure its clean
            esignal.ClearSymbolCache();

            Thread.Sleep(5000);
        }
        
		Receiver receiver;
        public void Start(Receiver receiver)
        {
        	log.Info("eSInterface Startup");
        	this.receiver = (Receiver) receiver;
        	Initialize();
						
        }

        Dictionary<string, string> data;
        string configFile;
        private void LoadProperties(string configFile) {
        	this.configFile = configFile;
			data = new Dictionary<string, string>();
			if( !File.Exists(configFile) ) {
				Directory.CreateDirectory(Path.GetDirectoryName(configFile));
		        using (StreamWriter sw = new StreamWriter(configFile)) 
		        {
		            // Add some text to the file.
		            sw.WriteLine("ClientId=3712");
		            sw.WriteLine("esignalaccount=CHANGEME");
		            // Arbitrary objects can also be written to the file.
		        }
			} 
			
			foreach (var row in File.ReadAllLines(configFile)) {
				string[] nameValue = row.Split('=');
				data.Add(nameValue[0].Trim(),nameValue[1].Trim());
			}
		}
        
        private string GetProperty( string name) {
        	string value;
			if( !data.TryGetValue(name,out value) ||
				value == null || value.Length == 0 || value.Contains("CHANGEME")) {
				throw new ApplicationException(name + " property must be set in " + configFile);
			}
        	return value;
        }
        
        
        public void Stop(Receiver receiver) {
        	esignal.ClearSymbolCache();
        }
        
		private string UpperFirst(string input)
		{
			string temp = input.Substring(0, 1);
			return temp.ToUpper() + input.Remove(0, 1);
		}        
        
		public void StartSymbol(Receiver receiver, SymbolInfo symbol, TimeStamp lastTimeStamp)
		{
			if( debug) log.Debug("StartSymbol " + symbol + ", " + lastTimeStamp);
            //Equity equity = new Equity(symbol.Symbol);
            SymbolHandler handler = GetSymbolHandler(symbol,receiver);

            //esignals needs " #F" for futires symbols
            esignal.RequestSymbol(symbol.Symbol + " #F", 1);
			receiver.OnRealTime(symbol);
		}
		
		public void StopSymbol(Receiver receiver, SymbolInfo symbol)
		{
			if( debug) log.Debug("StopSymbol");
            esignal.ReleaseSymbol(symbol.Symbol + " #F");
			receiver.OnEndRealTime(symbol);
		}	
		
		public void PositionChange(Receiver receiver, SymbolInfo symbol, double signal, IList<LogicalOrder> orders)
		{
//			try {
//				SymbolHandler handler = symbolHandlers[(int)symbol.BinaryIdentifier];
//				int delta = (int)(signal - handler.Position);
//				if( delta != 0) {
//					Contract contract = new Contract(symbol.Symbol,"SMART",SecurityType.Stock,"USD");
//					Order order = new Order();
//					order.OrderType = Krs.Ats.IBNet.OrderType.Market;
//					if( delta > 0) {
//						order.Action = ActionSide.Buy;
//					} else {
//						order.Action = ActionSide.Sell;
//					}
//					order.TotalQuantity = Math.Abs(delta);
//					while(nextValidId==0) {
//						Thread.Sleep(10);
//					}
//					nextValidId++;
//					client.PlaceOrder(nextValidId,contract,order);
//					if(debug) log.Debug("PlaceOrder: " + nextValidId + " for " + contract.Symbol + " = " + order.TotalQuantity);
//				}
//			} catch( Exception ex) {
//				log.Error(ex.Message,ex);
//				throw;
//			}
		}
		
		public void Stop()
		{	
			esignal.ClearSymbolCache();
		}

        private void esignal_OnQuoteChanged(String symbol)
        {
        	
            //holds the data for each tick   
            var quote = esignal.get_GetBasicQuote(symbol);

            //esignal uses " #F" at the end of each of the futures symbols, remove it.
            string sym = symbol.Substring(0, symbol.Length - 3);

            try
            {

                SymbolInfo si = Factory.Symbol.LookupSymbol(sym);
                SymbolHandler buffer = GetSymbolHandler(si, receiver);

                if(quote.dBid > 0)
                {
                	buffer.Bid = quote.dBid;
                	buffer.BidSize = quote.lBidSize;
//                	buffer.SendQuote();
                }
                if(quote.dAsk > 0)
                {
                	buffer.Ask = quote.dAsk;
                	buffer.AskSize = quote.lAskSize;
//                	buffer.SendQuote();                	
                }       
                if(quote.dLast > 0)
                {
                	buffer.Last = quote.dLast;
                	buffer.LastSize = quote.lLastSize;
                	buffer.SendTimeAndSales();
                }
                
//                tickIO.Initialize();
//                tickIO.SetSymbol(si.BinaryIdentifier);
//                if (si.TimeAndSales == TimeAndSales.ActualTrades)
//                {
////                	tickIO.SetTrade(quote.dLast, quote.lLastSize);
//                	tickIO.SetTrade(1, 1);
//                }
//                tickIO.SetTime(TimeStamp.UtcNow);                
//                if (si.QuoteType == QuoteType.Level1)
//                {
//                	tickIO.SetQuote(1, 1, (ushort)1, (ushort)1);
////                    tickIO.SetQuote(quote.dLast, quote.dLast, (ushort)quote.lLastSize, (ushort)quote.lLastSize);
////                    tickIO.SetQuote(quote.dBid, quote.dAsk, (ushort)quote.lBidSize, (ushort)quote.lAskSize);
//                }
//                TickBinary binary = tickIO.Extract();
//                receiver.OnSend(ref binary);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
		
        //private void client_ExecDetails(object sender, ExecDetailsEventArgs e)
        //{
        //    log.InfoFormat("Execution: {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
        //        e.Contract.Symbol, e.Execution.AccountNumber, e.Execution.ClientId, e.Execution.Exchange, e.Execution.ExecutionId,
        //        e.Execution.Liquidation, e.Execution.OrderId, e.Execution.PermId, e.Execution.Price, e.Execution.Shares, e.Execution.Side, e.Execution.Time);
        //}

        //private void client_RealTimeBar(object sender, RealTimeBarEventArgs e)
        //{
        //    if( debug) log.Debug("Received Real Time Bar: " + e.Close);
        //}

        //private void client_OrderStatus(object sender, OrderStatusEventArgs e)
        //{
        //    if(debug) log.Debug("Order Status " + e.Status);
        //}

        //private void client_UpdateMktDepth(object sender, UpdateMarketDepthEventArgs e)
        //{
        //    if(debug) log.Debug("Tick ID: " + e.TickerId + " Tick Side: " + EnumDescConverter.GetEnumDescription(e.Side) +
        //                      " Tick Size: " + e.Size + " Tick Price: " + e.Price + " Tick Position: " + e.Position +
        //                      " Operation: " + EnumDescConverter.GetEnumDescription(e.Operation));
        //}

        //private void client_NextValidId(object sender, NextValidIdEventArgs e)
        //{
        //    if( debug) log.Debug("Next Valid ID: " + e.OrderId);
        //    nextValidId = e.OrderId;
        //}

        //private void client_TickSize(object sender, TickSizeEventArgs e)
        //{
        //    SymbolHandler buffer = symbolHandlers[e.TickerId];
        //    if( e.TickType == TickType.AskSize) {
        //        buffer.AskSize = e.Size;
        //    } else if( e.TickType == TickType.BidSize) {
        //        buffer.BidSize = e.Size;
        //    } else if( e.TickType == TickType.LastSize) {
        //        buffer.LastSize = e.Size;
        //    }
        //}

        private SymbolHandler GetSymbolHandler(SymbolInfo symbol, Receiver receiver)
        {
            if (symbolHandlers.Count <= (int)symbol.BinaryIdentifier)
            {
                while (symbolHandlers.Count <= (int)symbol.BinaryIdentifier)
                {
                    if (symbolHandlers.Count == 0)
                    {
                        symbolHandlers.Add(null);
                    }
                    else
                    {
                        SymbolInfo tempSymbol = Factory.Symbol.LookupSymbol((ulong)symbolHandlers.Count);
                        symbolHandlers.Add(new SymbolHandler(tempSymbol, receiver));
                    }
                }
            }
            return symbolHandlers[(int)symbol.BinaryIdentifier];
        }
        
        private void RemoveSymbolHandler(SymbolInfo symbol) {
        	symbolHandlers[(int)symbol.BinaryIdentifier] = null;
        }

        //private void client_Error(object sender, Krs.Ats.IBNet.ErrorEventArgs e)
        //{
        //    log.Error("Error: "+ e.ErrorMsg);
        //}

        //private void client_TickPrice(object sender, TickPriceEventArgs e)
        //{
        //    SymbolHandler buffer = symbolHandlers[e.TickerId];
        //    if( e.TickType == TickType.AskPrice) {
        //        buffer.Ask = (double) e.Price;
        //        buffer.SendQuote();
        //    } else if( e.TickType == TickType.BidPrice) {
        //        buffer.Bid = (double) e.Price;
        //        buffer.SendQuote();
        //    } else if( e.TickType == TickType.LastPrice) {
        //        buffer.Last = (double) e.Price;
        //        buffer.SendTimeAndSales();
        //    }
        //}
        
        //private void cilent_UpdateAccountSize(object sender, UpdateAccountValueEventArgs e) {
        //}
        
        //private void client_UpdatePortfolio(object sender, UpdatePortfolioEventArgs e) {
        //    try {
        //        SymbolInfo symbol = Factory.Symbol.LookupSymbol(e.Contract.Symbol);
        //        SymbolHandler handler = GetSymbolHandler(symbol,receiver);
        //        handler.Position = e.Position;
        //        if(debug) log.Debug( "UpdatePortfolio: " + e.Contract.Symbol + " is " + e.Position);
        //    } catch( ApplicationException ex) {
        //        log.Warn("UpdatePortfolio: " + ex.Message);
        //    }
        //}
	}
}
