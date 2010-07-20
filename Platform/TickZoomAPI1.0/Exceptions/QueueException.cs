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

namespace TickZoom.Api
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	[Serializable]
	public class QueueException : System.ApplicationException
	{
		private EventType queueItemType;
		private string symbol;
		public QueueException(EventType queueItemType)
			: base("Queue returned item type: " + queueItemType) {
			this.queueItemType = queueItemType;
		}
		public QueueException(EventType queueItemType, string symbol)
			: base("Queue returned item type: " + queueItemType) {
			this.queueItemType = queueItemType;
			this.symbol = symbol;
		}
	
	    // Constructor needed for serialization 
	    // when exception propagates from a remoting server to the client.
	    protected QueueException(System.Runtime.Serialization.SerializationInfo info,
	        System.Runtime.Serialization.StreamingContext context) { }
		
		public EventType EntryType {
			get { return queueItemType; }
		}
		
		public string Symbol {
			get { return symbol; }
		}
	}
}
