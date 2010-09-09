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
using TickZoom.MBTFIX;

namespace Test
{
	public class FIXPretradeFilter {
		private ushort port;
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
	
		public FIXPretradeFilter() {
			WriteToLocalMethod = WriteToLocal;
			WriteToRemoteMethod = WriteToRemote;
			ListenToLocal();
		}
		
		private void ListenToLocal() {
			string address = "0.0.0.0";
			localSelector = Factory.Provider.Selector(address, port, 0, OnException);
			localSelector.OnConnect = OnConnect;
			localSelector.OnDisconnect = OnDisconnect;
			localSelector.Start();
			port = localSelector.ListenPort;
			log.Info("Listening to " + address + " on port " + port);
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
			string addrStr = "216.52.236.112";
			ushort port = 5679;
			remoteSelector = Factory.Provider.Selector( OnException);
			remoteSocket = Factory.Provider.Socket("FilterRemoteSocket");
			remoteSocket.PacketFactory = new PacketFactoryFIX4_4();
			remoteSelector.Start();
			remoteSocket.SetBlocking(true);
			remoteSocket.Connect(addrStr,port);
			remoteSocket.SetBlocking(false);
			remoteSelector.AddReader(remoteSocket);
			remoteSelector.AddWriter(remoteSocket);
			remoteTask = Factory.Parallel.Loop( "FilterRemoteRead", OnException, RemoteReadLoop);
			localTask = Factory.Parallel.Loop( "FilterLocalRead", OnException, LocalReadLoop);
			log.Info("Connected at " + addrStr + " and port " + port + " with socket: " + localSocket);
		}
		
		private Yield RemoteReadLoop() {
			if( remoteSocket.TryGetPacket(out remotePacket)) {
				if( trace) log.Trace("Remote Read: " + remotePacket);
				return Yield.DidWork.Invoke( WriteToLocalMethod);
			} else {
				return Yield.NoWork.Repeat;
			}
		}
		
		private Yield LocalReadLoop() {
			if( localSocket.TryGetPacket(out localPacket)) {
				if( trace) log.Trace("Local Read: " + localPacket);
				return Yield.DidWork.Invoke( WriteToRemoteMethod);
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
		
		public ushort Port {
			get { return port; }
		}
	}
}