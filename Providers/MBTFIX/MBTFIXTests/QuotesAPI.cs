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
using System.Security.Cryptography;
using System.Text;

using NUnit.Framework;
using TickZoom.Api;
using TickZoom.MBTFIX;

namespace Test
{
	[TestFixture]
	public class QuotesAPI 
	{
		private static Log log = Factory.SysLog.GetLogger(typeof(QuotesAPI));

		public QuotesAPI()
		{
		}

		[Test]
		public void StockQuotePOSTTest() {
//			PostSubmitter post=new PostSubmitter();
//			post.Url="https://www.mbtrading.com/secure/getquoteserverxml.asp";
//			post.PostItems.Add("username","DEMOXJRX");
//			post.PostItems.Add("password","1clock2bird");
//			string message=post.Post();
//			string expectedMessage = "";
//			Assert.AreEqual(expectedMessage,message);
		}
		
//		[Test]
//		public void ForexQuotePOSTTest() {
//			Progress progress = new Progress();
//			PostSubmitter post = new PostSubmitter(progress);
//			post.Url="https://www.mbtrading.com/secure/getquoteserverxml.asp";
//			post.PostItems.Add("username","DEMOYZPS");
//			post.PostItems.Add("password","1step2wax");
//			string message=post.Post();
//			string expectedMessage = @"<xml><logins username=""DEMOYZPS"" quote_Server=""";
//			Assert.IsTrue(message.StartsWith(expectedMessage));
//		}
		
		private void OnException( Exception ex) {
			log.Error("Exception occurred", ex);
		}
		
		[Test]
		public void ConnectToQuotes() {
			string addrStr = "216.52.236.111";
			ushort port = 5020;
			string userName = "DEMOYZPS";
			string password = "1step2wax";
//			string userName = "DEMOXJRX";
//			string password = "1clock2bird";
			using( Selector selector = Factory.Provider.Selector( OnException))
			using( Socket socket = Factory.Provider.Socket("TestSocket")) {
				socket.PacketFactory = new PacketFactoryMBTQuotes();
				selector.Start();
				socket.SetBlocking(true);
				socket.Connect(addrStr,port);
				socket.SetBlocking(false);
				selector.AddReader(socket);
				selector.AddWriter(socket);
		
				
				Packet packet = socket.CreatePacket();
				string hashPassword = MBTQuotesProvider.Hash(password);
				string login = "L|100="+userName+";133="+hashPassword+"\n";
				packet.DataOut.Write(login.ToCharArray());
				while( !socket.TrySendPacket(packet)) {
					Factory.Parallel.Yield();
				}
				while( !socket.TryGetPacket(out packet)) {
					Factory.Parallel.Yield();
				}
				string response = new string(packet.DataIn.ReadChars(packet.Remaining));
				string expectedResponse = "G|100=DEMOYZPS;8055=demo01\n";
				Assert.AreEqual(expectedResponse,response);
			}
		}
	}
}
