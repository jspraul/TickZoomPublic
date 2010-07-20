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
using TickZoom.Api;

namespace TickZoom.MBTFIX
{
	static class Program
	{
		/// <summary>
		/// This method starts the service.
		/// </summary>
		static void Main(string[] args)
		{
			try {
				ServiceConnection connection = Factory.Provider.ConnectionManager();
				connection.OnCreateProvider = () => new MBTProvider();
				if( args.Length > 0 ) {
					// Connection port provided on command line.
					ProviderService commandLine = Factory.Utility.CommandLineProcess();
					commandLine.Connection = connection;
					commandLine.Run(args);
				} else {
					// Connection port set via ServicePort in app.config 
					ProviderService service = Factory.Utility.WindowsService();
					service.Connection = connection;
					service.Run(args);
				}
			} catch( Exception ex) {
				string exception = ex.ToString();
				System.Diagnostics.Debug.WriteLine( exception);
				Console.WriteLine( exception);
				Environment.Exit(1);
			}
		}
	}
}
