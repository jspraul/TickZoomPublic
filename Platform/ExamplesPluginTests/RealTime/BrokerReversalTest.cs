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

using Loaders;
using NUnit.Framework;
using TickZoom;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Starters;
using ZedGraph;

namespace MockProvider
{
	[TestFixture]
	public class BrokerReversalTest : ExampleReversalTest {
		
		public BrokerReversalTest() {
			SyncTicks.Enabled = true;
			ConfigurationManager.AppSettings.Set("ProviderAddress","InProcess");
			DeleteFiles();
			Symbols = "Daily4Sim";
			CreateStarterCallback = CreateStarter;
			MatchTestResultsOf(typeof(ExampleReversalTest));
			ShowCharts = false;
			StoreKnownGood = false;
		}
		
		public Starter CreateStarter()
		{
			ushort servicePort = 6490;
			SetupWarehouseConfig(servicePort);
			Starter starter = new RealTimeStarter();
			starter.ProjectProperties.Engine.SimulateRealTime = true;
			starter.Config = "WarehouseTest.config";
			starter.Port = servicePort;
			return starter;
		}
		
		private void DeleteFiles() {
			while( true) {
				try {
					string appData = Factory.Settings["AppDataFolder"];
		 			File.Delete( appData + @"\Test\\ServerCache\Daily4Sim.tck");
					break;
				} catch( Exception) {
				}
			}
		}
	}
}
