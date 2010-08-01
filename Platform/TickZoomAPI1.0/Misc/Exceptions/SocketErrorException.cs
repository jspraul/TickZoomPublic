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
using SocketError = System.Net.Sockets.SocketError;

namespace TickZoom.Api
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	[Serializable]
	public class SocketErrorException : System.ApplicationException
	{
		SocketError socketError;
		public SocketErrorException(SocketError error)
			: base( "socket failed with " + error) {
			this.socketError = error;
		}
		
	    public SocketErrorException(SocketError error, string message)
	    	: base( message + " with " + error) {
			this.socketError = error;
		}
		
	    public SocketErrorException(string message)
	    	: base( message) {
			this.socketError = SocketError.Success;
		}
		
	    public SocketErrorException(SocketError error, string message, System.Exception inner)
	    	: base( message + " with " + error, inner) {
			this.socketError = error;
		}
	
	    // Constructor needed for serialization 
	    // when exception propagates from a remoting server to the client.
	    protected SocketErrorException(System.Runtime.Serialization.SerializationInfo info,
	        System.Runtime.Serialization.StreamingContext context) { }
		
		public SocketError SocketError {
			get { return socketError; }
		}
	}
}
