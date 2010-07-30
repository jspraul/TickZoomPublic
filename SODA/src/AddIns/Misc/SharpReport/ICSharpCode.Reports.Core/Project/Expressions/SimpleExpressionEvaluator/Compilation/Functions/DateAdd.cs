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
using System.Text.RegularExpressions;
using SimpleExpressionEvaluator.Compilation.Functions;
using SimpleExpressionEvaluator.Utilities;

namespace SimpleExpressionEvaluator.Compilation.Functions
{
    /// <summary>
    /// Add's the specified number of periods to the provided date.
    /// Argument 0 is the date to manipulate
    /// Argument 1 is the period to add d or D is Days, w or W is Weeks, m or M is Months, y or Y is Years
    /// Argument 2 is the number of periods to add
    /// </summary>
    public class DateAdd : Function<DateTime>
    {
        private static readonly Regex VALIDATOR = new Regex("[dDwWmMyY]", RegexOptions.Compiled);

        protected override int ExpectedArgumentCount
        {
            get
            {
                return 3;
            }
        }
        
        private static void ValidatePeriodCode(string code)
        {
            if (String.IsNullOrEmpty(code))
                throw new Exception(
                    "The second argument in DateAdd was not provided. It must be one of the following (case insensitive): d,w,m,y");

            if (code.Length != 1 || !VALIDATOR.IsMatch(code))
            {
                if (String.IsNullOrEmpty(code))
                    throw new Exception(
                        "The second argument in DateAdd was not provided. It must be one of the following (case insensitive): d,w,m,y");
            }
        }

        protected override DateTime EvaluateFunction(object[] args)
        {

            args[0] = args[0] == null ? DateTime.Today : TypeNormalizer.EnsureType(args[0], typeof (DateTime));

            args[1] = args[1].ToString().ToUpper();
            args[2] = TypeNormalizer.EnsureType(args[2], typeof (Int32));
            
            var startDate = (DateTime) args[0];
            if (startDate == DateTime.MinValue)
                startDate = DateTime.Today;

            string code = args[1].ToString();

            ValidatePeriodCode(code);

            var distance = (int) args[2];

            switch(code)
            {
                case "D":
                    return startDate.AddDays(distance);
                case "W":
                    return startDate.AddDays(distance*7);
                case "M":
                    return startDate.AddMonths(distance);
                case "Y":
                    return startDate.AddYears(distance);
                default:
                    return startDate.AddDays(distance);
            }
        }
    }
}