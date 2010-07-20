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
using System.Text;

using NUnit.Framework;
using TickZoom.Api;
using TickZoom.TickUtil;

namespace TickZoom.System.TickData
{
	[TestFixture]
	public class CompressTest
	{
		Log log = Engine.Log;
		[TestFixtureSetUp]
		public void SetUp() {
			log.Clear();
		}
		[TestFixtureTearDown]
		public void TearDown() {
			log.Disconnect();
		}
		
		[Test] 
		public void CompressionTest() {
//			string pair = "USD_JPY_Huge";
			string pair = "USD_JPY_YEARS";
			TickReader tickReader = new TickReader();
    		tickReader.LogProgress = true;
    		tickReader.Initialize("TestData",pair);

			TickCompressor compressor = new TickCompressor(true);
    		compressor.LogProgress = true;
    		compressor.Initialize("Compressed",pair);
    		
			long fileSize = 0;
			long compressSize = 0;
			
			TickIO tick = new TickImpl();
			TickImpl tickImpl = new TickImpl();
			TickBinary tickBinary = new TickBinary();
			try {
				for(int i=0;;i++) {
					if( !tickReader.ReadQueue.Dequeue(ref tickBinary)) continue;
					tickImpl.init( tickBinary);
					compressor.Add( tickImpl);
				}
			} catch( QueueException ex) {
				Assert.AreEqual(EntryType.EndHistorical,ex.EntryType);
	    	} catch( CollectionTerminatedException) {
	    		
	    	}
			tickReader.Stop();
			compressor.Close();
	    	log.WriteFile("File Size = " + fileSize + ", Compressed Size = "  + compressSize);
		}
		
		public string ByteArrayToString(byte[] ba, long length)
		{
			string hex = BitConverter.ToString(ba,0,(int)length);
		  return hex.Replace("-","");
		}
		
		public string ByteToString(byte ba)
		{
			return Convert.ToString(ba,2);
		}

	}
}
