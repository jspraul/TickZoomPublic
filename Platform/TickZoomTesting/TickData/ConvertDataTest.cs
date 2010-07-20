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

#if OBSOLETE

namespace TickZoom.TickData
{
//	[TestFixture]
	public class ConvertDataTest
	{
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		[TestFixtureSetUp]
		public void SetUp() {
		}
		[TestFixtureTearDown]
		public void TearDown() {
		}
		
		bool logging = false;
		
		[Test]
		public void BinaryDiffTest() {
//			string pair = "USD_JPY_Huge";
//			string pair = "USD_JPY_YEARS";
			string pair = "USD_JPY";
			TickReader tickReader = new TickReader();
    		tickReader.LogProgress = true;
    		tickReader.Initialize("TestData",pair);
    		
			byte[] previous = new byte[1024];
			MemoryStream stream = new MemoryStream();
			diffBits = new BitArray(1024);
			long fileSize = 0;
			long compressSize = 0;
			
	    	int count=0;
	    	try { 
				TickBinary tick = new TickBinary();
				TickIO tickIO = new TickImpl();
		    	while(true) {
					tickReader.ReadQueue.Dequeue(ref tick);
		    		stream.Seek(0,SeekOrigin.Begin);
		    		tickIO.Inject(tick);
		    		tickIO.ToWriter(stream);
		    		CompareSignature(stream.GetBuffer(),previous,stream.Position);
		    		fileSize += stream.Position;
		    		int totDiffLength = 1 + diffLength + diffBits.ByteCount;
		    		compressSize += totDiffLength;
		    		if( count > 1000 && count < 2000) {
//		    			log.WriteFile(ByteArrayToString(memory,stream.Position));
		    			if( logging) log.Debug(count + ": " + totDiffLength + " " + diffLength + " byte " + ByteArrayToString(diffBits.Bytes,diffBits.ByteCount) + " " + ByteArrayToString(diff,diffLength));
		    		}
		    		
		    		count++;
		    		Array.Copy(stream.GetBuffer(),previous,stream.Position);
		    	}
			} catch( QueueException ex) {
				Assert.AreEqual(EventType.EndHistorical,ex.EntryType);
	    	} catch( CollectionTerminatedException) {
	    		
	    	}
	    	TickReader.CloseAll();
	    	log.Debug("File Size = " + fileSize + ", Compressed Size = "  + compressSize);
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
		
		byte[] diff = new byte[1024];
		int diffLength = 0;
		BitArray diffBits;
		public void CompareSignature(byte[] memory,byte[] previous,long length) {
			diffBits.Clear();
			int[] sig = CreateSignatureTest.TestSignature;
			byte mem;
			diffLength = 0;
			int counter = 0;
			for( int i=0;i<sig.Length;i++) {
				mem = memory[sig[i]];
				if( mem != previous[sig[i]]) {
					diffBits.Add(true);
					diff[diffLength] = mem;
					diffLength++;
					counter = i;
				} else {
					diffBits.Add(false);
				}
			}
			diffBits.BitCount = counter;
		}
		
		
	}
}

#endif