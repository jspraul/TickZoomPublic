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

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Starters
{
	public class RealTimeStarter : HistoricalStarter
	{
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public override Provider[] SetupProviders(bool quietMode, bool singleLoad)
		{
			switch( Address) {
				case "InProcess":
					return base.SetupDataProviders("127.0.0.1",Port);
				default:
					return base.SetupDataProviders(Address,Port);
			}
		}
		
		public override void Run(ModelInterface model)
		{
			ServiceConnection service = null;
			switch( Address) {
				case "InProcess":
					service = Factory.Provider.ProviderService();
					foreach( var provider in ProviderPlugins) {
						service.AddProvider(provider);
					}
					if( Config != null) {
						service.SetConfig(Config);
					}
					service.SetAddress("127.0.0.1",Port);
					break;
				default:
					break;
			}
			runMode = RunMode.RealTime;
			try {
				if( service != null) {
					service.OnStart();
				}
				base.Run(model);
			} finally {
				if( service != null) {
					service.OnStop();
				}
			}
		}
		public void SetupWarehouseConfig(string providerAssembly, ushort servicePort)
		{
			try { 
				string storageFolder = Factory.Settings["AppDataFolder"];
				var providersPath = Path.Combine(storageFolder,"Providers");
				string configPath = Path.Combine(providersPath,"ProviderCommon");
				string configFile = Path.Combine(configPath,"WarehouseTest.config");
				ConfigFile warehouseConfig = new ConfigFile(configFile);
				warehouseConfig.SetValue("ServerCacheFolder","Test\\ServerCache");
				warehouseConfig.SetValue("ServiceAddress","0.0.0.0");
				warehouseConfig.SetValue("ServicePort",servicePort.ToString());
				warehouseConfig.SetValue("ProviderAssembly",providerAssembly);
	 			// Clear the history files
			} catch( Exception ex) {
				log.Error("Setup error.",ex);
				throw ex;
			}
		}
	}
}
