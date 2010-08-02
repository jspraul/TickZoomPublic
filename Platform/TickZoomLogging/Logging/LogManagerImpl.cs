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
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

using log4net.Core;
using log4net.Filter;
using log4net.Repository;
using TickZoom.Api;

namespace TickZoom.Logging
{
	public class LogManagerImpl : LogManager {
		private static Log exceptionLog;
		private static object locker = new object();
		private ILoggerRepository repository;
		Dictionary<string,LogImpl> map = new Dictionary<string,LogImpl>();
		public void Configure(string repositoryName) {
			repository = LoggerManager.CreateRepository(repositoryName);
			if( repositoryName == "TickZoom" ) {
				log4net.Config.XmlConfigurator.Configure(repository);
			} else {
				string storageFolder = Factory.Settings["AppDataFolder"];
				string configPath = Path.Combine(storageFolder,"Config");
				Directory.CreateDirectory(configPath);
				string configFile = Path.Combine(configPath,repositoryName+".Log.config");
				if( !File.Exists(configFile)) {
					StringBuilder sb = new StringBuilder();
					sb.AppendLine("Cannot find logging configuration file: " + configFile);
					sb.AppendLine("Please create the file and put your custom log4net configuration in it.");
					throw new ApplicationException(sb.ToString());
				}
				log4net.Config.XmlConfigurator.Configure(repository,new FileInfo(configFile));
			}
			lock( locker) {
				if( exceptionLog == null) {
					exceptionLog = GetLogger("TickZoom.AppDomain");
				    AppDomain.CurrentDomain.UnhandledException +=
				        new UnhandledExceptionEventHandler(UnhandledException);
				}
			}
		}
		
		private static void UnhandledException(object sender, UnhandledExceptionEventArgs e) {
		    Exception ex = (Exception)e.ExceptionObject;
		    exceptionLog.Error("Unhandled exception caught",ex);
		}
	
		public string LogFolder {
			get {
                // get the log directory
                string logDirectory = Factory.Settings["AppDataFolder"];
				string uniqueFolder = Environment.CurrentDirectory;
				uniqueFolder = uniqueFolder.Replace(Path.DirectorySeparatorChar,'-');
				uniqueFolder = uniqueFolder.Replace(":","");
				logDirectory = Path.Combine( logDirectory, "Logs");
				logDirectory = Path.Combine( logDirectory, uniqueFolder);
				return logDirectory;
			}
		}
		public Log GetLogger(Type type) {
			LogImpl log;
			if( map.TryGetValue(type.FullName, out log)) {
				return log;
			} else {
				ILogger logger = repository.GetLogger(type.FullName);
				log = new LogImpl(logger);
				map[type.FullName] = log;
			}
			return log;
		}
		public Log GetLogger(string name) {
			LogImpl log;
			if( map.TryGetValue(name, out log)) {
				return log;
			} else {
				ILogger logger = repository.GetLogger(name);
				log = new LogImpl(logger);
				map[name] = log;
			}
			return log;
		}
	}
}
