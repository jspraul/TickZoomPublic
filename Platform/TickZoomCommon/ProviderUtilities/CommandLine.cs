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

using TickZoom.Api;
using TickZoom.Native;

namespace TickZoom.Common
{
	public class CommandLineProcess : ProviderService
	{
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(CommandLineProcess));
		ServiceConnection connection;
		ServiceInstaller serviceInstaller;
		string assemblyName = Assembly.GetEntryAssembly().GetName().Name;
		
		public CommandLineProcess()
		{
		}
		
		/// <summary>
		/// Run this service.
		/// </summary>
		public void Run(string[] args)
		{
			var arguments = new Arguments( args);
			string value;
			if( arguments.TryGetValue( "help", out value) ||
			    arguments.TryGetValue( "h", out value) ||
			    arguments.TryGetValue( "?", out value)) {
	        	Help();
	        	return;
			}
			if( arguments.TryGetValue( "config", out value)) {
				connection.SetConfig( value);
			}
			var startOptions = 0;
			var startOption = "";
			string portString;
			if( arguments.TryGetValue( "port", out portString)) {
				startOptions++;
				startOption = "port";
			}
			if( arguments.TryGetValue( "run", out value)) {
				startOptions++;
				startOption = "run";
			}
			if( arguments.TryGetValue( "start", out value)) {
				startOptions++;
				startOption = "start";
			}
			if( arguments.TryGetValue( "stop", out value)) {
				startOptions++;
				startOption = "stop";
			}
			if( startOptions != 1) {
	        	Console.WriteLine("Too many options selected. Try --help.");
	        	return;
			}
			switch( startOption) {
				case "run":
	        		connection.OnRun();
	        		break;
				case "start":
		        	serviceInstaller = new ServiceInstaller(assemblyName);
		        	string codeBase = Assembly.GetEntryAssembly().Location;
		        	string fullPath = Path.GetFullPath( codeBase );
		        	serviceInstaller.InstallAndStart(assemblyName,fullPath);
		        	break;
				case "stop":
		        	serviceInstaller = new ServiceInstaller(assemblyName);
		        	serviceInstaller.StopAndUninstall();
		        	break;
		        case "port":
					ushort port = 0;
					if( !ushort.TryParse(portString, out port)) {
			        	Console.WriteLine("Please use a valid port number instead of '" + portString + "' for the --port argument. Try --help.");
			        	return;
					}
		        	try {
		        		connection.SetAddress("127.0.0.1",port);
		        		connection.OnRun();
		        	} catch( FormatException) {
			        	Console.WriteLine("Unknown command line argument. Try --help.");
		        	}
					break;
			}
		}
		
		private void Help() {
			Console.WriteLine("Usage: " + assemblyName + " [--start | --stop | --run | {port}] [{configuration}]");
			Console.WriteLine();
			Console.WriteLine("{port}: Run from command line using the port given on local host.");
			Console.WriteLine("--run : Create as a Service Process so it will auto start at boot up.");
			Console.WriteLine("--start : Create as a Service Process so it will auto start at boot up.");
			Console.WriteLine("--stop : Remove the Service Process.");
			Console.WriteLine();
			Console.WriteLine("config : name of a configuration file in the Config folder.");
			Console.WriteLine();
			Console.WriteLine("On Windows, --start both installs and starts the process unlesss already installed or started.");
			Console.WriteLine("And --stop stops and uninstalls unlesss already stopped or uninstalled.");
			Console.WriteLine();
			Console.WriteLine("The operating system starts " + assemblyName + " with zero arguments when installed");
			Console.WriteLine("as a service process. Running it without arguments from the command lines gives an error.");
		}
		
		public ServiceConnection Connection {
			get { return connection; }
			set { connection = value; }
		}
	}
}
