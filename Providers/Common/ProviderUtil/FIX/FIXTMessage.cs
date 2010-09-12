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
		private bool isWrapped = false;
		public FIXTMessage() {
		}
		public FIXTMessage( string version) {
			this.version = version;
		}
		public void Append(int key, string value) {
			EnsureUnwrapped();
			body.Append(key,value);
		}
		public void Append(int key, int value) {
			EnsureUnwrapped();
			body.Append(key,value);
		}
		public void Append(int key, double value) {
			EnsureUnwrapped();
			body.Append(key,value);
		}
		public void Append(int key, TimeStamp time) {
			EnsureUnwrapped();
			body.Append(key,time);
		}
		private void EnsureUnwrapped() {
			if( isWrapped) {
				throw new ApplicationException("FIXTMessage already wrapped. Must call Clear() first.");
			}
		}
		public abstract void AddHeader(string type);
		private void AddFIXTHeader() {
			// FIXT1.1 Header
			int length = body.Length;
			header.Clear();
			header.Append(8,version);
			header.Append(9,length);
			body.Insert(header.ToString());
		}
		private void AddFIXTFooter() {
			string message = body.ToString();
			int total = 0;
			for( int i=0; i<message.Length; i++) {
				total += (byte) message[i];
			}
			int checksum = total % 256;
			Append(10,checksum.ToString("000"));
		}
		public override string ToString()
		{
			if( !isWrapped) {
				if( header.Length == 0) {
					throw new ApplicationException("Must call AddHeaterFooter() before ToString()");
				}
				body.Insert(header.ToString());
				AddFIXTHeader();
				AddFIXTFooter();
				isWrapped = true;
			}
			return body.ToString();
		}
	}
}
