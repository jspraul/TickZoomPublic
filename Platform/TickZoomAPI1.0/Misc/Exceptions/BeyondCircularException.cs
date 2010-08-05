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

namespace TickZoom.Api
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	[Serializable]
	public class BeyondCircularException : System.ApplicationException
	{
		private Countable circularArray;
		private int position;
		private string stackTrace;
	    public BeyondCircularException() {
			StackFrame sf;
			int frame = 0;
			do {
				frame++;
				sf = new StackFrame(frame,true);
			} while( sf.GetMethod().Name.Equals("get_Item") ||
			         sf.GetMethod().Name.Equals("set_Item") );
			
			StackTrace st = new StackTrace(sf);
			stackTrace = st.ToString();
		}
		
	    // Constructor needed for serialization 
	    // when exception propagates from a remoting server to the client.
	    public BeyondCircularException(System.Runtime.Serialization.SerializationInfo info,
	        System.Runtime.Serialization.StreamingContext context) { }
		
		public Countable CircularArray {
			get { return circularArray; }
			set { circularArray = value; }
		}
		
		public int Position {
			get { return position; }
			set { position = value; }
		}
	    
		public override string Message {
	    	get { string typeName = circularArray.GetType().Name;
	    		if( circularArray.GetType().IsGenericType == true) {
	    			Type[] args = circularArray.GetType().GetGenericArguments();
	    			string generics = "<";
	    			for( int i=0; i<args.Length;i++) {
	    				if( i!=0) { typeName+=","; }
	    				generics += args[i].Name;
	    			}
	    			generics += ">";
	    			typeName = typeName.Replace("`1",generics);
	    		}
	    		
	    		return typeName + " used index of " + position; }
		}
	    
		public override string ToString()
		{
			return Message + StackTrace;
		}
	}
}
