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
using System.Text;
using System.Text.RegularExpressions;

namespace TickZoom.Common
{
    /// <summary>
    /// Arguments class
    /// </summary>

    public class Arguments {
        // Variables
        private Dictionary<string,string> parameters;
        private List<string> simpleArgs = new List<string>();
        // Constructor

        public Arguments(string[] args)
        {
            parameters = new Dictionary<string,string>();
            var spliter = new Regex(@"^-{1,2}|^/|=|:",
                RegexOptions.IgnoreCase|RegexOptions.Compiled);

            var remover = new Regex(@"^['""]?(.*?)['""]?$",
                RegexOptions.IgnoreCase|RegexOptions.Compiled);

            string parameter = null;
            string[] parts;
            
            

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'

            foreach(string txt in args)
            {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)

                parts = spliter.Split(txt,3);

                switch(parts.Length) {
                		
                // Found a value (for the last parameter 
                // found (space separator))
                case 1:
                    if(parameter != null)
                    {
                        if(!parameters.ContainsKey(parameter)) 
                        {
                            parts[0] = remover.Replace(parts[0], "$1");
                            parameters.Add(parameter, parts[0]);
                        }
                        parameter=null;
                    } else {
                    	simpleArgs.Add( parts[0]);
                    }
                    // else Error: no parameter waiting for a value (skipped)

                    break;

                // Found just a parameter
                case 2:
                    // The last parameter is still waiting. 
                    // With no value, set it to true.

                    if(parameter!=null)
                    {
                        if(!parameters.ContainsKey(parameter)) 
                            parameters.Add(parameter, "true");
                    }
                    parameter=parts[1];
                    break;

                // Parameter with enclosed value
                case 3:
                    // The last parameter is still waiting. 
                    // With no value, set it to true.

                    if(parameter != null)
                    {
                        if(!parameters.ContainsKey(parameter)) 
                            parameters.Add(parameter, "true");
                    }

                    parameter = parts[1];

                    // Remove possible enclosing characters (",')

                    if(!parameters.ContainsKey(parameter))
                    {
                        parts[2] = remover.Replace(parts[2], "$1");
                        parameters.Add(parameter, parts[2]);
                    }

                    parameter=null;
                    break;
                }
            }
            // In case a parameter is still waiting
            if(parameter != null)
            {
                if(!parameters.ContainsKey(parameter)) 
                    parameters.Add(parameter, "true");
            }
        }

        // Retrieve a parameter value if it exists 
        // (overriding C# indexer property)
        
        public bool TryGetValue( string Param, out string value) {
        	return parameters.TryGetValue(Param, out value);
        }

        public string this [string Param]
        {
            get
            {
                return(parameters[Param]);
            }
        }
        
        public string[] SimpleArgs {
        	get { return simpleArgs.ToArray(); }
		}
    }
}
