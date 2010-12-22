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
using System.Reflection;
using System.Text;
using System.Threading;

using TickZoom.Api;

namespace TickZoom.TZData
{
	public class Query : Command
	{
		StringBuilder stringBuilder = new StringBuilder();
		TickReader reader = Factory.TickUtil.TickReader();
		
		public void Run(string[] args)
		{
			if( args.Length != 2 && args.Length !=1 ) {
				stringBuilder.AppendLine("Query Usage:");
				stringBuilder.AppendLine("tzdata query <symbol> <file>");
				stringBuilder.AppendLine("tzdata query <file>");
				return;
			}
			if( args.Length > 1) {
				string symbol = args[0];
				string filePath = args[1];
				reader.Initialize(filePath,symbol);
				ReadFile();
			} else {
				string filePath = args[0];
				reader.Initialize(filePath);
				ReadFile();
			}
		}
		
		public void ReadFile() {
			TickQueue queue = reader.ReadQueue;
			TickIO firstTick = Factory.TickUtil.TickIO();
			TickIO lastTick = Factory.TickUtil.TickIO();
			TickIO prevTick = Factory.TickUtil.TickIO();
			long count = 0;
			long dups = 0;
			long quotes = 0;
			long trades = 0;
			long quotesAndTrades = 0;
			TickIO tickIO = Factory.TickUtil.TickIO();
			TickBinary tickBinary = new TickBinary();
			try { 
				while(true) {
					if( !TryGetNextTick(queue, ref tickBinary)) {
						break;
					}
					tickIO.Inject(tickBinary);
					if( count == 0) {
						firstTick.Copy(tickIO);
					}
					if( tickIO.IsQuote && tickIO.IsTrade) {
						quotesAndTrades++;
					} else if( tickIO.IsQuote) {
						quotes++;
					} else {
						trades++;
					}
					if( count > 0) {
						bool quoteDup = tickIO.IsQuote && prevTick.IsQuote && tickIO.Bid == prevTick.Bid && tickIO.Ask == prevTick.Ask;
						bool tradeDup = tickIO.IsTrade && prevTick.IsTrade && tickIO.Price == prevTick.Price;
						if( tickIO.IsQuote && tickIO.IsTrade) {
							if( quoteDup && tradeDup) {
								dups++;
							}
						} else if( tickIO.IsQuote) {
							if( quoteDup) {
								dups++;
							}
						} else {
							if( tradeDup) {
								dups++;
							}
						}
					}
					count++;
					prevTick.Copy(tickIO);
				}
			} catch( QueueException) {
				// Terminated.
			}
			lastTick.Copy( tickIO);
			stringBuilder.AppendLine("Symbol: " + reader.Symbol);
			stringBuilder.AppendLine("Version: " + reader.DataVersion);
			stringBuilder.AppendLine("Ticks: " + count);
			if( quotes > 0) {
				stringBuilder.AppendLine("Quote Only: " + quotes);
			}
			if( trades > 0) {
				stringBuilder.AppendLine("Trade Only: " + trades);
			}
			if( quotesAndTrades > 0) {
				stringBuilder.AppendLine("Quote and Trade: " + quotesAndTrades);
			}
			stringBuilder.AppendLine("From: " + firstTick.Time + "." + firstTick.Time.Microsecond);
			stringBuilder.AppendLine("To: " + lastTick.Time + "." + lastTick.Time.Microsecond);
			if( dups > 0) {
				stringBuilder.AppendLine("Prices duplicates: " + dups);
			}
			Factory.TickUtil.TickReader().CloseAll();
		}
		
		private bool TryGetNextTick(TickQueue queue, ref TickBinary binary) {
			bool result = false;
			do {
				try {
					result = queue.TryDequeue(ref binary);
				} catch( QueueException ex) {
					// Ignore any other events.
					if( ex.EntryType == EventType.EndHistorical) {
						throw;
					}
				}
			} while( !result);
			return result;
		}
		
		public override string ToString()
		{
			return stringBuilder.ToString();
		}
		
		public string[] Usage() {
			List<string> lines = new List<string>();
			string name = Assembly.GetEntryAssembly().GetName().Name;
			lines.Add( name + " query <symbol> <file>");
			lines.Add( name + " query <file>");
			return lines.ToArray();
		}
	}
}
