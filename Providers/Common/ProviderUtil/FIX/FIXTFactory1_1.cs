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

using TickZoom.Api;

namespace TickZoom.FIX
{
	public class FIXTFactory1_1 : FIXTFactory {
		private int nextSequence;
		private int lastSequence;
		private string sender;
		private string destination;
		private Dictionary<int,FIXTMessage1_1> messageHistory = new Dictionary<int,FIXTMessage1_1>();
		public FIXTFactory1_1(int nextSequence, string sender, string destination) {
			this.nextSequence = nextSequence;
			this.sender = sender;
			this.destination = destination;
		}
		public virtual FIXTMessage1_1 Create() {
			var message = new FIXTMessage1_1("FIXT1.1",sender,destination);
			message.Sequence = nextSequence;
			nextSequence ++;
			return message;
		}
		public string Sender {
			get { return sender; }
		}
		public string Destination {
			get { return destination; }
		}
		public int NextSequence {
			get { return nextSequence; }
			set { nextSequence = value; }
		}
		public void AddHistory(FIXTMessage1_1 fixMsg) {
			lastSequence = fixMsg.Sequence;
			messageHistory.Add( fixMsg.Sequence, fixMsg);
		}
		public FIXTMessage1_1 GetHistory(int sequence) {
			FIXTMessage1_1 result;
			if( messageHistory.TryGetValue(sequence, out result)) {
				return result;
			} else {
				throw new ApplicationException("Cannot get history for sequence: " + sequence);
			}
		}
		public int LastSequence {
			get { return lastSequence; }
		}
	}
}
