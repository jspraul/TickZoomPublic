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
		public static Dictionary<string,Command> commands = new Dictionary<string,Command>();
		
		public static void Main(string[] args)
		{
			commands["migrate"] = new Migrate();
			commands["filter"] = new Filter();
			commands["query"] = new Query();
			commands["register"] = new Register();
			commands["open"] = new Open();
			commands["import"] = new Import();
			
			if( args.Length == 0) {
				Console.WriteLine("tzdata Usage:");
				Console.WriteLine();
				foreach( var kvp in commands) {
					Command command = kvp.Value;
					foreach( var line in command.Usage()) {
						Console.WriteLine("  " + line);
					}
				}
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

			commands[args[0]].Run(taskArgs.ToArray());
		}
	}
}