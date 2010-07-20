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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;

using TickZoom.Api;

namespace TickZoom.Common
{
	/// <summary>
	/// Convert tick data exported from TradeStation into TickZoom format.
	/// </summary>
	public class TSTickConverter
	{
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public TSTickConverter(SymbolInfo symbolInfo)
		{
			this.symbol = symbolInfo;
		}
		
		TickWriter tickWriter;
		TickIO tickIO = Factory.TickUtil.TickIO();

		string path = Factory.Settings["AppDataFolder"] + @"\DataCache\";		// *** specify the path to the input file here
		SymbolInfo symbol;									// take user input symbol
		string ext = ".txt";							// *** specify input file extension here
		double aTick = 0.25;							// *** specify the minimum price fluctuation here
				
//		double utcOffset = TimeStamp.UtcOffset;
		String tsLine;				// the raw price data line
		String[] tsBits;			// the price line parsed
		double tickOpen;
		double tickHigh;
		double tickLow;
		double tickClose;
		double prevPrice;
		TimeStamp tickTime;
		double countTicksOut = 0;
		ArrayList ticksOut = new ArrayList();
//		int ticksOutCount = 0;
		bool fromMinute;

		char[] delimiterChars = { ',' };
		public void Convert(bool isFromMinute) {
			// Do all the work here.
			fromMinute = isFromMinute;
			int countLines = 0;
			
			try 
	        {
	            // Create an instance of StreamReader to read from the file.
	            // The using statement also closes the StreamReader.
	            using (StreamReader sr = new StreamReader(path + symbol + ext))
	            {
	                // Read and process lines from the file until the end of 
	                // the file is reached.
	                while ((tsLine = sr.ReadLine()) != null) 
	                {
	                	countLines++;
	                	
	                	if (countLines > 1) {
	                		// the first line is a header, skip it
	                		tsBits = tsLine.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
							
		                	if (fromMinute) {
								// Each tsLine is a 1 minute bar, create 4 ticks from each line,
								// preserving the time of the bar for the close and backing up
								// the times of the other synthesized 'ticks'.
								// If there is a 'gap' between consecutive tick prices then add ticks at
								// each intermediate missing price.  This gives a smooth progression through
								// the price bar, eliminating potentially large slippage on big bars.
								// Put each price in the file twice so that signal order processing will
								// fill at the same price as the order.

								// get common fields
								tickOpen = double.Parse(tsBits[2]);
								tickHigh = double.Parse(tsBits[3]);
								tickLow = double.Parse(tsBits[4]);
								tickClose = double.Parse(tsBits[5]);

								// the open always comes first, there are no ticks between previous close 
								// and next open
								writeATick(tickOpen);
								//writeATick(tickOpen);
								
								// next comes the price further from the close (high or low)
								if (tickHigh - tickClose >= tickClose - tickLow) {
									// high is further away, save high then low
									writeTicks(tickOpen, tickHigh);
									writeTicks(tickHigh, tickLow);
									prevPrice = tickLow;
								} else {
									// low is further away, save low then high
									writeTicks(tickOpen, tickLow);
									writeTicks(tickLow, tickHigh);
									prevPrice = tickHigh;
								}
								
								// last is the close
								writeTicks(prevPrice, tickClose);
								
		                	} else {
								// each tsLine is a separate tick, written independently
								double tickPrice = double.Parse(tsBits[2]);

								// get a timestamp and write out the tick
								writeATick(tickPrice);
		                	}
	                	}
	                }
	            }
	        }
	        catch (Exception e) 
	        {
	            // Let the user know what went wrong.
	            log.Notice("The file could not be read: " + e.Message);
	            log.Debug(e.ToString());
	            return;
	        }
	        finally {
	        	if( tickWriter != null) {
					tickWriter.Close();
	        	}
	        }
			
			using (StreamWriter sw = new StreamWriter(path + symbol + "_log" + ext)) {
				for (int i = 0; i < ticksOut.Count; i++) {
					sw.WriteLine(ticksOut[i]);
				}
			}
		}

		private void writeTicks(double prevPrice, double nextPrice) {
			// Write out ticks that are greater than prevPrice and less than or
			// equal to nextPrice.  But be sure nextPrice is written out when it
			// equal to prevPrice.

			if (prevPrice == nextPrice) {
				// this is only called when doing minute bars, write the tick twice
				writeATick(nextPrice);
				//writeATick(nextPrice);
			} else {
				// The objective is to be sure each tick between prevPrice and nextPrice is
				// included in the file.  Determine the prices of missing ticks with respect
				// to direction of price movement.

				int priceDirection;
				if (prevPrice <= nextPrice) {
					priceDirection = 1;
				} else {
					priceDirection = -1;
				}
								
				double insertPrice = prevPrice + priceDirection * aTick;
				while (priceDirection * insertPrice <= priceDirection * nextPrice) {				
					// write out the new tick twice
					writeATick(insertPrice);
					//writeATick(insertPrice);
					
					insertPrice = insertPrice + priceDirection * aTick;
				}
			}
		}
		
		TimeStamp startTime = new TimeStamp(2009,2,23,9,0,0);
		TimeStamp endTime = new TimeStamp(2009,2,23,10,0,0);
		TimeStamp utcTime = new TimeStamp();
		SymbolTimeZone timeZone;
		private void writeATick(double insertPrice) {
			tickTime = getTimeStamp(tsBits[0], tsBits[1]);
			utcTime.Assign(tickTime.Year,tickTime.Month,tickTime.Day,tickTime.Hour,tickTime.Minute,tickTime.Second,tickTime.Millisecond);
			if( timeZone == null) {
				timeZone = new SymbolTimeZone(symbol);
				timeZone.SetExchangeTimeZone();
			}
			utcTime.AddSeconds( - timeZone.UtcOffset(tickTime));
			double price = insertPrice;
			
			tickIO.Initialize();
			tickIO.SetSymbol(symbol.BinaryIdentifier);
			tickIO.SetTime(utcTime);
			tickIO.SetQuote(price, price);

			if( tickWriter == null) {
				tickWriter = Factory.TickUtil.TickWriter(true);
	 			tickWriter.KeepFileOpen = true;
				string folder = "DataCache";
				tickWriter.Initialize(folder, symbol.Symbol);
			}
			
			tickWriter.Add(tickIO);
			
			countTicksOut++;
		}
		
		int prevTSMin = 333;		// ensure initialization
		int newSec;
		int newMilli;
		private TimeStamp getTimeStamp(String date, String time) {
			// Convert a TS date and time into a TimeStamp for the tick.
			// Inputs: date from TS, in "mm/dd/yyyy" format
			//			time from TS, in "hhmm" format

			// extract pieces from the received date and time
			int tsMonth = int.Parse(date.Substring(0, 2));
			int tsDay = int.Parse(date.Substring(3, 2));
			int tsYear = int.Parse(date.Substring(6, 4));
			int tsHour = int.Parse(time.Substring(0, 2));
			int tsMin = int.Parse(time.Substring(2, 2));

			// create new pieces needed for TimeStamp
			if (tsMin != prevTSMin) {
				if (fromMinute) {
					// initialize at the start of each new minute (or higher) bar
					// *** TS does not report seconds for ticks, only minutes, so lots of ticks
					// *** can have the same time stamp.  The very first tick of a new minute is
					// *** reporting ending time as of that minute.  To keep the ticks in TZ on 
					// *** the same minute as in TS we have to use the previous minute with some
					// *** seconds and milliseconds.
					newSec = 0;
					newMilli = -59999;		// in previous minute to keep times in synch
				} else {
					// keep existing time when loading ticks
					newSec = 0;
					newMilli = 1;
				}
			} else {
				// increment previous values during the same minute
				// maximum is roughly 60k ticks in one minute
				newMilli++;
				if ((fromMinute && newMilli > -1) || (!fromMinute && newMilli > 59999)) {
					throw new OverflowException();
				}
			}
			// save the previous TS minute after checking to see if it changed
			prevTSMin = tsMin;
			
			// make the new time stamp for this tick
			DateTime newDateTime = new DateTime(tsYear, tsMonth, tsDay, tsHour, tsMin, newSec, 0);
			TimeStamp newTimeStamp = (TimeStamp)newDateTime.AddMilliseconds(newMilli);
			
			//return newTimeStamp;
			return newTimeStamp;
		}
	}
}
