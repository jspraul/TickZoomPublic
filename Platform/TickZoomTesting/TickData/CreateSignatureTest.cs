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
using NUnit.Framework;
using TickZoom.Api;

#if OBSOLETE

namespace TickZoom.TickData
{
//	[TestFixture]
	public class CreateSignatureTest
	{
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		[SetUp]
		public void SetUp() {
		}
		[TearDown]
		public void TearDown() {
			TickReader.CloseAll();
		}
		
		public static int[] TestSignature = new int[] { 2, 3, 1, 28, 29, 51, 27, 35, 52, 30, 26, 41, 53, 54, 39, 36, 43, 49, 44, 50, 57, 58, 55, 56, 42, 45, 46, 19, 20, 40, 47, 48, 18, 11, 12, 21, 4, 10, 13, 0, 5, 6, 7, 8, 9, 14, 15, 16, 17, 22, 23, 24, 25, 31, 32, 33, 34, 37, 38 };
		
		[Test]
		public void SignatureResult() {
			string pair = "USD_JPY_Volume";
			TickReader tickReader = new TickReader();
    		tickReader.LogProgress = true;
    		tickReader.Initialize("TestData",pair);
    		int totalBytes = 0;
    		SignatureCompressor compressor = new SignatureCompressor();
    		ByteMemory compressed = new ByteMemory("compressed");
    		int compressedLength = 0;
	    	try { 
				TickBinary tick = new TickBinary();
    			TickImpl tickImpl = new TickImpl();
				for(int i=0;i<101;i++) {
    				tickReader.ReadQueue.Dequeue( ref tick);
					tickImpl.Inject(tick);
					compressed.Reset();
					compressor.CompressTick(tick,compressed.Memory);
					totalBytes+=compressedLength;
		    	}
	    		log.Debug( compressor.Signature.ToString());
	    		Assert.AreEqual( TestSignature, compressor.Signature);
				for(int i=0;i<200;i++) {
	    			tickReader.ReadQueue.Dequeue(ref tick);
					tickImpl.Inject(tick);
					compressed.Reset();
					compressor.CompressTick(tick,compressed.Memory);
					totalBytes+=compressedLength;
					int length = compressed.Bytes[0];
					int diffBytes = length/8+1;
					log.Debug( compressedLength + ": " +
					              compressed.Bytes[0] + " " + 
					              ByteArrayToString(compressor.Difference,0,compressor.DiffLength));
		    	}
	    	} catch( CollectionTerminatedException) {
	    		
	    	}
    		log.Debug("Total Compressed Bytes: " + totalBytes);
    		compressor.LogCounts();
		}
		
		[Test]
		unsafe public void TickToBytes() {
			byte[] bytes = new byte[sizeof(TickBinary)];	
		}
		
//		[Test]
		public void CompressTickTest() {
			string pair = "USD_JPY_Volume";
			TickReader tickReader = new TickReader();
    		tickReader.LogProgress = true;
    		tickReader.Initialize("TestData",pair);
    		int totalBytes = 0;
    		SignatureCompressor compressor = new SignatureCompressor();
    		SignatureCompressor decompressor = new SignatureCompressor();
    		ByteMemory output = new ByteMemory("compressed");
    		int length = 0;
	    	try { 
				TickBinary tick = new TickBinary();
    			TickImpl tickImpl = new TickImpl();
				for(int i=0;i<101;i++) {
    				tickReader.ReadQueue.Dequeue(ref tick);
					tickImpl.Inject(tick);
					compressor.CompareTick(tickImpl);
					if( compressor.Count < 100) {
						compressor.CopyMemory(output.Bytes,out length);
					}
		    		compressor.SwapBuffers();
					compressor.Count++; 
					if( compressor.Count % 100 == 0) {
						compressor.CreateSignature();
						compressor.ResetCounters();
					}
				} 
    			string temp = "";
    			for( int i=0; i<compressor.Signature.Length; i++) {
    				if( i!=0) { temp += ", "; }
    				temp += compressor.Signature[i];
    			}
    			log.Notice( "signature = " + temp);
	    		Assert.AreEqual( TestSignature, compressor.Signature);
	    		
	    		byte[] buffer = new byte[1024];
				for(int i=0;i<200;i++) {
	    			tickReader.ReadQueue.Dequeue(ref tick);
					tickImpl.Inject(tick);
					compressor.CompareTick(tickImpl);
					compressor.CalculateDifference(compressor.Current.Bytes,compressor.Previous.Bytes,compressor.Current.Length);
					Array.Copy(compressor.Current.Bytes,buffer,compressor.Current.Length);
					compressor.ReverseDifference(compressor.Current.Bytes,compressor.Previous.Bytes,compressor.Previous.Length);
					Assert.AreEqual(buffer,compressor.Current.Bytes);
					compressor.WriteMemory(output.Bytes,out length);
		    		compressor.SwapBuffers();
					compressor.Count++; 
					if( compressor.Count % 100 == 0) {
						compressor.CreateSignature();
						compressor.ResetCounters();
					}
				}
	    	} catch( CollectionTerminatedException) {
	    		
	    	}
    		log.Debug("Total Compressed Bytes: " + totalBytes);
    		compressor.LogCounts();
		}
		
		public string ByteArrayToString(byte[] ba, int offset, int length)
		{
			string hex = BitConverter.ToString(ba,offset,length);
		    return hex;
		}
		
		public string ByteToString(byte ba)
		{
			return Convert.ToString(ba,2);
		}
		
		
	}
}

#endif