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
		private int heartbeatDelay = 35;
		private bool logRecovery = true;
        private string configFilePath;
        private string configSection;
        private bool hasFirstRecovery = false;
        private bool useLocalFillTime = true;
		private FIXTFactory fixFactory;
        
		public bool UseLocalFillTime {
			get { return useLocalFillTime; }
		}
        
		public bool HasFirstRecovery {
			get { return hasFirstRecovery; }
		}
		
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
		
		protected void RegenerateSocket() {
			Socket old = socket;
			if( socket != null) {
				socket.Dispose();
			}
			if( fixFilterController != null) {
				fixFilterController.Dispose();
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
				string configFile = appDataFolder+@"/Providers/"+providerName+"/Default.config";
				failedFile = appDataFolder+@"/Providers/"+providerName+"/LoginFailed.txt";
				
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
					socket.Connect("127.0.0.1",fixFilterController.LocalPort);
//					socket.Connect("127.0.0.1",port);
					selector.AddWriter(socket);
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
			if( !this.socket.Equals(socket)) {
				log.Info("OnDisconnect( " + this.socket + " != " + socket + " ) - Ignored.");
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
			hasFirstRecovery = true;
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
							var result = OnLogin();
							return result;
						case Status.PendingRecovery:
						case Status.Recovered:
							if( retryDelay != retryStart) {
								retryDelay = retryStart;
								log.Info("(RetryDelay reset to " + retryDelay + " seconds.)");
							}
							if( Factory.Parallel.TickCount >= heartbeatTimeout) {
								log.Warn("Heartbeat Timeout");
								SyncTicks.LogStatus();
								SetupRetry();
								IncreaseRetryTimeout();
								return Yield.DidWork.Repeat;
							}
							Packet packet;
							if( Socket.TryGetPacket(out packet)) {
								if( debug) log.Debug( "Received FIX Message: " + packet);
								if( !CheckForResend(packet)) {
									ReceiveMessage(packet);
								}
								IncreaseRetryTimeout();
								return Yield.DidWork.Repeat;
							} else {
								return Yield.NoWork.Repeat;
							}
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
		
		private bool CheckForResend(Packet packet) {
			var packetFIX = (PacketFIXT1_1) packet;
			var result = false;
			if( packetFIX.MessageType == "2") {
				int end = packetFIX.EndSeqNum == 0 ? fixFactory.LastSequence : packetFIX.EndSeqNum;
				if( debug) log.Debug( "Found resend request for " + packetFIX.BegSeqNum + " to " + end + ": " + packetFIX);
				for( int i = packetFIX.BegSeqNum; i <= end; i++) {
					if( debug) log.Debug("Resending message " + i + "...");
					var message = fixFactory.GetHistory(i);
					message.SetDuplicate(true);
			    	SendMessageInternal( message );
				}
				result = true;
			}
			return result;
		}

		protected void IncreaseRetryTimeout() {
			retryTimeout = Factory.Parallel.TickCount + retryDelay * 1000L;
			heartbeatTimeout = Factory.Parallel.TickCount + (long)heartbeatDelay * 1000L;
		}
		
		protected abstract void OnStartRecovery();
		
		protected abstract void ReceiveMessage(Packet packet);
		
		private long retryTimeout;
		
		private void OnException( Exception ex) {
			// Attempt to propagate the exception.
			log.Error("Exception occurred", ex);
			SendError( ex.Message + "\n" + ex.StackTrace);
			Dispose();
		}
		
        public void Start(Receiver receiver)
        {
        	if( debug) log.Debug("Start() receiver: " + receiver);
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
	        
        private void LoadProperties(string configFilePath) {
        	this.configFilePath = configFilePath;
	        ConfigFile configFile;
	        log.Notice("Using section " + configSection + " in file: " + configFilePath);
			if( !File.Exists(configFilePath) ) {
	        	configFile = new ConfigFile(configFilePath);
	        	configFile.SetValue("EquityDemo/UseLocalFillTime","true");
	        	configFile.SetValue("EquityDemo/ServerAddress","127.0.0.1");
	        	configFile.SetValue("EquityDemo/ServerPort","5679");
	        	configFile.SetValue("EquityDemo/UserName","CHANGEME");
	        	configFile.SetValue("EquityDemo/Password","CHANGEME");
	        	configFile.SetValue("EquityDemo/AccountNumber","CHANGEME");
	        	configFile.SetValue("ForexDemo/UseLocalFillTime","true");
	        	configFile.SetValue("ForexDemo/ServerAddress","127.0.0.1");
	        	configFile.SetValue("ForexDemo/ServerPort","5679");
	        	configFile.SetValue("ForexDemo/UserName","CHANGEME");
	        	configFile.SetValue("ForexDemo/Password","CHANGEME");
	        	configFile.SetValue("ForexDemo/AccountNumber","CHANGEME");
	        	configFile.SetValue("EquityLive/UseLocalFillTime","true");
	        	configFile.SetValue("EquityLive/ServerAddress","127.0.0.1");
	        	configFile.SetValue("EquityLive/ServerPort","5680");
	        	configFile.SetValue("EquityLive/UserName","CHANGEME");
	        	configFile.SetValue("EquityLive/Password","CHANGEME");
	        	configFile.SetValue("EquityLive/AccountNumber","CHANGEME");
	        	configFile.SetValue("ForexLive/UseLocalFillTime","true");
	        	configFile.SetValue("ForexLive/ServerAddress","127.0.0.1");
	        	configFile.SetValue("ForexLive/ServerPort","5680");
	        	configFile.SetValue("ForexLive/UserName","CHANGEME");
	        	configFile.SetValue("ForexLive/Password","CHANGEME");
	        	configFile.SetValue("ForexLive/AccountNumber","CHANGEME");
	        	configFile.SetValue("Simulate/UseLocalFillTime","false");
	        	configFile.SetValue("Simulate/ServerAddress","127.0.0.1");
	        	configFile.SetValue("Simulate/ServerPort","6489");
	        	configFile.SetValue("Simulate/UserName","Simulate1");
	        	configFile.SetValue("Simulate/Password","only4sim");
	        	configFile.SetValue("Simulate/AccountNumber","11111111");
	        } else {
	        	configFile = new ConfigFile(configFilePath);
	        }
			
			ParseProperties(configFile);
		}
        
        private void ParseProperties(ConfigFile configFile) {
			var value = GetField("UseLocalFillTime",configFile, false);
			if( !string.IsNullOrEmpty(value)) {
				useLocalFillTime = value.ToLower() != "false";
        	}
			
        	AddrStr = GetField("ServerAddress",configFile, true);
        	var portStr = GetField("ServerPort",configFile, true);
			if( !ushort.TryParse(portStr, out port)) {
				Exception( "ServerPort", configFile);
			}
			userName = GetField("UserName",configFile, true);
			password = GetField("Password",configFile, true);
			accountNumber = GetField("AccountNumber",configFile, true);
			
			if( File.Exists(failedFile) ) {
				throw new ApplicationException("Please correct the username or password error described in " + failedFile + ". Then delete the file before retrying, please.");
			}
        }
        
        private string GetField( string field, ConfigFile configFile, bool required) {
			var result = configFile.GetValue(configSection + "/" + field);
			if( required && string.IsNullOrEmpty(result)) {
				Exception( field, configFile);
			}
			return result;
        }
        
        private void Exception( string field, ConfigFile configFile) {
        	var sb = new StringBuilder();
        	sb.AppendLine("Sorry, an error occurred finding the '" + field +"' setting.");
        	sb.AppendLine("Please either set '" + field +"' in section '"+configSection+"' of '"+configFile+"'.");
            sb.AppendLine("Otherwise you may choose a different section within the config file.");
            sb.AppendLine("You can choose the section either in your project.tzproj file or");
            sb.AppendLine("if you run a standalone ProviderService, in the ProviderServer\\Default.config file.");
            sb.AppendLine("In either case, you may set the ProviderAssembly value as <AssemblyName>/<Section>");
            sb.AppendLine("For example, MBTFIXProvider/EquityDemo will choose the MBTFIXProvider.exe assembly");
            sb.AppendLine("with the EquityDemo section within the MBTFIXProvider\\Default.config file for that assembly.");
            throw new ApplicationException(sb.ToString());
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
		
		public abstract void PositionChange(Receiver receiver, SymbolInfo symbol, int signal, Iterable<LogicalOrder> orders);
		
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
	    
	    public void SendMessage(FIXTMessage1_1 fixMsg) {
	    	fixFactory.AddHistory(fixMsg);
	    	SendMessageInternal( fixMsg);
	    }
		
	    private void SendMessageInternal(FIXTMessage1_1 fixMsg) {
			var fixString = fixMsg.ToString();
			if( debug) {
				string view = fixString.Replace(FIXTBuffer.EndFieldStr,"  ");
				log.Debug("Send FIX message: \n" + view);
			}
			var packet = Socket.CreatePacket();
			packet.DataOut.Write(fixString.ToCharArray());
			var end = Factory.Parallel.TickCount + (long)heartbeatDelay * 1000L;
			while( !Socket.TrySendPacket(packet)) {
				if( IsInterrupted) return;
				if( Factory.Parallel.TickCount > end) {
					throw new ApplicationException("Timeout while sending heartbeat.");
				}
				Factory.Parallel.Yield();
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
        
		public string ConfigSection {
			get { return configSection; }
			set { configSection = value; }
		}		
		
		public FIXTFactory FixFactory {
			get { return fixFactory; }
			set { fixFactory = value; }
		}
	}
}
