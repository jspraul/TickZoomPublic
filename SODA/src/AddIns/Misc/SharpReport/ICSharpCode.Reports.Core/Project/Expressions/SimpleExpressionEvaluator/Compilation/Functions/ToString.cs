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


namespace SimpleExpressionEvaluator.Compilation.Functions
{
    /// <summary>
    /// A C# type function that converts the 2nd and the rest of arguments to string using the 1st argument 
    /// as the formatter. If there is only one argument passed in, the string representation of that argument
    /// is returned.
    /// </summary>
    public class ToString : Function<string>
    {
        protected override string EvaluateFunction(object[] args)
        {
            if (args.Length == 1)
                return args[0].ToString();
            if (args.Length > 1)
            {
                int nTarget = args.Length - 1;
                string strFormat = args[0].ToString();
                var newArgs = new object[nTarget];
                Array.Copy(args, 1, newArgs, 0, nTarget);
                try
                {
                    return String.Format(strFormat, newArgs);
                }
                catch (Exception ex)
                {
                    return "#ERROR:" + ex.Message + "#";
                }
            }
            return null;
        }
    }
}