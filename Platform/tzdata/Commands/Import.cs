using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

using TickZoom.Api;

namespace TickZoom.TZData
{
	/// <summary>
	/// Convert tick data exported from TradeStation into TickZoom format.
	/// </summary>
	public class Import : Command
	{
		string assemblyName;

		// Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		TimeStamp tickTime;
		TickWriter tickWriter;
		String[] tsBits;			// the price line parsed
		SymbolInfo symbol;
		TickIO tickIO = Factory.TickUtil.TickIO();
		double countTicksOut = 0;
		bool fromMinute = true;			
		string fromFile;

		TimeStamp startTime = new TimeStamp(2009,2,23,9,0,0);
		TimeStamp endTime = new TimeStamp(2009,2,23,10,0,0);
		TimeStamp utcTime = new TimeStamp();
		SymbolTimeZone timeZone;
		
		int prevTSMin = 333;		// ensure initialization
		int newSec;
		int newMilli;
		
		public Import() {
			Assembly assembly = Assembly.GetEntryAssembly();
			if( assembly != null) {
				assemblyName = assembly.GetName().Name;
			}
		}

		public void Run(string[] args)
		{
			if( args.Length != 2 && args.Length != 4) {
				Console.Write("Import Usage:");
				Console.Write("tzdata " + Usage()); 
				return;
			}
			SymbolInfo symbol;
			string symbolString = args[0];
			fromFile = args[1];
			TimeStamp startTime;
			TimeStamp endTime;
			if( args.Length > 2) {
				startTime = new TimeStamp(args[2]);
				endTime = new TimeStamp(args[3]);
			} else {
				startTime = TimeStamp.MinValue;
				endTime = TimeStamp.MaxValue;
			}

        	double aTick; 									// *** specify the minimum price fluctuation here
			
        	symbol = Factory.Symbol.LookupSymbol(symbolString);
        	aTick  = symbol.MinimumTick;

        	
			// Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			
			TSConverter(symbol);
			// TSConverter(symbol,from,to,startTime,endTime,fromMinute);
			// TZConverter(args[1],args[0]);
		}
	
		public void TSConverter(SymbolInfo symbolParam)
//		public void TSConverter(SymbolInfo symbolParam, string from, string to, TimeStamp startTime, TimeStamp endTime, bool fromMinute)
		{

		// this.symbol = symbolInfo;
		
		symbol = symbolParam;
		

		string path = Factory.Settings["AppDataFolder"] + @"\DataCache\";		// *** specify the path to the input file here
		// SymbolInfo symbol;									// take user input symbol
		string ext = ".txt";							// *** specify input file extension here
		
//		double utcOffset = TimeStamp.UtcOffset;
		String tsLine;				// the raw price data line

		double tickOpen;
		double tickHigh;
		double tickLow;
		double tickClose;
		int    tickVolume;
		
		ArrayList ticksOut = new ArrayList();
//		int ticksOutCount = 0;
//		bool fromMinute;

		char[] delimiterChars = { ',' };

		//	Do all the work here.
		//	fromMinute = isFromMinute;
			int countLines = 0;
			
			try 
	        {
	            using (StreamReader sr = new StreamReader(fromFile))
	            {
					tickIO.Initialize();
					tickIO.SetSymbol(symbol.BinaryIdentifier);
	                while ((tsLine = sr.ReadLine()) != null) 
	                {
	                	countLines++;
	                	
	                	if (countLines > 1) {
	                		tsBits = tsLine.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                		
		                	if (fromMinute) {

	                			tickOpen = double.Parse(tsBits[2]);
								tickHigh = double.Parse(tsBits[3]);
								tickLow = double.Parse(tsBits[4]);
								tickClose = double.Parse(tsBits[5]);
								tickVolume = int.Parse(tsBits[6]);

								if ( tickOpen == tickHigh && tickOpen == tickLow && tickOpen == tickClose ){
									writeATick(tickOpen, tickVolume);
								} else {
									writeATick(tickOpen, 0);                            
									if ( tickHigh - tickClose >= tickClose - tickOpen ){
    	                            	writeATick(tickHigh, 0);
        	                        	writeATick(tickLow, 0);
            	                    	writeATick(tickHigh, 0);									
									} else {
                    	            	writeATick(tickLow, 0);
                        	        	writeATick(tickHigh, 0);
                            	    	writeATick(tickLow, 0);									
									}
									writeATick(tickClose, tickVolume);
								}
		                	}
	                	}
	                }
	            }
	        }
	        catch (Exception ex) 
	        {
	        	throw new ApplicationException("Error on line " + countLines + ": " + ex.Message, ex);
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

		public void TZConverter(string file, string symbol){
			
			TickReader reader = Factory.TickUtil.TickReader();
			reader.Initialize( file, symbol);
			TickQueue queue = reader.ReadQueue;

			TickBinary tickBinary = new TickBinary();
				
			if( !TryGetNextTick(queue, ref tickBinary)) {
			
			}
			

		}
		
		private void writeATick(double insertPrice, int volume) {
			if ( tsBits[1].Length <= 3 ){		    
        		tickTime = new TimeStamp( tsBits[0] + " " + tsBits[1] + ":00");
			} else {
        		tickTime = new TimeStamp( tsBits[0] + " " + tsBits[1] );				
			}
        	utcTime = tickTime;
			utcTime.AddSeconds( - GetUtcOffset(tickTime));
			double price = insertPrice;
			
			tickIO.SetTime(utcTime);
			if ( volume >= 1 ) {
				tickIO.SetTrade(price, volume);
			} 
			else {
//				tickIO.SetQuote(price, price);
				tickIO.SetTrade(price, volume);
			}

			if( tickWriter == null) {
				tickWriter = Factory.TickUtil.TickWriter(true);
	 			tickWriter.KeepFileOpen = true;
				string folder = "DataCache";
				tickWriter.Initialize(folder, symbol.Symbol);
			}
			
			// Console.WriteLine(tickIO);
			
			tickWriter.Add(tickIO);
			
			countTicksOut++;
		}

		TimeStamp nextOffsetUpdate;
		long utcOffset;
		// This method is for performance. It only update the UTC offset on weekend boundaries.
		private long GetUtcOffset( TimeStamp tickTime) {
			if( tickTime.Internal >= nextOffsetUpdate.Internal) {
				if( timeZone == null) {
					timeZone = new SymbolTimeZone(symbol);
					timeZone.SetExchangeTimeZone();
				}
				utcOffset = timeZone.UtcOffset(tickTime);
				nextOffsetUpdate = utcTime;
				int dayOfWeek = nextOffsetUpdate.GetDayOfWeek();
				nextOffsetUpdate.AddDays( 7 - dayOfWeek);
				nextOffsetUpdate.SetDate(nextOffsetUpdate.Year,nextOffsetUpdate.Month,nextOffsetUpdate.Day);
			}
			return utcOffset;
		}
		private TimeStamp getTimeStamp(String date, String time) {
			int tsMonth = int.Parse(date.Substring(0, 2));
			int tsDay = int.Parse(date.Substring(3, 2));
			int tsYear = int.Parse(date.Substring(6, 4));
			int tsHour = int.Parse(time.Substring(0, 2));
			int tsMin = int.Parse(time.Substring(3, 2));

			// create new pieces needed for TimeStamp
			if (tsMin != prevTSMin) {
//	            Console.WriteLine("tsMin != prevTSMin");
				if (fromMinute) {
					newSec = 0;
					newMilli = -59999;		// in previous minute to keep times in synch
				} else {
					// keep existing time when loading ticks
					newSec = 0;
					newMilli = 1;
				}
			} else {
				newMilli++;
				if ((fromMinute && newMilli > -1) || (!fromMinute && newMilli > 59999)) {
					throw new OverflowException();
				}
			}
			// save the previous TS minute after checking to see if it changed
			prevTSMin = tsMin;
			
			// make the new time stamp for this tick
			DateTime newDateTime = new DateTime(tsYear, tsMonth, tsDay, tsHour, tsMin, newSec, 0);
//			TimeStamp newTimeStamp = (TimeStamp)newDateTime.AddMilliseconds(newMilli);
			TimeStamp newTimeStamp = (TimeStamp)newDateTime.AddMilliseconds(0);
            
			//return newTimeStamp;
			return newTimeStamp;
		
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

		public string[] Usage() {
			return new string[] { assemblyName + " importer <symbol> <fromfile> <tofile> [<starttimestamp> <endtimestamp>]" };
		}
		
		public string AssemblyName {
			get { return assemblyName; }
			set { assemblyName = value; }
		}
		
	}
}