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
	public class RealTimeStarter : RealTimeStarterBase
	{
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		public override void Run(ModelLoaderInterface loader)
		{
			Factory.SysLog.Reconfigure("RealTime",GetDefaultLogConfig());
			base.Run(loader);
		}
	
		public override void Run(ModelInterface model)
		{
			ProjectProperties.Starter.EndTime = TimeStamp.MaxValue;
			base.Run(model);
		}
		
		private string GetDefaultLogConfig() {
			return @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
 <log4net>
    <root>
	<level value=""INFO"" />
	<appender-ref ref=""FileAppender"" />
	<appender-ref ref=""ConsoleAppender"" />
    </root>
    <logger name=""StatsLog"">
        <level value=""DEBUG"" />
    	<additivity value=""false"" />
	<appender-ref ref=""StatsLogAppender"" />
    </logger>
    <logger name=""TradeLog"">
        <level value=""DEBUG"" />
    	<additivity value=""false"" />
	<appender-ref ref=""TradeLogAppender"" />
    </logger>
    <logger name=""TransactionLog.Performance"">
        <level value=""DEBUG"" />
    	<additivity value=""false"" />
	<appender-ref ref=""TransactionLogAppender"" />
    </logger>
    <logger name=""BarDataLog"">
        <level value=""DEBUG"" />
    	<additivity value=""false"" />
	<appender-ref ref=""BarDataLogAppender"" />
    </logger>
    <logger name=""TickZoom.Common"">
        <level value=""INFO"" />
    </logger>
    <logger name=""TickZoom.FIX"">
        <level value=""DEBUG"" />
    </logger>
    <logger name=""TickZoom.MBTFIX"">
        <level value=""DEBUG"" />
    </logger>
    <logger name=""TickZoom.MBTQuotes"">
        <level value=""DEBUG"" />
    </logger>
    <logger name=""TickZoom.Engine.SymbolReceiver"">
        <level value=""INFO"" />
    </logger>
    <logger name=""TickZoom.ProviderService"">
        <level value=""INFO"" />
    </logger>
    <logger name=""TickZoom.Engine.EngineKernel"">
        <level value=""INFO"" />
    </logger>
    <logger name=""TickZoom.Internals.OrderGroup"">
        <level value=""INFO"" />
    </logger>
    <logger name=""TickZoom.Internals.OrderManager"">
        <level value=""INFO"" />
    </logger>
    <logger name=""TickZoom.Engine.SymbolController"">
        <level value=""DEBUG"" />
    </logger>
    <logger name=""TickZoom.Interceptors.FillSimulatorPhysical"">
        <level value=""DEBUG"" />
    </logger>
    <logger name=""TickZoom.Interceptors.FillHandlerDefault"">
        <level value=""DEBUG"" />
    </logger>
    <logger name=""TickZoom.Common.OrderAlgorithmDefault"">
        <level value=""DEBUG"" />
    </logger>
 </log4net>
</configuration>
";				
		}
	}
}
