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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using TickZoom.Api;
using TickZoom.Statistics;
using TickZoom.Transactions;
using ZedGraph;

namespace TickZoom.Reports
{
	public class ReportHelper { 
		public static readonly Log log = Factory.SysLog.GetLogger(typeof(ReportHelper));
		public static readonly bool debug = log.IsDebugEnabled;
		public static readonly bool trace = log.IsTraceEnabled;
			
		protected StreamWriter fwriter;
		
		public void WriteTotalStatsHeader() {
			fwriter.WriteLine("<TABLE class=\"stats\" cellspacing=\"2\">");
			fwriter.WriteLine("<TR>");
			fwriter.WriteLine("<TD class=\"toprow\">Starting Equity</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Closed Equity</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Open Equity</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Total Equity</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Net Profit</TD>");
			fwriter.WriteLine("</TR>");
		}
		
		public void WriteTotalStatsData(Equity equity) {
			fwriter.WriteLine("<TR>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(equity.StartingEquity,2)+"</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(equity.ClosedEquity,2)+"</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(equity.OpenEquity,2)+"</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(equity.CurrentEquity,2)+"</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(equity.NetProfit,2)+"</TD>");
			fwriter.WriteLine("</TR>");
		}
		
		public void WriteStatsHeader() {
			fwriter.WriteLine("<TABLE class=\"stats\" cellspacing=\"2\">");
			fwriter.WriteLine("<TR>");
			fwriter.WriteLine("<TD class=\"toprow\">Time Frame</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Sortino Ratio</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Sharpe Ratio</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Volitility</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Max Downside</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Annualized Return</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">% Profitable</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Profit Factor</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Count</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Win Rate</TD>");
			fwriter.WriteLine("<TD class=\"toprow\">Average Net Profit</TD>");
			fwriter.WriteLine("</TR>");
		}
		
		public void WriteStatsFooter() {
			fwriter.WriteLine("</TABLE>");
		}
		
		public void WriteStatsData(TradeStats stats, string type) {
			fwriter.WriteLine("<TR>");
			if( type == "Day") {
				type = "Daily";
			} else if( type == "Trade") {
				type = "Trades";
			} else if( type == "ComboTrades") {
				type = "ComboTrades";
			} else {
				type += "ly";
			}
			fwriter.WriteLine("<TD class=\"leftcol\">"+type+"</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(stats.SortinoRatio,2)+"</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(stats.SharpeRatio,2)+"</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(stats.Volatility,2)+"</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(stats.MaxDownsideRisk*100,2)+"%</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(stats.AnnualReturn*100,1)+"%</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(stats.WinRate*100,1)+"%</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(stats.ProfitFactor,2)+"</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+stats.Count+"</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(stats.WinRate*100)+"%</TD>");
			fwriter.WriteLine("<TD class=\"data\">"+Math.Round(stats.Average,2)+"</TD>");
			fwriter.WriteLine("</TR>");
		}
		
		public void WriteStyle() {
		    fwriter.WriteLine("<STYLE type=\"text/css\">");
		    fwriter.WriteLine("table.stats { border-collapse: collapse; border-spacing: 0px; border: solid #000 1px; width: 200px; }");
			fwriter.WriteLine("table.stats td { padding: 1px; border: solid #000 .5px; }");
			fwriter.WriteLine(".data { color: #000000; text-align: right; }");
	            fwriter.WriteLine(".toprow { font-style: italic; text-align: center; background-color: #FFFFCC; }");
	            fwriter.WriteLine(".leftcol { font-weight: bold; text-align: left; width: 150px; ");
		    fwriter.WriteLine("</STYLE>");
		}
	
		public void WriteChart(string folder, string strategyName, string chartName, TradeStats stats, TransactionPairs daily)
		{
			if( daily.Count == 0) return;
			GraphPane myPane = new GraphPane( new RectangleF( 0, 0, 640, 480 ),
			         chartName, "Date", "Price" );
			
			PointPairList ppl = new PointPairList();
			double y = stats.BeginningBalance;
			ppl.Add( daily[0].EntryTime.ToOADate(), y);
			for(int i=0; i<daily.Count; i++) {
				y += daily.CalcProfitLoss(i);
				ppl.Add( daily[i].ExitTime.ToOADate(), y);
			}
			
			if( trace) log.Trace( "Chart start = " + ppl[0].Y + ", end = " + ppl[ppl.Count-1].Y );
			
			LineItem myCurve = myPane.AddCurve( "Profit/Loss Equity Curve", ppl, Color.Blue, SymbolType.None);
		    myCurve.Line.Fill = new Fill( Color.Blue );
			
		    // pretty it up a little
		    myPane.Chart.Fill = new Fill( Color.White, Color.LightGoldenrodYellow, 45.0f );
		    myPane.Fill = new Fill( Color.White);
		    myPane.Border.IsVisible = false;
		    myPane.Title.FontSpec.Size = 20.0f;
		    myPane.XAxis.Type = AxisType.DateAsOrdinal;
		    myPane.XAxis.Title.FontSpec.Size = 14.0f;
		    myPane.XAxis.Scale.FontSpec.Size = 14.0f;
		    myPane.XAxis.Title.IsOmitMag = true;
		    
		    myPane.YAxis.Title.FontSpec.Size = 14.0f;
		    myPane.YAxis.Scale.FontSpec.Size = 14.0f;
//		    myPane.YAxis.Title.IsOmitMag = true;
		    myPane.Legend.IsVisible = false;
		    
			myPane.AxisChange();
		    
		    string pathName = folder + strategyName + @"\Images\"+chartName+".gif";
		    Directory.CreateDirectory(Path.GetDirectoryName(pathName));
			myPane.GetImage().Save(pathName, ImageFormat.Gif);
		}
		
		public void WriteYearHeader() {
			fwriter.WriteLine("<TABLE class=\"stats\">");
		}
		
		public string WriteYearTitles(int year) {
			string yearTitle = "<TR>";
			yearTitle += "<TD class=\"toprow\">"+year+"</TD>";
			yearTitle += "<TD class=\"toprow\">Net Profit</TD>";
			yearTitle += "<TD class=\"toprow\">Monthly Return</TD>";
			yearTitle += "<TD class=\"toprow\">Year to Date</TD>";
			yearTitle += "<TD class=\"toprow\">Equity</TD>";
			yearTitle += "</TR>";
			return yearTitle;
		}
		
		public void WriteYearFooter() {
			fwriter.WriteLine("</TABLE>");
		}
		
		public void WriteMonths(TransactionPairs trades, TradeStats stats) {
			if( trades.Count > 0) {
				int year = 2003;
				double currentBalance = stats.BeginningBalance;
				double yearBalance = stats.BeginningBalance;
				double ytdProfitLoss = 0;
				WriteYearHeader();
				List<string> tableData = new List<string>();
	        	for( int i=0; i<trades.Count; i++) {
					if( trades[i].EntryTime.Year != year) {
						tableData.Add( WriteYearTitles(year) );
						yearBalance = yearBalance + ytdProfitLoss;
						ytdProfitLoss = 0;
						year = trades[i].EntryTime.Year;
					}
					double profitLoss = trades.CalcProfitLoss(i);
					string tableRow = "<tr>";
					tableRow += "<td class=\"leftcol\">"+trades[i].EntryTime.Month+"/"+year+"</td>";
					tableRow += "<td class=\"data\">"+Math.Round(profitLoss,2)+"</td>";
					double monthlyReturn = ((currentBalance+profitLoss)/currentBalance)-1;
					tableRow += "<td class=\"data\">"+Math.Round(monthlyReturn*100,2)+"%</td>";
					ytdProfitLoss += profitLoss;
					double ytdReturn = ((yearBalance+ytdProfitLoss)/yearBalance)-1;
					tableRow += "<td class=\"data\">"+Math.Round(ytdReturn*100,2)+"%</td>";
					currentBalance += profitLoss;
					tableRow += "<td class=\"data\">"+Math.Round(currentBalance,2)+"</td>";
					tableRow += "</tr>";
					tableData.Add(tableRow);
	        	}
				tableData.Add( WriteYearTitles(year) );
				for( int i=tableData.Count-1; i>= 0; i--) {
					fwriter.WriteLine( tableData[i]);
				}
				WriteYearFooter();
			}
		}
		
		public void WriteDays(TransactionPairs trades, TradeStats stats) {
			if( trades.Count > 0) {
				int year = 0;
				double currentBalance = stats.BeginningBalance;
				double yearBalance = stats.BeginningBalance;
				double ytdProfitLoss = 0;
				WriteYearHeader();
				List<string> tableData = new List<string>();
	        	for( int i=0; i<trades.Count; i++) {
					if( trades[i].EntryTime.Year != year) {
						tableData.Add( WriteYearTitles(year) );
						yearBalance = yearBalance + ytdProfitLoss;
						ytdProfitLoss = 0;
						year = trades[i].EntryTime.Year;
					}
					double profitLoss = trades.CalcProfitLoss(i);
					string tableRow = "<tr>";
					tableRow += "<td class=\"leftcol\">"+trades[i].EntryTime.Month+"/"+trades[i].EntryTime.Day+"/"+year+"</td>";
					tableRow += "<td class=\"data\">"+Math.Round(profitLoss,2)+"</td>";
					double monthlyReturn = ((currentBalance+profitLoss)/currentBalance)-1;
					tableRow += "<td class=\"data\">"+Math.Round(monthlyReturn*100,2)+"%</td>";
					ytdProfitLoss += profitLoss;
					double ytdReturn = ((yearBalance+ytdProfitLoss)/yearBalance)-1;
					tableRow += "<td class=\"data\">"+Math.Round(ytdReturn*100,2)+"%</td>";
					currentBalance += profitLoss;
					tableRow += "<td class=\"data\">"+Math.Round(currentBalance,2)+"</td>";
					tableRow += "</tr>";
					tableData.Add(tableRow);
	        	}
				tableData.Add( WriteYearTitles(year) );
				for( int i=tableData.Count-1; i>= 0; i--) {
					fwriter.WriteLine( tableData[i]);
				}
				WriteYearFooter();
			}
		}
		
		public void WriteTradeHeader() {
			fwriter.WriteLine("<TABLE class=\"stats\">");
		}
		
		public string WriteTradeTitles(int year, bool combo) {
			string yearTitle = "<TR>";
			yearTitle += "<TD class=\"toprow\">#</TD>";
			if( combo) {
				yearTitle += "<TD class=\"toprow\">Combo<BR>Entry</TD>";
				yearTitle += "<TD class=\"toprow\">Combo<BR>Exit</TD>";
			} else {
				yearTitle += "<TD class=\"toprow\">Entry<BR>Time</TD>";
				yearTitle += "<TD class=\"toprow\">Exit<BR>Time</TD>";
			}
			yearTitle += "<TD class=\"toprow\">Entry Price</TD>";
			yearTitle += "<TD class=\"toprow\">Entry Bar</TD>";
			yearTitle += "<TD class=\"toprow\">Exit Price</TD>";
			yearTitle += "<TD class=\"toprow\">Exit Bar</TD>";
			yearTitle += "<TD class=\"toprow\">Direction/Size</TD>";
			yearTitle += "<TD class=\"toprow\">MAE</TD>";
			yearTitle += "<TD class=\"toprow\">MFE</TD>";
			yearTitle += "<TD class=\"toprow\">Net Profit</TD>";
			yearTitle += "<TD class=\"toprow\">Trade Return</TD>";
			yearTitle += "<TD class=\"toprow\">Year to Date</TD>";
			yearTitle += "<TD class=\"toprow\">Equity</TD>";
			yearTitle += "</TR>";
			return yearTitle;
		}
		
		public void WriteTradeFooter() {
			fwriter.WriteLine("</TABLE>");
		}
		
		public void WriteTrades(TransactionPairs transactionPairs, TradeStats stats, bool combo) {
			if( transactionPairs != null && transactionPairs.Count > 0) {
				int year = 0;
				double currentBalance = stats.BeginningBalance;
				double yearBalance = stats.BeginningBalance;
				double ytdProfitLoss = 0;
				WriteTradeHeader();
				List<string> tableData = new List<string>();
	        	for( int i=0; i<transactionPairs.Count; i++) {
					if( transactionPairs[i].EntryTime.Year != year) {
						tableData.Add( WriteTradeTitles(year,combo) );
						yearBalance += ytdProfitLoss;
						ytdProfitLoss = 0;
						year = transactionPairs[i].EntryTime.Year;
					}
					double profitLoss = transactionPairs.CalcProfitLoss(i);
					string tableRow = "<tr>";
					tableRow += "<td class=\"leftcol\">"+i+"</td>";
					tableRow += "<td class=\"data\">"+transactionPairs[i].EntryTime+"</td>";
					tableRow += "<td class=\"data\">"+transactionPairs[i].ExitTime+"</td>";
					tableRow += "<td class=\"data\">"+transactionPairs[i].EntryPrice.ToString(",0.000")+"</td>";
					tableRow += "<td class=\"data\">"+transactionPairs[i].EntryBar+"</td>";
					tableRow += "<td class=\"data\">"+transactionPairs[i].ExitPrice.ToString(",0.000")+"</td>";
					tableRow += "<td class=\"data\">"+transactionPairs[i].ExitBar+"</td>";
					tableRow += "<td class=\"data\">"+((transactionPairs[i].Direction>0)?"Long":"Short")+" "+Math.Round(Math.Abs(transactionPairs[i].Direction),2)+"</td>";
					tableRow += "<td class=\"data\">"+Math.Round(transactionPairs.CalcMaxAdverse(i),2)+"</td>";
					tableRow += "<td class=\"data\">"+Math.Round(transactionPairs.CalcMaxFavorable(i),2)+"</td>";
					tableRow += "<td class=\"data\">"+Math.Round(profitLoss,2)+"</td>";
					double monthlyReturn = ((currentBalance+profitLoss)/currentBalance)-1;
					tableRow += "<td class=\"data\">"+Math.Round(monthlyReturn*100,2)+"%</td>";
					ytdProfitLoss += profitLoss;
					double ytdReturn = ((yearBalance+ytdProfitLoss)/yearBalance)-1;
					tableRow += "<td class=\"data\">"+Math.Round(ytdReturn*100,2)+"%</td>";
					currentBalance += profitLoss;
					tableRow += "<td class=\"data\">"+Math.Round(currentBalance,2)+"</td>";
					tableRow += "</tr>";
					tableData.Add(tableRow);
	        	}
				tableData.Add( WriteTradeTitles(year, combo) );
				for( int i=0; i<tableData.Count; i++) {
					fwriter.WriteLine( tableData[i]);
				}
				WriteTradeFooter();
			}
		}
	}
	

}
