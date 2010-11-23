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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

using TickZoom.Api;
using TickZoom.Statistics;

namespace TickZoom.Reports
{
	public class EquityStatsReport : ReportHelper {
		Equity equity;
		string folder;
		string name;
		StrategyStats strategyStats;

		public EquityStatsReport(Equity equity) {
			this.equity = equity;
		}
		
		public bool WriteReport(string name, string folder) {
			this.folder = folder;
			this.name = name;
			string fileName = folder + @"\" + name + @"\" + "Equity.html";
			Directory.CreateDirectory( Path.GetDirectoryName(fileName));			fwriter = File.CreateText( fileName );
			WriteReport();
			fwriter.Flush();
			fwriter.Close();
			fwriter = null;
			Thread.Sleep(100);
			return true;
		}
	
		private void WriteReport() {
			EquityStats stats = new EquityStats(equity.StartingEquity,equity.Daily,equity.Weekly,equity.Monthly,equity.Yearly);
//			WriteChart(folder,name,"DailyEquity",stats.Daily,equity.Daily);
			fwriter.WriteLine("<HTML>");
			fwriter.WriteLine("<HEAD>");
			WriteStyle();
			fwriter.WriteLine("</HEAD>");
			fwriter.WriteLine("<BODY>");
			fwriter.WriteLine("<H1>" + name + " Strategy Report</H1>");
	
//			fwriter.WriteLine("<img src=\"Images/DailyEquity.gif\"/><BR>");
			
			fwriter.WriteLine("<BR>");
			
			WriteTotalStatsHeader();
			WriteTotalStatsData(equity);
			WriteStatsFooter();
			
			fwriter.WriteLine("<BR>");
						
			WriteStatsHeader();
			WriteStatsData(stats.Monthly,"Month");
			WriteStatsData(stats.Yearly,"Year");
			WriteStatsData(stats.Weekly,"Week");
			WriteStatsData(stats.Daily,"Day");
			WriteStatsData(strategyStats.ComboTrades,"ComboTrades");
			WriteStatsFooter();
			
			fwriter.WriteLine("<BR>");
			
			WriteMonths(equity.Monthly, stats.Monthly);
			
			fwriter.WriteLine("<BR>");
			
			WriteDays(equity.Daily, stats.Daily);
			
			fwriter.WriteLine("</BODY>");
			fwriter.WriteLine("</HTML>");
	
		}
		
		public StrategyStats StrategyStats {
			get { return strategyStats; }
			set { strategyStats = value; }
		}
	}
}
