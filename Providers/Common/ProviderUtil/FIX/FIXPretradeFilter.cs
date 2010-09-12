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
using TickZoom.Api;

namespace TickZoom.FIX
{
	public class FIXPretradeFilter : IDisposable {
		private FIXFilter filter;
		private string localAddress = "0.0.0.0";
		private ushort localPort = 0;
		private string remoteAddress;
		private ushort remotePort;
		private static Log log = Factory.SysLog.GetLogger(typeof(FIXPretradeFilter));
		private static bool trace = log.IsTraceEnabled;
		private Selector localSelector;
		private Socket localSocket;
		private Selector remoteSelector;
		private Socket remoteSocket;
		private Packet remotePacket;
		private Packet localPacket;
		private YieldMethod WriteToLocalMethod;
		private YieldMethod WriteToRemoteMethod;
		private Task remoteTask;
		private Task localTask;
		private FIXContext fixContext;
		
		public FIXPretradeFilter(string address, ushort port) {
			this.remoteAddress = address;
			this.remotePort = port;
			WriteToLocalMethod = WriteToLocal;
			WriteToRemoteMethod = WriteToRemote;
			ListenToLocal();
		}
		
		private void ListenToLocal() {
			localSelector = Factory.Provider.Selector(localAddress, localPort, 0, OnException);
			localSelector.OnConnect = OnConnect;
			localSelector.OnDisconnect = OnDisconnect;
			localSelector.Start();
			localPort = localSelector.ListenPort;
			log.Info("Listening to " + localAddress + " on port " + localPort);
		}
		
		private void OnConnect( Socket localSocket) {
			this.localSocket = localSocket;
			this.localSocket.PacketFactory = new PacketFactoryFIX4_4();
			localSelector.AddReader(localSocket);
			localSelector.AddWriter(localSocket);
			log.Info("Received local connection: " + localSocket);
			ConnectToRemote();
		}
		
		private void OnDisconnect( Socket local) {
			if( this.localSocket == local) {
				log.Info("Disconnecting socket: " + local);
				remoteTask.Stop();
				localTask.Stop();
				remoteSocket.Dispose();
				remoteSelector.Dispose();
				this.localSocket.Dispose();
			}
		}
		
		private void ConnectToRemote() {
			remoteSelector = Factory.Provider.Selector( OnException);
			remoteSocket = Factory.Provider.Socket("FilterRemoteSocket");
			remoteSocket.PacketFactory = new PacketFactoryFIX4_4();
			remoteSelector.Start();
			remoteSocket.SetBlocking(true);
			remoteSocket.Connect( remoteAddress,remotePort);
			remoteSocket.SetBlocking(false);
			remoteSelector.AddReader(remoteSocket);
			remoteSelector.AddWriter(remoteSocket);
			remoteTask = Factory.Parallel.Loop( "FilterRemoteRead", OnException, RemoteReadLoop);
			localTask = Factory.Parallel.Loop( "FilterLocalRead", OnException, LocalReadLoop);
			fixContext = new FIXContextDefault( localSocket, remoteSocket);
			log.Info("Connected at " + remoteAddress + " and port " + remotePort + " with socket: " + localSocket);
		}
		
		private Yield RemoteReadLoop() {
			if( remoteSocket.TryGetPacket(out remotePacket)) {
				if( trace) log.Trace("Remote Read: " + remotePacket);
				try {
					if( filter != null) filter.Remote( fixContext, remotePacket);
					return Yield.DidWork.Invoke( WriteToLocalMethod);
				} catch( FilterException) {
					return Yield.Terminate;
				}
			} else {
				return Yield.NoWork.Repeat;
			}
		}
		
		private Yield LocalReadLoop() {
			if( localSocket.TryGetPacket(out localPacket)) {
				if( trace) log.Trace("Local Read: " + localPacket);
				try {
					if( filter != null) filter.Local( fixContext, localPacket);
					return Yield.DidWork.Invoke( WriteToRemoteMethod);
				} catch( FilterException) {
					return Yield.Terminate;
				}
			} else {
				return Yield.NoWork.Repeat;
			}
		}
	
		private Yield WriteToLocal() {
			if( localSocket.TrySendPacket(remotePacket)) {
				if(trace) log.Trace("Local Write: " + remotePacket);
				return Yield.DidWork.Return;
			} else {
				return Yield.NoWork.Repeat;
			}
		}
	
		private Yield WriteToRemote() {
			if( remoteSocket.TrySendPacket(localPacket)) {
				if(trace) log.Trace("Remote Write: " + localPacket);
				return Yield.DidWork.Return;
			} else {
				return Yield.NoWork.Repeat;
			}
		}
		
		private void OnException( Exception ex) {
			log.Error("Exception occurred", ex);
		}
		
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
	            	if( localTask != null) {
	            		localTask.Stop();
	            	}
	            	if( remoteTask != null) {
	            		remoteTask.Stop();
	            	}
	            	if( localSelector != null) {
	            		localSelector.Dispose();
	            	}
	            	if( remoteSelector != null) {
		            	remoteSelector.Dispose();
	            	}
	            	if( localSocket != null) {
		            	localSocket.Dispose();
	            	}
	            	if( remoteSocket != null) {
	            		remoteSocket.Dispose();
	            	}
	            }
    		}
	    }    
	        
		public ushort LocalPort {
			get { return localPort; }
		}
		
		public FIXFilter Filter {
			get { return filter; }
			set { filter = value; }
		}
	}
}