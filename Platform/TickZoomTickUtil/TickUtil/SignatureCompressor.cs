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
using TickZoom.Api;
using TickZoom.TickUtil;

namespace TickZoom.TickUtil
{
	public class SignatureCompressor
	{
		Log log = Factory.SysLog.GetLogger(typeof(SignatureCompressor));
		ByteMemory current = new ByteMemory("current");
		ByteMemory previous = new ByteMemory("previous");
		Dictionary<int,ByteCount> byteCounts;
		List<ByteCount> shortValueCounts;
		int[] signature = null;
		int maxLength=0;
		bool shortValueCompression=false;
		TickImpl currentTick;
    	
		bool logging = false;
		
		int count = 0;
		
		public SignatureCompressor() {
			currentTick = new TickImpl();
			ResetCounters();
			if( shortValueCompression) {
				shortValueCounts = new List<ByteCount>(256*256);
				for( int i=0; i<shortValueCounts.Capacity; i++) {
					shortValueCounts.Add(new ByteCount(i,0));
				}
			}
		}
		
		public void ResetCounters() {
			byteCounts = new Dictionary<int,ByteCount>();
		}
		
		private void CompareTick( BinaryReader reader) {
			byte version = reader.ReadByte();
			currentTick.FromReader(version,reader);
			CompareTick(currentTick);
		}
		
		public void CompareTick( TickIO tick) {
	    	tick.ToWriter(current.Memory);
    		if( count>0) {
    			CompareMemory(current.Bytes,previous.Bytes,current.Length);
    		}
		}
		
		// This save copying and reallocating.
		public void SwapBuffers() {
		    ByteMemory temp = previous;
    		previous = current;
    		current = temp;
    		current.Reset();
		}
		
		private void CompareMemory(byte[] memory,byte[] previous,long length) {
			int counter=0;
			for( int i=0;i<length;i++) {
				if( memory[i] != previous[i]) {
					ByteCount value;
					if( byteCounts.TryGetValue(i,out value)) {
						byteCounts[i] = new ByteCount(i,byteCounts[i].Count+1);
					} else {
						byteCounts.Add(i,new ByteCount(i,1));
					}
					counter++;
				}
			}
			maxLength = (int) Math.Max(maxLength,length);
		}
		
		
		public void CreateSignature() {
			// Ensure there's a ByteCount for every byte
			for( int i=0; i<maxLength; i++) {
				ByteCount value;
				if( !byteCounts.TryGetValue(i,out value) ) {
					byteCounts.Add(i,new ByteCount(i,0));
				}
			}
			
			// Sort the byte counts in reverse order.
			List<ByteCount> counts = new List<ByteCount>(byteCounts.Values);
			counts.Sort();
			counts.Reverse();
			
			signature = new int[maxLength];
			// Log the signature.
			for( int i=0;i<maxLength;i++) {
				signature[i] = counts[i].Offset;
			}
			if( logging) {
				string signatureStr = "{ ";
				for( int i=0;i<maxLength;i++) {
					if( i!=0) { signatureStr += ", "; }
					signatureStr += counts[i].Offset;
				}
				signatureStr += "}";
				log.Debug("Signature: " + signatureStr);
			}
		}

		public void LogCounts() {
			if( shortValueCompression) {
				shortValueCounts.Sort();
				shortValueCounts.Reverse();
				string intValueCountStr = "";
				int count=0;
				int totalCount=0;
				int totalUnder255 = 0;
				for( int i=0;i<shortValueCounts.Count;i++) {
					if(i!=0) intValueCountStr+= ", ";
					if( shortValueCounts[i].Count>0) {
						totalCount+=shortValueCounts[i].Count;
						if( count < 255) {
							totalUnder255+=shortValueCounts[i].Count;		
						}
						count++;
	//					log.Info( intValueCounts[i].Offset+"="+intValueCounts[i].Count);
					}
				}
				log.Debug( count + " int combinations" );
				log.Debug( " 255 int combinations account for " + totalUnder255 + " out of " + totalCount/2);
			}
		}
		
		public void CopyMemory(byte[] output, out int length) {
			length = current.Length;
			for( int i=0;i<current.Length;i++) {
				output[i] = current.Bytes[i];
			}
		}
		
		public TickImpl DecompressTick( BinaryReader reader) {
			if( count < 100) {
				CompareTick(reader);
			} else {
				ReadMemory( reader);
				current.Reset();
				ReverseDifference(current.Bytes,previous.Bytes,previous.Length);
				CompareMemory(current.Bytes,previous.Bytes,previous.Length);
				CompareTick(current.Reader);
				if( shortValueCompression) {
					// Frequency of individual short values
					for( int i=0; i<previous.Length; i++) {
						int intValue = (int) (current.Bytes[i] | (current.Bytes[i+1] << 8));
						ByteCount bc = shortValueCounts[intValue];
						shortValueCounts[intValue] = new ByteCount(intValue,bc.Count+1);
					}
				}
			}
    		SwapBuffers();
			count++; 
			if( count % 100 == 0) { CreateSignature(); ResetCounters(); }
			return currentTick;
		}
		
		TickIO tickIO = new TickImpl();
		public void CompressTick( TickBinary tick, MemoryStream memory) {
			tickIO.Inject(tick);
			byte[] output = memory.GetBuffer();
			int length = (int) memory.Length;
			CompareTick(tickIO);
			if( count < 100) {
				CopyMemory(output,out length);
			} else {
				CalculateDifference(current.Bytes,previous.Bytes,current.Length);
				WriteMemory(output,out length);

				if( shortValueCompression) {
					// Frequency of individual short values
					for( int i=0; i<length-1; i++) {
						int intValue = (int) (output[i] | (output[i+1] << 8));
						ByteCount bc = shortValueCounts[intValue];
						shortValueCounts[intValue] = new ByteCount(intValue,bc.Count+1);
					}
				}
			}
    		SwapBuffers();
			count++; 
			if( count % 100 == 0) { CreateSignature(); ResetCounters(); }
		}
		
		byte[] difference = new byte[1024];
		
		public byte[] Difference {
			get { return difference; }
		}
		int diffLength = 0;
		
		public int DiffLength {
			get { return diffLength; }
		}
		BitBuffer diffBits = new BitBuffer(1024);
		public void CalculateDifference(byte[] current, byte[] previous,long length) {
			// Reset
			diffBits.Clear();
			int index;
			diffLength = 0;
			int count = 0;
			
			// Calculate differences
			for( int i=0;i<signature.Length;i++) {
				index = signature[i];
				if( current[index] != previous[index]) {
					diffBits.Add(true);
					difference[diffLength] = current[index];
					diffLength++;
					count = i;
				} else {
					diffBits.Add(false);
				}
			}
			// Reduce diff bit count to eliminate final matching bytes.
			diffBits.BitCount = count+1;
		}
		
		public void ReverseDifference(byte[] current, byte[] previous, long length) {
			int index;
			int count=0;
			
			log.Debug("diff bits = " + diffBits.DebugBytes);
			
			// Calculate differences
			for( int i=0;i<signature.Length;i++) {
				index = signature[i];
				if( i < diffBits.BitCount) {
					if( diffBits[i]) {
						current[index] = difference[count];
						count++;
					} else {
					    current[index] = previous[index];
					}
				} else {
					current[index] = previous[index];
				}
			}
		}
		
		private void ReadMemory( BinaryReader reader) {

			// Get length
			diffLength = reader.ReadByte();
			
			// Clear diffBits
			diffBits.Clear();
			
			// Calculate diffBits byte length
			int byteCount = (diffLength>>3) + ((diffLength&7) != 0 ? 1 : 0);
			for( int i=0; i<byteCount; i++) {
				diffBits.Add(reader.ReadByte());
			}
			
			for( int i=0; i<diffLength; i++) {
				difference[i] = reader.ReadByte();
			}
		}
		
		public void WriteMemory( byte[] output, out int length) {
			// Create compressed buffer
			length = 0;
			
			output[length] = (byte) diffLength;
			length++;
			
			// diff bits
			int bitCount = diffBits.BitCount;
			int byteCount = diffBits.ByteCount;
			for( int i=0; i<byteCount; i++) {
				output[length] = diffBits.Bytes[i];
				length++;
			}
			
			for( int i=0; i<diffLength; i++) {
				output[length] = difference[i];
				length++;
			}
		}
		
		public int[] Signature {
			get { return signature; }
		}
		
		public int Count {
			get { return count; }
			set { count = value; }
		}
		
		public ByteMemory Current {
			get { return current; }
		}
		
		public ByteMemory Previous {
			get { return previous; }
		}
	}
}
