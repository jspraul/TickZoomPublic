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

#define FOREX
using System;
using System.IO;
using System.Media;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using NUnit.Framework;
using TickZoom.Api;
using TickZoom.MBTFIX;

namespace Test
{
	[TestFixture]
	public class FIXAPI 
	{
		private static Log log = Factory.Log.GetLogger(typeof(FIXAPI));
	
		public FIXAPI()
		{ 
		}
	
		private void OnException( Exception ex) {
			log.Error("Exception occurred", ex);
		}
		
		[Test]
		public void TestPositionLength() {
			MemoryStream memory = new MemoryStream();
			memory.Position = 100;
			memory.SetLength( 100);
			memory.SetLength( memory.Length + 1);
			Assert.AreEqual(100,memory.Position);
		}

		private void run() {
   			SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\chimes.wav");
    		simpleSound.Play();
			log.Info("runing " + (runTime / 1000) + " seconds.");
			Thread.Sleep(runTime);
		}
		
		private void Wait() {
   			SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\ding.wav");
    		simpleSound.Play();
			log.Info("Delaying " + (delayTime / 1000) + " seconds.");
			Thread.Sleep(delayTime);
		}
		
		int runMaximum = 10000;
		int runIncrement = 3000;
		int runMinimum = 1000;
		int delayMaximum = 30000;
		int delayIncrement = 5000;
   		int delayMinimum = 2000;
		int runTime;
		int delayTime;
		Random rand = new Random();
		
//		[Test]
//		public void TestTiming() {
//			runTime = rand.Next(runMinimum,runMaximum);
//			delayTime = delayMaximum;
//			while( true) {
//				Wait();
//				run();
//				DecreaseTimes();
//				TryResetTimes();
//			}
//		}
		
		private void IncreaseTimes() {
		}
		
		private void DecreaseTimes() {
			int decrease = rand.Next(0,runIncrement);
			runTime -= decrease;
			decrease  = rand.Next(0,delayIncrement);
			delayTime -= decrease;
			delayTime = Math.Max( runTime, delayTime);
		}
		
		private void TryResetTimes() {
			if( delayTime <= delayMinimum) {
				int newTime = rand.Next(delayMinimum,delayMaximum);
				delayTime = newTime;
				newTime = rand.Next(runMinimum,runMaximum);
				runTime = newTime;
			}
			if( runTime <= runMinimum) {
				int newTime = rand.Next(runMinimum,runMaximum);
				runTime = newTime;
			}
			delayTime = Math.Max( runTime, delayTime);
		}
		
		[Test]
		public unsafe void ConnectToFIX() {
			string addrStr = "216.52.236.112";
			ushort port = 5679;
			// Forex
//			string password = "1step2wax";
//			string userName = "DEMOYZPSFIX";
			// Equity
			string password = "1lake2dust";
			string userName = "DEMOXJSPFIX";
			using( Selector selector = Factory.Provider.Selector( OnException))
			using( Socket socket = Factory.Provider.Socket("TestSocket")) {
				socket.PacketFactory = new PacketFactoryFIX4_4();
				selector.Start();
				socket.SetBlocking(true);
				socket.Connect(addrStr,port);
				socket.SetBlocking(false);
				selector.AddReader(socket);
				selector.AddWriter(socket);
		
				Packet packet = socket.CreatePacket();
				string hashPassword = MBTQuotesProvider.Hash(password);
				
				var mbtMsg = new FIXMessage4_4(userName);
				mbtMsg.SetEncryption(0);
				mbtMsg.SetHeartBeatInterval(30);
				mbtMsg.ResetSequence();
				mbtMsg.SetEncoding("554_H1");
				mbtMsg.SetPassword(password);
				mbtMsg.AddHeader("A");
				
				string login = mbtMsg.ToString();
//				string view = login.Replace(FIXTBuffer.EndField,'\n');
				packet.DataOut.Write(login.ToCharArray());
				log.Info("Login message: \n" + packet );
				while( !socket.TrySendPacket(packet)) {
					Factory.Parallel.Yield();
				}
				
				long end = Factory.Parallel.TickCount + 5000;
				while( !socket.TryGetPacket(out packet)) {
					if( Factory.Parallel.TickCount > end) {
						Assert.Fail("Login Timed Out.");
					}
					Factory.Parallel.Yield();
				}
				PacketFIX4_4 packetFIX = (PacketFIX4_4) packet;
				
//				packetFIX.ReadMessage();
				Assert.AreEqual("FIX.4.4",packetFIX.Version);
				Assert.AreEqual("A",packetFIX.MessageType);
				Assert.AreEqual("MBT",packetFIX.Sender);
				Assert.AreEqual(userName,packetFIX.Target);
				Assert.AreEqual(1,packetFIX.Sequence);
				Assert.NotNull(packetFIX.TimeStamp);
				Assert.AreEqual("0",packetFIX.Encryption);
				Assert.AreEqual(30,packetFIX.HeartBeatInterval);
			}
		}
		
		private void OnDisconnect( Socket socket) {
			log.Warn("OnDisconnect(" + socket + ")");
		}
		
		[Test]
		public unsafe void ConnectToFIXFails() {
			string addrStr = "216.52.236.112";
			ushort port = 5679;
			string password = "badpassword";
			string userName = "DEMOXJSPFIX";
			using( Selector selector = Factory.Provider.Selector( OnException))
			using( Socket socket = Factory.Provider.Socket("TestSocket")) {
				socket.PacketFactory = new PacketFactoryFIX4_4();
				selector.Start();
				selector.OnDisconnect = OnDisconnect;
				socket.SetBlocking(true);
				socket.Connect(addrStr,port);
				socket.SetBlocking(false);
				selector.AddReader(socket);
				selector.AddWriter(socket);
		
				Packet packet = socket.CreatePacket();
				string hashPassword = MBTQuotesProvider.Hash(password);
				
				var mbtMsg = new FIXMessage4_4(userName);
				mbtMsg.SetEncryption(0);
				mbtMsg.SetHeartBeatInterval(30);
				mbtMsg.ResetSequence();
				mbtMsg.SetEncoding("554_H1");
				mbtMsg.SetPassword(password);
				mbtMsg.AddHeader("A");
				
				string login = mbtMsg.ToString();
				packet.DataOut.Write(login.ToCharArray());
				log.Info("Login message: \n" + packet );
				while( !socket.TrySendPacket(packet)) {
					Factory.Parallel.Yield();
				}
				
				long end = Factory.Parallel.TickCount + 5000;
				while( !socket.TryGetPacket(out packet)) {
					if( Factory.Parallel.TickCount > end) {
						return;
					}
					Factory.Parallel.Yield();
				}
				Assert.Fail("Should have received login timed out response.");
			}
		}
	}
}
