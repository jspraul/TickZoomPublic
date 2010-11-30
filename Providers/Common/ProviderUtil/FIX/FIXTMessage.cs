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
using System.Text;
using TickZoom.Api;

namespace TickZoom.FIX
{
	public abstract class FIXTMessage {
		protected FIXTBuffer header = new FIXTBuffer();
		protected FIXTBuffer body = new FIXTBuffer();
		protected string version = "FIXT.1.1";
		public FIXTMessage() {
		}
		public FIXTMessage( string version) {
			this.version = version;
		}
		public void Append(int key, string value) {
			body.Append(key,value);
		}
		public void Append(int key, int value) {
			body.Append(key,value);
		}
		public void Append(int key, double value) {
			body.Append(key,value);
		}
		public void Append(int key, TimeStamp time) {
			body.Append(key,time);
		}
		public abstract void AddHeader(string type);
		public abstract void CreateHeader();
		private void AddFIXTHeader(FIXTBuffer message) {
			var buffer = new FIXTBuffer();
			buffer.Append(8,version);
			buffer.Append(9,message.Length);
			message.Insert(buffer.ToString());
		}
		private void AddFIXTFooter(FIXTBuffer buffer) {
			string message = buffer.ToString();
			int total = 0;
			for( int i=0; i<message.Length; i++) {
				total += (byte) message[i];
			}
			int checksum = total % 256;
			buffer.Append(10,checksum.ToString("000"));
		}
		
		public override string ToString()
		{
			var message = new FIXTBuffer();
			CreateHeader();
			message.Append(body.ToString());
			message.Insert(header.ToString());
			AddFIXTHeader(message);
			AddFIXTFooter(message);
			return message.ToString();
		}
	}
}
