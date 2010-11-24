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
using System.Diagnostics;
using System.Reflection;

namespace TickZoom.Api
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	[Serializable]
	public class TickZoomException : System.ApplicationException
	{
		private string stackTrace;
		private string message;
		public TickZoomException(string message) : base() {
			StackFrame sf;
			int frame = 0;
			do {
				frame++;
				sf = new StackFrame(frame,true);
			} while( sf.GetMethod().DeclaringType.Namespace == "TickZoom.Interceptors");
			
			StackTrace st = new StackTrace(sf);
			stackTrace = st.ToString();
			this.message = message + " " + stackTrace;
		}
		
	    // Constructor needed for serialization 
	    // when exception propagates from a remoting server to the client.
	    public TickZoomException(System.Runtime.Serialization.SerializationInfo info,
	        System.Runtime.Serialization.StreamingContext context) { }
		
		public override string StackTrace {
	    	get { if( stackTrace == null) {
	    			return base.StackTrace;
	    		} else {
	    			return stackTrace;
	    		}
	    	}
		}
	    
		public override string Message {
			get { return message; }
		}

	    public override string ToString()
		{
			return Message + StackTrace;
		}
	}
}
