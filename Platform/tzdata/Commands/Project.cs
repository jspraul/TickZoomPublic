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
using System.Threading;

using TickZoom.Api;

namespace TickZoom.TZData
{
	public class Project : Command
	{
		public void Run(string[] args)
		{
			if( args.Length != 1) {
				Console.Write("Solution Usage:");
				Console.Write("tzdata " + Usage());
				return;
			}
			GenerateProject(args[0]);
		}
		
		public string[] Usage() {
			List<string> lines = new List<string>();
			string name = Assembly.GetEntryAssembly().GetName().Name;
			lines.Add( name + " project <path>");
			return lines.ToArray();
		}
		
		private void GenerateProject(string path) {
			if( !Directory.Exists(path)) {
				Console.WriteLine("Please provide a value path to a project folder.");
				return;
			}
			var files = new List<string>();
//			files.AddRange( Directory.GetFiles(path,"*.cs",SearchOption.AllDirectories));
			
			foreach( var filePath in files) {
				var fullRelative = MakeRelativePath( Environment.CurrentDirectory, filePath).Replace('/',Path.DirectorySeparatorChar);
				var partialRelative = MakeRelativePath( path, filePath).Replace('/',Path.DirectorySeparatorChar);
				fullRelative = @"..\" + fullRelative;
				Console.WriteLine("    <Compile Include=\"" + fullRelative + "\">");
				Console.WriteLine("      <Link>" + partialRelative + "</Link>");
				Console.WriteLine("    </Compile>");
			}
			files.Clear();
			files.AddRange( Directory.GetFiles(path,"*.resx",SearchOption.AllDirectories));
			foreach( var filePath in files) {
				var fullRelative = MakeRelativePath( Environment.CurrentDirectory, filePath).Replace('/',Path.DirectorySeparatorChar);
				var partialRelative = MakeRelativePath( path, filePath).Replace('/',Path.DirectorySeparatorChar);
				fullRelative = @"..\" + fullRelative;
				Console.WriteLine("    <EmbeddedResource Include=\"" + fullRelative + "\">");
				Console.WriteLine("      <Link>" + partialRelative + "</Link>");
				Console.WriteLine("    </EmbeddedResource>");
			}
		}
		
	    public static String MakeRelativePath(String fromPath, String toPath)
	    {
	        if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
	        if (String.IsNullOrEmpty(toPath))   throw new ArgumentNullException("toPath");
	
	        Uri fromUri = new Uri(fromPath);
	        Uri toUri = new Uri(toPath);
	        
	        Uri relativeUri = fromUri.MakeRelativeUri(toUri);
	
	        return relativeUri.ToString();
	
	    }
	}
	
}
