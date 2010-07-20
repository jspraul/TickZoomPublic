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
using System.Configuration;
using System.IO;

using Microsoft.Win32;
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.TZData;

namespace TickZoom.Utilities
{
	[TestFixture]
	public class tzdataTest
	{
		[Test]
		public void TestFilter()
		{
	       	string storageFolder = Factory.Settings["AppDataFolder"];
	       	if( storageFolder == null) {
	       		throw new ApplicationException( "Must set AppDataFolder property in app.config");
	       	}
			string[] args = {
				storageFolder + @"\TestData\Daily4Ticks.tck",
				storageFolder + @"\TestData\Daily4Sim.tck",
			};
			Filter filter = new Filter(args);
		}
		
		[Test]
		public void TestMigrate()
		{
	       	string storageFolder = Factory.Settings["AppDataFolder"];
	       	if( storageFolder == null) {
	       		throw new ApplicationException( "Must set AppDataFolder property in app.config");
	       	}
	       	string origFile = storageFolder + @"\TestData\Migrate.tck";
	       	string tempFile = origFile + ".temp";
	       	string backupFile = origFile + ".back";
	       	File.Delete( backupFile);
	       	File.Delete( origFile);
	       	string fileName = storageFolder + @"\TestData\USD_JPY.tck";
	       	if( !File.Exists(fileName)) {
	       		fileName = fileName.Replace(".tck","_Tick.tck");
	       	}
	       	File.Copy(fileName, origFile);
	       	
	       	string[] args = { "USD/JPY", storageFolder + @"\TestData\Migrate.tck" };
	       	
	       	Migrate migrate = new Migrate(args);
			Assert.IsTrue( File.Exists( origFile));
			Assert.IsTrue( File.Exists( backupFile));
			Assert.IsFalse( File.Exists( tempFile));
		}
		
		[Test]
		public void TestQuery()
		{
			string[] args = { @"C:\TickZoom\TestData\ESH0.tck" };
			Query query = new Query(args);
			string expectedOutput = @"Symbol: /ESH0
Ticks: 15683
Trade Only: 15683
From: 2010-02-16 16:49:28.769
To: 2010-02-16 16:59:56.140
Prices duplicates: 14489
";
			string output = query.ToString();
			Assert.AreEqual(expectedOutput,output);			
		}
		
		[Test]
		public void TestRegister()
		{
	       	Register register = new Register(null);

	       	string expectedExtension = ".tck";
	       	string expectedProgId = "TZData";
	       	string expectedDescription = "TickZoom Tick Data";
	       	string expectedExecutable = "tzdata.exe\" \"open\" \"%1\"";
	        using (RegistryKey extKey = Registry.ClassesRoot.OpenSubKey(expectedExtension, true))
	        {
	        	Assert.AreEqual(expectedProgId,extKey.GetValue(string.Empty));
	        }
			
	        using (RegistryKey progIdKey = Registry.ClassesRoot.OpenSubKey(expectedProgId,true))
	        {
	        	object description = progIdKey.GetValue(string.Empty);
	        	Assert.AreEqual(expectedDescription,description);
				
	            using (RegistryKey command = progIdKey.OpenSubKey("shell\\open\\command",true))
	            {
	            	string executable = (string) command.GetValue(string.Empty);
	            	Assert.IsTrue(executable.Contains(expectedExecutable),executable + " contains " + expectedExecutable);
	            }
	        }
		}
	}
}
