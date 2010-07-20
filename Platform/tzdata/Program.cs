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

using TickZoom.Api;

namespace TickZoom.TZData
{
	class Program
	{
		public static void Main(string[] args)
		{
			if( args.Length == 0) {
				Console.Write("tzdata Usage:");
				Console.Write("tzdata migrate <symbol> <file>");
				return;
			}
			List<string> taskArgs = new List<string>(args);
			taskArgs.RemoveAt(0); // Remove the command string.
			
			// Remove debug flag which is only used by the
			// FactorySupport to determine console logging
			// for bootstrap loading of assemblies. Other 
			// debug logging gets controlled by app.config for
			// log4net.
			taskArgs.Remove("-d");
			taskArgs.Remove("--debug");

			if( args[0] == "migrate") {
				new Migrate(taskArgs.ToArray());
			}
			if( args[0] == "filter") {
				new Filter(taskArgs.ToArray());
			}
			if( args[0] == "query") {
				Console.Write(new Query(taskArgs.ToArray()));
			}
			if( args[0] == "register") {
				new Register(taskArgs.ToArray());
			}
			if( args[0] == "open") {
				new Open(taskArgs.ToArray());
			}
		}
	}
}