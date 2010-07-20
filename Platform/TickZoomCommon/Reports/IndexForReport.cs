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
	public class IndexForReport : ReportHelper {
		Performance performance;
		public IndexForReport(Performance performance) {
			this.performance = performance;
		}
		
		public bool WriteReport(string name, string folder) {
			string pathName = folder + name + @"\index.html";
			fwriter = File.CreateText( pathName );
			WriteReport(name,fwriter);
			fwriter.Flush();
			fwriter.Close();
			fwriter = null;
			Thread.Sleep(100);
			return true;
		}
	
		private void WriteReport(string name, StreamWriter writer) {
			fwriter = writer;
			StrategyStats stats = new StrategyStats(performance.ComboTrades);
			fwriter.WriteLine("<HTML>");
			fwriter.WriteLine("<HEAD>");
			WriteStyle();
			fwriter.WriteLine("</HEAD>");
			fwriter.WriteLine("<BODY>");
			fwriter.WriteLine("<H1>" + name + " Strategy Report</H1>");
	
			fwriter.WriteLine("<A href=\"Equity.html\">Equity Report</A><BR>");
			fwriter.WriteLine("<A href=\"Trades.html\">Trade List</A>");
			
			fwriter.WriteLine("</BODY>");
			fwriter.WriteLine("</HTML>");
	
		}
	
	}
}
