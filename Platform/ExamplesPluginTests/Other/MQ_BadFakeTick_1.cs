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

#region Namespaces
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TickZoom.Api;
using TickZoom.Common;
#endregion

namespace Other
{
    public class MQ_BadFakeTick_1 : Strategy
	{

        /// <summary>
        /// Contracts to trade
        /// </summary>
        public int Quantity
        { get { return quantity; } set { quantity = value; } }
        int quantity = 50;


        public MQ_BadFakeTick_1()
        {
		}
		   
		public override void OnInitialize()
		{
		}

		public override bool OnIntervalClose()
		{
            if ((((DateTime)Bars.EndTime[0]).ToString("yyyyMMdd") == "20080908") &&
                (Bars.EndTime[0].TimeOfDay == new Elapsed(9, 42, 0)))          
            {
                Orders.Enter.ActiveNow.BuyLimit(127.64,quantity);
                if (Bars.High[0] != Bars.Low[0]) {};
            }
            if ((((DateTime)Bars.EndTime[0]).ToString("yyyyMMdd") == "20080908") &&
                (Bars.EndTime[0].TimeOfDay == new Elapsed(9, 43, 0)))
            {
                Orders.Exit.ActiveNow.SellStop(127.09);
                if (Bars.High[0] != Bars.Low[0]) { };
            }

			return true;
		}
    }
}