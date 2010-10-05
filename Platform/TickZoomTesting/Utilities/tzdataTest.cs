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
				storageFolder + @"\Test\\DataCache\Daily4Ticks.tck",
				storageFolder + @"\Test\\DataCache\Daily4Sim.tck",
			};
	       	Filter filter = new Filter();
	       	filter.AssemblyName = "tzdata";
	       	filter.Run(args);
		}
		
		[Test]
		public void TestMigrate()
		{
	       	string storageFolder = Factory.Settings["AppDataFolder"];
	       	if( storageFolder == null) {
	       		throw new ApplicationException( "Must set AppDataFolder property in app.config");
	       	}
	       	string origFile = storageFolder + @"\Test\\DataCache\Migrate.tck";
	       	string tempFile = origFile + ".temp";
	       	string backupFile = origFile + ".back";
	       	File.Delete( backupFile);
	       	File.Delete( origFile);
	       	string fileName = storageFolder + @"\Test\\DataCache\USD_JPY.tck";
	       	if( !File.Exists(fileName)) {
	       		fileName = fileName.Replace(".tck","_Tick.tck");
	       	}
	       	File.Copy(fileName, origFile);
	       	
	       	string[] args = { "USD/JPY", storageFolder + @"\Test\\DataCache\Migrate.tck" };
	       	
	       	Migrate migrate = new Migrate();
	       	migrate.Run(args);
			Assert.IsTrue( File.Exists( origFile));
			Assert.IsTrue( File.Exists( backupFile));
			Assert.IsFalse( File.Exists( tempFile));
		}
		
		[Test]
		public void TestQuery()
		{
			var appData = Factory.Settings["AppDataFolder"];
			string[] args = { appData + @"\Test\\DataCache\ESH0.tck" };
			Query query = new Query();
			query.Run(args);
			string expectedOutput = @"Symbol: /ESH0
Version: 8
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
	       	Register register = new Register();
	       	register.Directory = Path.GetFullPath(".");
	       	register.Run(null);
	       	
			RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Environment",true);
			string variable = "TickZoom";
			string tickZoom = (string) key.GetValue(variable);
			string path = (string) key.GetValue("Path");
			
			Assert.NotNull(tickZoom);
			Assert.NotNull(path);
			Assert.AreEqual(register.Directory,tickZoom,"TickZoom environment variable equals directory.");
			Assert.IsTrue(path.Contains(tickZoom),"path has directory");
		}
	}
}
