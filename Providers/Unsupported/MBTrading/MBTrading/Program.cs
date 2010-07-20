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
using System.ServiceProcess;
using System.Text;
using System.Threading;

using TickZoom.Api;
using TickZoom.MBTrading;

namespace MBTProvider
{
	static class Program
	{
		public static string ServiceName = "MB Trading Service";
		/// <summary>
		/// This method starts the service.
		/// </summary>
		static void Main(string[] args)
		{
			try {
				ServiceConnection connection = Factory.Provider.ConnectionManager();
				connection.OnCreateProvider = () => new MbtInterface();
				if( args.Length > 0 ) {
					// Connection port provided on command line.
					CommandLine commandLine = new CommandLine();
					commandLine.Connection = connection;
					commandLine.OnRun(args);
				} else {
					// Connection port set via ServicePort in app.config 
					WindowsService service = new WindowsService();
					service.Connection = connection;
					ServiceBase.Run(new ServiceBase[] { service });
				}
			} catch( Exception ex) {
				string exception = ex.GetType() + ": " + ex.Message + Environment.NewLine + ex.StackTrace;
				System.Diagnostics.Debug.WriteLine( exception);
				Console.WriteLine( exception);
				Environment.Exit(1);
			}
		}
	}
}
