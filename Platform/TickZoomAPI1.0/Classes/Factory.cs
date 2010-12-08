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
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace TickZoom.Api
{
	/// <summary>
	/// Description of Factory.
	/// </summary>
	public static class Factory
	{
		private static EngineFactory engineFactory;
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(Factory));
		private static object locker;
		private static LogManager logManager;
		private static LogManager sysLogManager;
		private static TickUtilFactory tickUtilFactory;		
		private static Parallel parallel;		
		private static ProviderFactory provider;
		private static SymbolFactory symbolFactory;
		private static StarterFactory starterFactory;
		private static UtilityFactory utilityFactory;
		private static FactoryLoader factoryLoader;
		
		private static object Locker {
			get {
				if( locker == null) {
					locker = new object();
				}
				return locker;
			}
		}
	
		public static FactoryLoader FactoryLoader {
			get {
				if( factoryLoader == null) {
					lock(Locker) {
						if( factoryLoader == null) {
							factoryLoader = new BootStrap().FactoryLoader();
						}
					}
				}
				return factoryLoader;
			}
		}
		
		[CLSCompliant(false)]
		public static EngineFactory Engine {
			get { 
				if( engineFactory == null) {
					lock(Locker) {
						if( engineFactory == null) {
							string profilerFlag = Factory.Settings["TickZoomProfiler"];
				       		string engineName;
				       		if( "true".Equals(profilerFlag)) {
				       			engineName = "TickZoomProfiler";
				       			try { 
									engineFactory = (EngineFactory) FactoryLoader.Load( typeof(EngineFactory), engineName);
				       			} catch( Exception ex) {
				       				log.Notice( "ERROR: In the config file, TickZoomProfiler is set to \"true\"");
				       				log.Notice( "However, an error occurred while loading TickZoomProfiler engine.");
				       				log.Notice( "Please check the TickZoom.log for further detail.");
				       				log.Debug( "TickZoomProfiler load ERROR: " + ex);
				       				throw;
				       			}
				       		}
				       		
				       		if( engineFactory == null) {
				       			engineName = "TickZoomEngine";
								engineFactory = (EngineFactory) FactoryLoader.Load( typeof(EngineFactory), engineName);
				       		}
						}
					}
				}
				return engineFactory;
			}
		}
		
		public static LogManager SysLog {
			get { 
				if( sysLogManager == null) {
					lock(Locker) {
						if( sysLogManager == null) {
							sysLogManager = (LogManager) FactoryLoader.Load(typeof(LogManager), "TickZoomLogging" );
							sysLogManager.Configure("SysLog");
						}
						
					}
				}
				return sysLogManager;
			}
		}
		
		public static LogManager Log {
			get { 
				if( logManager == null) {
					lock(Locker) {
						if( logManager == null) {
							logManager = (LogManager) FactoryLoader.Load(typeof(LogManager), "TickZoomLogging" );
							logManager.Configure("Log");
						}
						
					}
				}
				return logManager;
			}
		}
		
		public static Parallel Parallel {
			get {
				if( parallel == null) {
					lock(Locker) {
						if( parallel == null) {
							parallel = Engine.Parallel();
						}
					}
				}
				return parallel;
			}
		}
		
		private static long frequency = Stopwatch.Frequency / 1000;
		
		private static long Frequency {
			get {
				if( frequency == 0L) {
					long freq = Stopwatch.Frequency;
					freq /= 1000;
					Interlocked.Exchange(ref frequency,  freq);
				}
				return frequency;
			}
		}
		
		public static long TickCount {
			get {
				return Parallel.TickCount;
			}
		}
		
		public static SymbolFactory Symbol {
			get {
				if( symbolFactory == null) {
					lock(Locker) {
						if( symbolFactory == null) {
							symbolFactory = (SymbolFactory) FactoryLoader.Load( typeof(SymbolFactory), "TickZoomStarters" );
						}
					}
				}
				return symbolFactory;
			}
		}
		
		public static StarterFactory Starter {
			get {
				if( starterFactory == null) {
					lock(Locker) {
						if( starterFactory == null) {
							starterFactory = (StarterFactory) FactoryLoader.Load( typeof(StarterFactory), "TickZoomStarters" );
						}
					}
				}
				return starterFactory;
			}
		}
		
		[CLSCompliant(false)]
		public static UtilityFactory Utility {
			get {
				if( utilityFactory == null) {
					lock(Locker) {
						if( utilityFactory == null) {
							utilityFactory = (UtilityFactory) FactoryLoader.Load( typeof(UtilityFactory), "TickZoomPluginCommon" );
						}
					}
				}
				return utilityFactory;
			}
		}
		
		[CLSCompliant(false)]
		public static ProviderFactory Provider {
			get {
				if( provider == null) {
					lock(Locker) {
						if( provider == null) {
							provider = (ProviderFactory) FactoryLoader.Load( typeof(ProviderFactory), "ProviderCommon" );
						}
					}
				}
				return provider;
			}
		}

		[CLSCompliant(false)]
		public static TickUtilFactory TickUtil {
			get {
				if( tickUtilFactory == null) {
					lock(Locker) {
						if( tickUtilFactory == null) {
							tickUtilFactory = (TickUtilFactory) FactoryLoader.Load( typeof(TickUtilFactory), "TickZoomTickUtil" );
						}
					}
				}
				return tickUtilFactory;
			}
		}

	    public static bool AutoUpdate(BackgroundWorker bw) {
	    	return FactoryLoader.AutoUpdate(bw);
   		}
		
		public static Settings Settings {
			get {
				return new Settings();
			}
		}
	}
	

}
