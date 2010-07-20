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
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Statistics
{
	public class ProfitLossCallback : ProfitLoss {
		double slippage = 0.0D;
		double commission = 0.0D;
		double fullPointValue = 1D;
		Model model;
		bool firstTime = true;
		bool userImplemented = false;
		SymbolInfo symbol;
		
		public ProfitLossCallback() {
		}
		
		public ProfitLossCallback(Model model) {
			this.model = model;
		}
		
		public double CalculateProfit( double position, double entry, double exit) {
			if( firstTime ){
				try {
					if( model != null) {
						model.OnCalculateProfitLoss(position, entry, exit);
						userImplemented = true;
					} else {
						userImplemented = false;
					}
				} catch( NotImplementedException) {
					userImplemented = false;							
				}
				firstTime = false;
			}
			
			if( userImplemented) {
				return model.OnCalculateProfitLoss(position, entry, exit);
			} else {
				double transactionCosts = (slippage + commission)*fullPointValue*Math.Abs(position);
				double pnl = ((exit - entry) * position * fullPointValue) - transactionCosts;
				return pnl.Round();
			}
		}
		
		public SymbolInfo Symbol {
			get { return symbol; }
			set { symbol = value; }
		}
	}
}
