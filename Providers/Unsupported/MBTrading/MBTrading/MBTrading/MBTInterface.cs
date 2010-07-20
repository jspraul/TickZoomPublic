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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;

using MBTCOMLib;
using MBTORDERSLib;
using MBTQUOTELib;
using TickZoom.Api;

//using System.Data;
namespace TickZoom.MBTrading
{
	public class MBConst
	{
		public const int tickEvenUp		= 0;
		public const int tickDown		= 1;
		public const int VALUE_BUY		= 10000;
		public const int VALUE_SELL		= 10001;
		public const int VALUE_LIMIT	= 10030;
		public const int VALUE_MARKET	= 10031;
		public const int VALUE_DAY		= 10011;
        public const int VALUE_GTC      = 10008;
		public const int VALUE_NORMAL	= 10042;
	}
	
	public class MbtInterface : Provider
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(MbtInterface));
		private static readonly bool debug = log.IsDebugEnabled;
		private MBTCOMLib.MbtComMgr m_ComMgr;
        private MBTORDERSLib.MbtOrderClient m_OrderClient;
        private MBTORDERSLib.MbtAccount m_Account;
        public MbtQuotes m_Quotes;
        private readonly object readersLock = new object();
        private InstrumentReaders instrumentReaders;
        private bool closePending = false;
        private int lastChangeQuotes, lastChangeOrders, lastChangePerms, lastChangeReaders;
        private int memberId;
        private string username;
        private string password;
	    private readonly static object listLock = new object();
        private enumConnectionState permsHealth = enumConnectionState.csDisconnected;
        private enumConnectionState ordersHealth = enumConnectionState.csDisconnected;
        private enumConnectionState quotesHealth = enumConnectionState.csDisconnected;
		private Thread monitorThread = null;
        private int retryTime = 5000; // Milliseconds
        private int timeoutTime = 60000; // Milliseconds
        public void m_ComMgr_OnCriticalShutdown()
        {
            log.Fatal("Critical Shutdown Event Received");
            Application.Restart();
        }

        private void NameThread(string name) {
        	string threadName = Thread.CurrentThread.Name;
        	if( threadName == null) {
        		Thread.CurrentThread.Name = name; 
        	}
        }
       
        public void m_ComMgr_OnHealthUpdate(enumServerIndex index, enumConnectionState state)
        {
        	NameThread("MBTradingSDK");
        	switch( index) {
        		case enumServerIndex.siPerms:
        			permsHealth = state;
	            	lastChangePerms = Environment.TickCount;
        			break;
        		case enumServerIndex.siOrders:
        			ordersHealth = state;
	            	lastChangeOrders = Environment.TickCount;
        			break;
        		case enumServerIndex.siQuotes:
        			quotesHealth = state;
	            	lastChangeQuotes = Environment.TickCount;
        			break;
        		default:
		            log.Error("Health Update: ERROR: Unknown: "+index.ToString() + ":" + state.ToString());
		            break;
        	}
        	log.Debug("Health Update: "+index.ToString() + ":" + state.ToString());
        }
        
        public void PermsHealthUpdate() {
        	switch( permsHealth) {
        		case enumConnectionState.csDisconnected:
        			m_Account = null;
        			break;
        		default:
        			break;
        	}
        }
        
        public void m_OrderClient_OnAccountLoaded(MBTORDERSLib.MbtAccount account)
        {
        	try {
	            m_Account = m_OrderClient.Accounts.DefaultAccount;
	            log.Info("Account loaded");
        	} catch( Exception e) {
        		log.Error( "OnAccountLoaded Exception", e);
        	}
        }
        
        public void m_OrderClient_OnAccountUnavailable(MBTORDERSLib.MbtAccount account) {
        	try {
	            log.Info("Account temporarily unavailable.");
	            m_Account = m_OrderClient.Accounts.DefaultAccount;
        	} catch( Exception e) {
        		log.Error( "OnAccountUnavailable Exception", e);
        	}
	    }
        
        public void m_OrderClient_OnSubmit(MBTORDERSLib.MbtOpenOrder order) 
        {
        	log.Info("Order submitted " + order);
        }
        
        public void m_ComMgr_OnLogonSucceed()
        {
        	try {
	            log.Info("Com Manager Connected");
//            	AttemptConnectReaders();
            	lastChangePerms = 0;
        	} catch( Exception e) {
        		log.Error( "OnLogonSucceed exception", e);
        	}
        }
        
        public void m_OrderClient_OnLogonSucceed()
        {
        	try {
        		if( debug) log.Debug("Order Client Connected");
//            	AttemptConnectReaders();
            	lastChangeOrders = 0;
        	} catch( Exception e) {
        		log.Error( "Order Client Connect exception", e);
        	}
        }
        
        public void AttemptConnectReaders() {
        	try {
	            InstrumentReaders.AdviseAll();
	            InstrumentReaders.AssignEventHandlers();
	            lastChangeReaders = Environment.TickCount;
        	} catch( Exception e) {
        		log.Error( e);
        	}
        }
        
        public void m_OrderClient_OnConnect(int ErrorCode)
        {
        	if( debug) log.Debug( "Order Client OnConnect err="+ErrorCode);
        }
        
        public void m_OrderClient_OnClose(int ErrorCode)
        {
        	try {
	        	if( !closePending) {
		            log.Warn("Connection to OrderClient losterr="+ErrorCode+"!!!");
	        	}
        	} catch( Exception e) {
        		log.Error( e);
        	}
        }
        
 		bool retrying = false;
		private void TryRetryStart(int millis) {
  			if( debug) log.Debug("Retrying quotes with ...");
			if( !retrying) {
 				double seconds = millis/1000;
				log.Info( "QuoteServer connection lost. Retrying every " + seconds + " seconds ...");
				retrying = true;
			}
		}
		
		private void TryRetryEnd() {
			if( retrying) {
				log.Info("QuoteServer reconnecting.");
			}
			retrying = false;
		}

        public void m_Quotes_OnClose(int ErrorCode)
        {
        	NameThread("MBTradingQuotesSDK");
        	try {
	        	if( !closePending) {
        			TryRetryStart(retryTime);
        			if( debug) log.Debug("Connection to Quote server lost err="+ErrorCode+"!!!");
	        	}
        	} catch( Exception e) {
        		log.Error( e);
        	}
        }
        
        public void m_Quotes_OnConnect(int ErrorCode)
        {
        	
        	if( debug) log.Info( "Quotes OnConnect err="+ErrorCode);
        }
        
        public void m_Quotes_OnLogonSucceed()
        {
        	try {
        		TryRetryEnd();
        		if( debug) log.Debug("Quote Server Connected");
            	AttemptConnectReaders();
            	InstrumentReaders.UpdateLastConnectTime();
            	lastChangeQuotes = 0;
        	} catch( Exception e) {
        		log.Error( e);
        	}
        }
        
        public MbtInterface()
		{
        }
        
        public void AssignEventHandlers()
        {
            m_OrderClient.OnAccountLoaded += m_OrderClient_OnAccountLoaded;
            m_OrderClient.OnAccountUnavailable += m_OrderClient_OnAccountUnavailable;
            m_OrderClient.OnSubmit += m_OrderClient_OnSubmit;
            m_OrderClient.OnClose += m_OrderClient_OnClose;
            m_OrderClient.OnConnect += m_OrderClient_OnConnect;
            m_OrderClient.OnLogonSucceed += m_OrderClient_OnLogonSucceed;
            m_ComMgr.OnCriticalShutdown += m_ComMgr_OnCriticalShutdown;
            m_ComMgr.OnHealthUpdate += m_ComMgr_OnHealthUpdate;
            m_ComMgr.OnLogonSucceed += m_ComMgr_OnLogonSucceed;
            m_Quotes.OnClose += m_Quotes_OnClose;
            m_Quotes.OnConnect += m_Quotes_OnConnect;
            m_Quotes.OnLogonSucceed += m_Quotes_OnLogonSucceed;
        }
        
        public void RemoveEventHandlers()
        {
            m_OrderClient.OnAccountLoaded -= m_OrderClient_OnAccountLoaded;
            m_OrderClient.OnAccountUnavailable -= m_OrderClient_OnAccountUnavailable;
            m_OrderClient.OnSubmit -= m_OrderClient_OnSubmit;
            m_OrderClient.OnClose -= m_OrderClient_OnClose;
            m_OrderClient.OnConnect -= m_OrderClient_OnConnect;
            m_OrderClient.OnLogonSucceed -= m_OrderClient_OnLogonSucceed;
            m_ComMgr.OnCriticalShutdown -= m_ComMgr_OnCriticalShutdown;
            m_ComMgr.OnHealthUpdate -= m_ComMgr_OnHealthUpdate;
            m_ComMgr.OnLogonSucceed -= m_ComMgr_OnLogonSucceed;
            m_Quotes.OnClose -= m_Quotes_OnClose;
            m_Quotes.OnConnect -= m_Quotes_OnConnect;
            m_Quotes.OnLogonSucceed -= m_Quotes_OnLogonSucceed;
        }
        
		public void MonitorThread()
        {
            log.Info("Real Time Connection Monitor Started");
            DoLogin();
            int start = Environment.TickCount;
        	while( !closePending) {
            	CheckHealth();
            	CheckAccount();
            	DoEvents();
            	Thread.Sleep(1);
// Simulate unhandled exceptions.
//            	if( Environment.TickCount - start > 10000) {
//            		throw new NullReferenceException();
//            	}
        	}
            log.Info("Real Time Connection Monitor Closing");
        }
        
        public void DoEvents() {
        	try {
        		Application.DoEvents();
        	} catch( Exception e) {
        		log.Error( e);
        	}
        }

        private void CheckHealth() {
        	try {
                CheckPermsHealth();
                CheckOrdersHealth();
                CheckQuotesHealth();
        	} catch( Exception e) {
        		log.Error( e);
        	}
    	}

        private void CheckPermsHealth() {
            bool retryPerms = Environment.TickCount - lastChangePerms > retryTime;
            if (retryPerms && permsHealth != enumConnectionState.csLoggedIn) {
	  			if( debug) log.Debug("Retrying perms connection with ...");
            	m_ComMgr.ReconnectPerms();
            	lastChangePerms = Environment.TickCount;
            }
        }
        
        private void CheckOrdersHealth() {
            bool retryOrders = Environment.TickCount - lastChangeOrders > retryTime;
	        if (retryOrders && ordersHealth != enumConnectionState.csLoggedIn) {
	  			if( debug) log.Debug("Retrying order connection with ...");
	        	m_OrderClient.Connect();
	        	lastChangeOrders = Environment.TickCount;
	        }
        }
        
        private void CheckQuotesHealth() {
            bool retryQuotes = Environment.TickCount - lastChangeQuotes > retryTime;
            if (retryQuotes && quotesHealth != enumConnectionState.csLoggedIn) {
            	if( debug) log.Debug("Reconnect Quotes");
            	TryRetryStart(retryTime);
            	m_Quotes.Connect();
            	if( debug) log.Debug("Returned from Quotes.Connect");
            	lastChangeQuotes = Environment.TickCount;
            	return;
            }
            
            // If Quotes logged in check the readers to make sure they're staying current.
            if (retryQuotes && quotesHealth == enumConnectionState.csLoggedIn) {
                bool retryReaders = Environment.TickCount - lastChangeReaders > retryTime;
            	// Update the last readers time.
            	lastChangeReaders = InstrumentReaders.GetLastChangeTime();
            	// Is it still longer than the retry time?
            	int currentTime = Environment.TickCount;
            	retryReaders = currentTime - lastChangeReaders > timeoutTime;
            	if( retryReaders) {
		            log.Info("Reconnect Connect {Readers timed out}");
	            	m_Quotes.Disconnect();
		            if( debug) log.Debug("Returned from Quotes.Connect {Readers Timeout}");
            	}
            }
        }

  		bool reloading = false;
		private void TryAcctReloadStart(string message, int millis) {
  			if( debug) log.Debug("Retrying account with " + message + "...");
			if( !reloading ) {
 				double seconds = millis/1000;
				log.Info( "Acccount: " + message + ". Retrying every " + seconds + " seconds." );
				reloading = true;
			}
		}
		
		private void TryAcctReloadEnd() {
			if( reloading ) {
				log.Info("Account loaded.");
			}
			reloading = false;
		}
  		
  		int lastAccountTry = 0;
		private void CheckAccount()
		{
			if( m_Account == null ) {
				return;
			}
			enumAcctState acctState = m_Account.State;
			int reloadMillis = 5000;
			if( Environment.TickCount - lastAccountTry > reloadMillis) {
				lastAccountTry = Environment.TickCount;
				switch( acctState) {
					case enumAcctState.asUnavailable:
					case enumAcctState.asUnloaded:
					case enumAcctState.asLoading:
					case enumAcctState.asReloading:
						TryAcctReloadStart( acctState.ToString(), reloadMillis);
						m_Account.Load();
						break;
					case enumAcctState.asLoaded:
						TryAcctReloadEnd();
						break;
					default:
						log.Error( "Unknown AccountStatus: " + m_Account.State);
						break;
				}
			}
		}

        
        public void DoLogin() {
            log.Info("Login: Checking if previous instance detected");
            if( m_ComMgr.IsPreviousInstanceDetected( username)) {
	            log.Info("Login: Checking current health.");
	            CheckPermsHealth();
            } else {
	           	m_OrderClient.OnDemandMode = false;
	            log.Info("Executing DoLogin with " + memberId + ", " + username);
	            while (!m_ComMgr.DoLogin(memberId, username, password, ""))
	            {
	                log.Error("Login Failed!!! Retrying in 5 seconds.");
	                int iMaxWait = 5000;
	                int dwStart = Environment.TickCount;
	                while(!closePending && Environment.TickCount < dwStart + iMaxWait) {
	                	Application.DoEvents();
	                	Thread.Sleep(5);
	                }
	            }
	        	lastChangeQuotes = lastChangeOrders = lastChangePerms = lastChangeReaders = Environment.TickCount;
	            log.Info("Login Succeeded - Please wait for all accounts to load.");
            }
        }
        
		Receiver receiver;
        public void Start(Receiver receiver)
        {
        	log.Info("MBTInterface Startup");
        	this.receiver = (Receiver) receiver;
        	Initialize();
			string appDataFolder = Factory.Settings["AppDataFolder"];
			if( appDataFolder == null) {
				throw new ApplicationException("Sorry, AppDataFolder must be set in the app.config file.");
			}
			string configFile = appDataFolder+@"/Providers/MBTradingService.config";
			
			LoadProperties(configFile);
			
			string liveOrDemo = GetProperty("LiveOrDemo");
			liveOrDemo = UpperFirst(liveOrDemo.ToLower());
			log.Info("Found live or demo selection = " + liveOrDemo);
			
			string equityOrForex = GetProperty("EquityOrForex");
			equityOrForex = UpperFirst(equityOrForex.ToLower());
			log.Info("Found equity or forex selection = " + equityOrForex);
			
			string prefix = equityOrForex + liveOrDemo;

			string memberIdStr = GetProperty("ClientId");
			int _memberId = Convert.ToInt32(memberIdStr);
			
			string _username = GetProperty(prefix + "UserName");
			string _password = GetProperty(prefix + "Password");
			
        	memberId = _memberId;
        	username = _username;
        	password = _password;
         	closePending=false;
			
            monitorThread = new Thread(new ThreadStart(MonitorThread));
            monitorThread.IsBackground = true;
            monitorThread.Priority = ThreadPriority.AboveNormal;
            monitorThread.Name = "MBTMonitor";
            monitorThread.Start();
            // Wait for connection.
            while(permsHealth != enumConnectionState.csLoggedIn) {
            	Application.DoEvents();
            	Thread.Sleep(5);
            	permsHealth = m_ComMgr.get_CurrentHealth(enumServerIndex.siPerms);
            }
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
		            sw.WriteLine("EquityOrForex=equity");
		            sw.WriteLine("LiveOrDemo=demo");
		            sw.WriteLine("ForexDemoUserName=CHANGEME");
		            sw.WriteLine("ForexDemoPassword=CHANGEME");
		            sw.WriteLine("EquityDemoUserName=CHANGEME");
		            sw.WriteLine("EquityDemoPassword=CHANGEME");
		            sw.WriteLine("ForexLiveUserName=CHANGEME");
		            sw.WriteLine("ForexLivePassword=CHANGEME");
		            sw.WriteLine("EquityLiveUserName=CHANGEME");
		            sw.WriteLine("EquityLivePassword=CHANGEME");
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
        
        private void Initialize() {
            m_ComMgr = null; 
            m_ComMgr = new MBTCOMLib.MbtComMgrClass();
            m_ComMgr.SilentMode = true;
            m_ComMgr.EnableSplash(false); 
            
            m_OrderClient = m_ComMgr.OrderClient;
            m_OrderClient.SilentMode = true;
            
            m_Quotes=m_ComMgr.Quotes;
            
            AssignEventHandlers();
            InstrumentReaders = new InstrumentReaders(receiver,m_OrderClient,m_Quotes);
        }
        
        public void QuotesDisconnect() {
           	m_Quotes.Disconnect();
        }
        
        public void ReadersDisconnect() {
        	InstrumentReaders.UnadviseAll();
        }
        
        public void OrdersDisconnect() {
           	m_OrderClient.Disconnect();
        }
        
        public void Stop(Receiver receiver) {
        }
        
		private string UpperFirst(string input)
		{
			string temp = input.Substring(0, 1);
			return temp.ToUpper() + input.Remove(0, 1);
		}

        public void LogoutInternal() {
        	log.Info("MBTInterface Logout");
        	closePending=true;
        	if( monitorThread != null) { monitorThread.Join(); }
            if( m_Quotes.ConnectionState != enumConnectionState.csDisconnected) {
            	m_Quotes.Disconnect();
            }
        	if( m_OrderClient.IsConnected() ) {
            	m_OrderClient.Disconnect();
            }
        	InstrumentReaders.Close();
        	RemoveEventHandlers();
        }
        
		public InstrumentReaders InstrumentReaders {
			get { 
        		lock( readersLock) {
        			return instrumentReaders;
        		}
        	}
			set { 
        		lock( readersLock) {
        			instrumentReaders = value;
        		}
        	}
		}
        
		public enumConnectionState PermsHealth {
			get { return permsHealth; }
		}
        
		public enumConnectionState OrdersHealth {
			get { return ordersHealth; }
		}
        
		public enumConnectionState QuotesHealth {
			get { return quotesHealth; }
		}
		
		public void StartSymbol(Receiver receiver, SymbolInfo symbol, StartSymbolDetail detail)
		{
			if( debug) log.Debug("StartSymbol " + symbol + ", " + detail.LastTime);
			receiver.OnEvent(symbol,(int)EventType.StartRealTime,null);
			instrumentReaders.AddDepth(symbol);
		}
		
		public void StopSymbol(Receiver receiver, SymbolInfo symbol)
		{
			if( debug) log.Debug("StartSymbol");
			instrumentReaders.Remove(symbol.Symbol);
			receiver.OnEvent(symbol,(int)EventType.EndRealTime,null);
		}
		
		public void PositionChange(Receiver receiver, SymbolInfo symbol, double signal, IList<LogicalOrder> orders)
		{
			instrumentReaders.Signal(symbol.Symbol,signal);
		}
		
		
 		private volatile bool isDisposed = false;
	    public void Dispose() 
	    {
	        Dispose(true);
	        GC.SuppressFinalize(this);      
	    }
	
	    protected virtual void Dispose(bool disposing)
	    {
       		if( !isDisposed) {
	            isDisposed = true;   
	            if (disposing) {
	            	if( receiver != null) {
						LogoutInternal();
	            	}
	            }
    		}
	    }
		
		public void SendEvent( Receiver receiver, SymbolInfo symbol, int eventType, object eventDetail) {
			switch( (EventType) eventType) {
				case EventType.Connect:
					Start(receiver);
					break;
				case EventType.Disconnect:
					Stop(receiver);
					break;
				case EventType.StartSymbol:
					StartSymbol(receiver, symbol, (StartSymbolDetail) eventDetail);
					break;
				case EventType.StopSymbol:
					StopSymbol(receiver,symbol);
					break;
				case EventType.PositionChange:
					PositionChangeDetail positionChange = (PositionChangeDetail) eventDetail;
					PositionChange(receiver,symbol,positionChange.Position,positionChange.Orders);
					break;
				case EventType.Terminate:
					Dispose();
					break;
				default:
					throw new ApplicationException("Unexpected event type: " + (EventType) eventType);
			}
		}
	}
}
