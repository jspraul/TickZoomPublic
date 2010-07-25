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
using System.IO;
using System.Reflection;
using System.Threading;

using TickZoom.Api;

namespace TickZoom.TZData
{
	public class Filter : Command
	{
		string assemblyName;
		public Filter() {
			Assembly assembly = Assembly.GetEntryAssembly();
			if( assembly != null) {
				assemblyName = assembly.GetName().Name;
			}
		}
		
		public void Run(string[] args)
		{
			if( args.Length != 3 && args.Length != 5) {
				Console.Write("Filter Usage:");
				Console.Write("tzdata " + Usage()); 
				return;
			}
			string symbol = args[0];
			string from = args[1];
			string to = args[2];
			TimeStamp startTime;
			TimeStamp endTime;
			if( args.Length > 3) {
				startTime = new TimeStamp(args[3]);
				endTime = new TimeStamp(args[4]);
			} else {
				startTime = TimeStamp.MinValue;
				endTime = TimeStamp.MaxValue;
			}
			FilterFile(symbol,from,to,startTime,endTime);
		}
		
		private void FilterFile(string symbol, string inputPath, string outputPath, TimeStamp startTime, TimeStamp endTime) {
			TickReader reader = Factory.TickUtil.TickReader();
			TickWriter writer = Factory.TickUtil.TickWriter(true);
			writer.KeepFileOpen = true;
			writer.Initialize( outputPath, symbol);
			reader.Initialize( inputPath, symbol);
			TickQueue inputQueue = reader.ReadQueue;
			TickIO firstTick = Factory.TickUtil.TickIO();
			TickIO lastTick = Factory.TickUtil.TickIO();
			TickIO prevTick = Factory.TickUtil.TickIO();
			long count = 0;
			long fast = 0;
			long dups = 0;
			TickIO tickIO = Factory.TickUtil.TickIO();
			TickBinary tickBinary = new TickBinary();
			inputQueue.Dequeue(ref tickBinary);
			tickIO.Inject(tickBinary);
			count++;
			firstTick.Copy(tickIO);
			firstTick.IsSimulateTicks = true;
			prevTick.Copy(tickIO);
			prevTick.IsSimulateTicks = true;
			if( tickIO.Time >= startTime) {
				writer.Add(firstTick);
			}
			try {
				while(true) {
					while( !inputQueue.TryDequeue(ref tickBinary)) {
						Thread.Sleep(1);
					}
					tickIO.Inject(tickBinary);
					
					count++;
					if( tickIO.Time >= startTime) {
						if( tickIO.Time > endTime) break;
//						if( tickIO.Bid == prevTick.Bid && tickIO.Ask == prevTick.Ask) {
//							dups++;
//						} else {
//							Elapsed elapsed = tickIO.Time - prevTick.Time;
							prevTick.Copy(tickIO);
							prevTick.IsSimulateTicks = true;
//							if( elapsed.TotalMilliseconds < 5000) {
//								fast++;
//							} else {
							while( !writer.TryAdd(prevTick)) {
								Thread.Sleep(1);
							}
//							}	
//						}
					}
				}
			} catch( QueueException ex) {
				if( ex.EntryType != EventType.EndHistorical) {
					throw new ApplicationException("Unexpected QueueException: " + ex);
				}
			}
			lastTick.Copy( tickIO);
			Console.WriteLine(reader.Symbol + ": " + count + " ticks from " + firstTick.Time + " to " + lastTick.Time + " " + dups + " duplicates, " + fast + " less than 50 ms");
			Factory.TickUtil.TickReader().CloseAll();
			writer.Close();
		}

		public string[] Usage() {
			return new string[] { assemblyName + " filter <symbol> <fromfile> <tofile> [<starttimestamp> <endtimestamp>]" };
		}
		
		public string AssemblyName {
			get { return assemblyName; }
			set { assemblyName = value; }
		}
	}
}
