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
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using TickZoom.Api;

namespace TickZoom.FIX
{

	public abstract class FIXProviderSupport : Provider
	{
		private FIXFilter fixFilter;
		private FIXPretradeFilter fixFilterController;
		private readonly Log log;
		private readonly bool debug;
		private readonly bool trace;
		private static long nextConnectTime = 0L;
		protected readonly object symbolsRequestedLocker = new object();
		protected Dictionary<long,SymbolInfo> symbolsRequested = new Dictionary<long, SymbolInfo>();
		private Selector selector;
		private Socket socket;
		private Task socketTask;
		private string failedFile;
		protected Receiver receiver;
		private long retryDelay = 30; // seconds
		private long retryStart = 30; // seconds
		private long retryIncrease = 5;
		private long retryMaximum = 30;
		private volatile Status connectionStatus = Status.New;
		private string addrStr;
		private ushort port;
		private string userName;
		private	string password;
		private	string accountNumber;
		public abstract void OnDisconnect();
		public abstract void OnRetry();
		public abstract Yield OnLogin();
		private string providerName;
		private long heartbeatTimeout;
		private int heartbeatDelay = 30;
		private bool logRecovery = false;
		
		public FIXProviderSupport()
		{
			log = Factory.SysLog.GetLogger(typeof(FIXProviderSupport)+"."+GetType().Name);
			debug = log.IsDebugEnabled;
			trace = log.IsTraceEnabled;
        	log.Info(providerName+" Startup");
			selector = Factory.Provider.Selector( OnException);
			selector.OnDisconnect = OnDisconnect;
			selector.Start();
			RegenerateSocket();
			socketTask = Factory.Parallel.Loop(GetType().Name, OnException, SocketTask);
  			string logRecoveryString = Factory.Settings["LogRecovery"];
  			logRecovery = !string.IsNullOrEmpty(logRecoveryString) && logRecoveryString.ToLower().Equals("true");
        }
		
		private void RegenerateSocket() {
			Socket old = socket;
			if( socket != null) {
				socket.Dispose();
			}
			socket = Factory.Provider.Socket("MBTFIXSocket");
			socket.PacketFactory = new PacketFactoryFIX4_4();
			if( debug) log.Debug("Created new " + socket);
			connectionStatus = Status.New;
			if( trace) {
				string message = "Generated socket: " + socket;
				if( old != null) {
					message += " to replace: " + old;
				}
				log.Trace(message);
			}
		}
		
		protected void Initialize() {
        	try { 
				if( debug) log.Debug("> Initialize.");
				string appDataFolder = Factory.Settings["AppDataFolder"];
				if( appDataFolder == null) {
					throw new ApplicationException("Sorry, AppDataFolder must be set in the app.config file.");
				}
				string configFile = appDataFolder+@"/Providers/"+providerName+".config";
				failedFile = appDataFolder+@"/Providers/"+providerName+"LoginFailed.txt";
				
				LoadProperties(configFile);
				
        	} catch( Exception ex) {
        		log.Error(ex.Message,ex);
        		throw;
        	}
        	// Initiate socket connection.
        	while( true) {
        		try { 
        			fixFilterController = new FIXPretradeFilter(addrStr,port);
        			fixFilterController.Filter = fixFilter;
					selector.AddWriter(socket);
					socket.Connect("127.0.0.1",fixFilterController.LocalPort);
					if( debug) log.Debug("Requested Connect for " + socket);
					return;
        		} catch( SocketErrorException ex) {
        			log.Error("Non fatal error while trying to connect: " + ex.Message);
        			RegenerateSocket();
        		}
        	}
		}
		
		public enum Status {
			New,
			Connected,
			PendingLogin,
			PendingRecovery,
			Recovered,
			Disconnected,
			PendingRetry
		}
		
		public void FailLogin(string packetString) {
			string message = "Login failed for user name: " + userName + " and password: " + new string('*',password.Length);
			string fileMessage = "Resolve the problem and then delete this file before you retry.";
			string logMessage = "Resolve the problem and then delete the file " + failedFile + " before you retry.";
			if( File.Exists(failedFile)) {
				File.Delete(failedFile);
			}
			using( var fileOut = new StreamWriter(failedFile)) {
				fileOut.WriteLine(message);
				fileOut.WriteLine(fileMessage);
				fileOut.WriteLine("Actual FIX message for login that failed:");
				fileOut.WriteLine(packetString);
			}
			log.Error(message + " " + logMessage + "\n" + packetString);
			throw new ApplicationException(message + " " + logMessage);
		}
		
		private void OnDisconnect( Socket socket) {
			if( this.socket != socket) {
				log.Warn("OnDisconnect( " + this.socket + " != " + socket + " ) - Ignored.");
				return;
			}
			log.Info("OnDisconnect( " + socket + " ) ");
			connectionStatus = Status.Disconnected;
		}
	
		public bool IsInterrupted {
			get {
				return isDisposed || socket.State != SocketState.Connected;
			}
		}

		public void StartRecovery() {
			connectionStatus = Status.PendingRecovery;
			if( debug) log.Debug("ConnectionStatus changed to: " + connectionStatus);
			OnStartRecovery();
		}
		
		public void EndRecovery() {
			connectionStatus = Status.Recovered;
			if( debug) log.Debug("ConnectionStatus changed to: " + connectionStatus);
		}
		
		public bool IsRecovered {
			get { 
				return connectionStatus == Status.Recovered;
			}
		}
		
		private void SetupRetry() {
			OnRetry();
			RegenerateSocket();
			if( trace) log.Trace("ConnectionStatus changed to: " + connectionStatus);
		}
		
		public bool IsRecovery {
			get {
				return connectionStatus == Status.PendingRecovery;
			}
		}
		
		private Yield SocketTask() {
			if( isDisposed ) return Yield.NoWork.Repeat;
			switch( socket.State) {
				case SocketState.New:
					if( receiver != null && Factory.Parallel.TickCount > nextConnectTime) {
						Initialize();
						retryTimeout = Factory.Parallel.TickCount + retryDelay * 1000;
						log.Info("Connection will timeout and retry in " + retryDelay + " seconds.");
						return Yield.DidWork.Repeat;
					} else {
						return Yield.NoWork.Repeat;
					}
				case SocketState.PendingConnect:
					if( Factory.Parallel.TickCount >= retryTimeout) {
						log.Warn("Connection Timeout");
						SetupRetry();
						retryDelay += retryIncrease;
						retryDelay = Math.Min(retryDelay,retryMaximum);
						return Yield.DidWork.Repeat;
					} else {
						return Yield.NoWork.Repeat;
					}
				case SocketState.Connected:
					if( connectionStatus == Status.New) {
						connectionStatus = Status.Connected;
						if( debug) log.Debug("ConnectionStatus changed to: " + connectionStatus);
					}
					switch( connectionStatus) {
						case Status.Connected:
							connectionStatus = Status.PendingLogin;
							if( debug) log.Debug("ConnectionStatus changed to: " + connectionStatus);
							selector.AddReader(socket);
							IncreaseRetryTimeout();
							Yield result = OnLogin();
							return result;
						case Status.PendingRecovery:
						case Status.Recovered:
							if( retryDelay != retryStart) {
								retryDelay = retryStart;
								log.Info("(RetryDelay reset to " + retryDelay + " seconds.)");
							}
							if( Factory.Parallel.TickCount >= heartbeatTimeout) {
								log.Warn("Heartbeat Timeout");
								SetupRetry();
								IncreaseRetryTimeout();
								return Yield.DidWork.Repeat;
							}
							result = ReceiveMessage();
							if( !result.IsIdle) {
								IncreaseRetryTimeout();
							}
							return result;
						case Status.PendingLogin:
						default:
							return Yield.NoWork.Repeat;
					}
				case SocketState.Disconnected:
					switch( connectionStatus) {
						case Status.Disconnected:
							OnDisconnect();
							retryTimeout = Factory.Parallel.TickCount + retryDelay * 1000;
							connectionStatus = Status.PendingRetry;
							if( debug) log.Debug("ConnectionStatus changed to: " + connectionStatus + ". Retrying in " + retryDelay + " seconds.");
							retryDelay += retryIncrease;
							retryDelay = Math.Min(retryDelay,retryMaximum);
							return Yield.NoWork.Repeat;
						case Status.PendingRetry:
							if( Factory.Parallel.TickCount >= retryTimeout) {
								log.Warn("Retry Time Elapsed");
								OnRetry();
								RegenerateSocket();
								if( trace) log.Trace("ConnectionStatus changed to: " + connectionStatus);
								return Yield.DidWork.Repeat;
							} else {
								return Yield.NoWork.Repeat;
							}
						default:
							return Yield.NoWork.Repeat;
					}
				default:
					string message = "Unknown socket state: " + socket.State;
					log.Error( message);
					throw new ApplicationException(message);
			}
		}

		protected void IncreaseRetryTimeout() {
			retryTimeout = Factory.Parallel.TickCount + retryDelay * 1000;
			heartbeatTimeout = Factory.Parallel.TickCount + heartbeatDelay * 1000;
		}
		
		protected abstract void OnStartRecovery();
		
		protected abstract Yield ReceiveMessage();
		
		private long retryTimeout;
		
		private void OnException( Exception ex) {
			// Attempt to propagate the exception.
			log.Error("Exception occurred", ex);
			SendError( ex.Message + "\n" + ex.StackTrace);
			Dispose();
		}
		
        public void Start(Receiver receiver)
        {
        	this.receiver = (Receiver) receiver;
        }
        
        public void Stop(Receiver receiver) {
        	
        }

        public void StartSymbol(Receiver receiver, SymbolInfo symbol, StartSymbolDetail detail)
        {
        	log.Info("StartSymbol( " + symbol + ")");
        	if( this.receiver != receiver) {
        		throw new ApplicationException("Invalid receiver. Only one receiver allowed for " + this.GetType().Name);
        	}
        	// This adds a new order handler.
        	TryAddSymbol(symbol);
        	OnStartSymbol(symbol);
        }
        
        public abstract void OnStartSymbol( SymbolInfo symbol);
        
        public void StopSymbol(Receiver receiver, SymbolInfo symbol)
        {
        	log.Info("StopSymbol( " + symbol + ")");
        	if( this.receiver != receiver) {
        		throw new ApplicationException("Invalid receiver. Only one receiver allowed for " + this.GetType().Name);
        	}
        	if( TryRemoveSymbol(symbol)) {
        		OnStopSymbol(symbol);
        	}
        }
        
        public abstract void OnStopSymbol(SymbolInfo symbol);
	        
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
		            sw.WriteLine("####################################");
		            sw.WriteLine("# The first 2 properties determine which user name and password get used.");
		            sw.WriteLine("####################################");
		            sw.WriteLine("EquityOrForexOrCombo=equity");
		            sw.WriteLine("LiveOrDemo=demo");
		            sw.WriteLine();
		            sw.WriteLine("DemoAddress=127.0.0.1");
		            sw.WriteLine("DemoPort=5679");
		            sw.WriteLine();
		            sw.WriteLine("LiveAddress=127.0.0.1");
		            sw.WriteLine("LivePort=5680");
		            sw.WriteLine();
		            sw.WriteLine("ForexDemoUserName=CHANGEME");
		            sw.WriteLine("ForexDemoPassword=CHANGEME");
		            sw.WriteLine("ForexDemoAccountNumber=CHANGEME");
		            sw.WriteLine();
		            sw.WriteLine("ForexLiveUserName=CHANGEME");
		            sw.WriteLine("ForexLivePassword=CHANGEME");
		            sw.WriteLine("ForexLiveAccountNumber=CHANGEME");
		            sw.WriteLine();
		            sw.WriteLine("ComboDemoUserName=CHANGEME");
		            sw.WriteLine("ComboDemoPassword=CHANGEME");
		            sw.WriteLine("ComboDemoAccountNumber=CHANGEME");
		            sw.WriteLine();
		            sw.WriteLine("ComboLiveUserName=CHANGEME");
		            sw.WriteLine("ComboLivePassword=CHANGEME");
		            sw.WriteLine("ComboLiveAccountNumber=CHANGEME");
		            sw.WriteLine();
		            sw.WriteLine("EquityDemoUserName=CHANGEME");
		            sw.WriteLine("EquityDemoPassword=CHANGEME");
		            sw.WriteLine("EquityDemoAccountNumber=CHANGEME");
		            sw.WriteLine();
		            sw.WriteLine("EquityLiveUserName=CHANGEME");
		            sw.WriteLine("EquityLivePassword=CHANGEME");
		            sw.WriteLine("EquityLiveAccountNumber=CHANGEME");
		        }
			} 
			
			foreach (var row in File.ReadAllLines(configFile)) {
				if( string.IsNullOrEmpty(row) || row.TrimStart()[0] == '#') continue;
				string[] nameValue = row.Split('=');
				data[nameValue[0].Trim().ToLower()] = nameValue[1].Trim();
			}
			
			ParseProperties();
		}
        
        private void ParseProperties() {
        	string equityForexCombo = GetProperty("EquityOrForexOrCombo");
			equityForexCombo = equityForexCombo.ToLower();
			switch( equityForexCombo) {
				case "equity":
				case "forex":
				case "combo":
					break;
				default:
					throw new ApplicationException("Please set 'EquityOrForexOrCombo' to either equity,forex, or combo in '"+configFile+"'.");
			}
			string liveOrDemo = GetProperty("LiveOrDemo");
			liveOrDemo = liveOrDemo.ToLower();
			switch( liveOrDemo) {
				case "live":
				case "demo":
					break;
				default:
					throw new ApplicationException("Please set 'LiveOrDemo' to live, or demo in '"+configFile+"'.");
			}
			
			string prefix = equityForexCombo + liveOrDemo;
			
			AddrStr = GetProperty(liveOrDemo + "Address");
			string portStr = GetProperty(liveOrDemo + "port");
			port = ushort.Parse(portStr);
			userName = GetProperty(prefix + "UserName");
			password = GetProperty(prefix + "Password");
			accountNumber = GetProperty(prefix + "AccountNumber");
			
			if( File.Exists(failedFile) ) {
				throw new ApplicationException("Please correct the username or password error described in " + failedFile + ". Then delete the file before retrying, please.");
			}
        }
	        
        private string GetProperty( string name) {
        	string value;
        	if( !data.TryGetValue(name.ToLower(),out value) ||
        	   string.IsNullOrEmpty(value) || value.Contains("CHANGEME")) {
				throw new ApplicationException(name + " property must be set in " + configFile);
			}
        	return value;
        }
	        
		private string UpperFirst(string input)
		{
			string temp = input.Substring(0, 1);
			return temp.ToUpper() + input.Remove(0, 1);
		}        
		
		public void SendError(string error) {
			if( receiver!= null) {
				ErrorDetail detail = new ErrorDetail();
				detail.ErrorMessage = error;
				receiver.OnEvent(null,(int)EventType.Error, detail);
			}
		}
		
		public bool GetSymbolStatus(SymbolInfo symbol) {
			lock( symbolsRequestedLocker) {
				return symbolsRequested.ContainsKey(symbol.BinaryIdentifier);
			}
		}
		
		private bool TryAddSymbol(SymbolInfo symbol) {
			lock( symbolsRequestedLocker) {
				if( !symbolsRequested.ContainsKey(symbol.BinaryIdentifier)) {
					symbolsRequested.Add(symbol.BinaryIdentifier,symbol);
					return true;
				}
			}
			return false;
		}
		
		private bool TryRemoveSymbol(SymbolInfo symbol) {
			lock( symbolsRequestedLocker) {
				if( symbolsRequested.ContainsKey(symbol.BinaryIdentifier)) {
					symbolsRequested.Remove(symbol.BinaryIdentifier);
					return true;
				}
			}
			return false;
		}
		
		public abstract void PositionChange(Receiver receiver, SymbolInfo symbol, double signal, Iterable<LogicalOrder> orders);
		
	 	protected volatile bool isDisposed = false;
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
	            	if( debug) log.Debug("Dispose()");
	            	if( socketTask != null) {
		            	socketTask.Stop();
	            	}
	            	if( selector != null) {
		            	selector.Dispose();
	            	}
	            	if( socket != null) {
		            	socket.Dispose();
	            	}
	            	if( fixFilterController != null) {
	            		fixFilterController.Dispose();
	            	}
	            	nextConnectTime = Factory.Parallel.TickCount + 10000;
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
					StartSymbol(receiver,symbol, (StartSymbolDetail) eventDetail);
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
		
		public Socket Socket {
			get { return socket; }
		}
		
		public string AddrStr {
			get { return addrStr; }
			set { addrStr = value; }
		}
		
		public ushort Port {
			get { return port; }
			set { port = value; }
		}
		
		public string UserName {
			get { return userName; }
			set { userName = value; }
		}
		
		public string Password {
			get { return password; }
			set { password = value; }
		}
		
		public string ProviderName {
			get { return providerName; }
			set { providerName = value; }
		}
		
		public long RetryStart {
			get { return retryStart; }
			set { retryStart = retryDelay = value; }
		}
		
		public long RetryIncrease {
			get { return retryIncrease; }
			set { retryIncrease = value; }
		}
		
		public long RetryMaximum {
			get { return retryMaximum; }
			set { retryMaximum = value; }
		}
		
		public int HeartbeatDelay {
			get { return heartbeatDelay; }
			set { heartbeatDelay = value;
				IncreaseRetryTimeout();
			}
		}
		
		public bool LogRecovery {
			get { return logRecovery; }
		}
		
		public string AccountNumber {
			get { return accountNumber; }
		}
		
		public FIXProviderSupport.Status ConnectionStatus {
			get { return connectionStatus; }
		}
		
		public FIXFilter FIXFilter {
			get { return fixFilter; }
			set { fixFilter = value; }
		}
	}
}
