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
using System.Security.Cryptography;
using System.Threading;

using TickZoom.Api;

namespace TickZoom.FIX
{
	public class FIXTMessage1_1 : FIXTMessage {
		string encoding;
		string sender;
		string target;
		int sequence;
		public int Sequence {
			get {
				return sequence;
			}
			set {
				sequence = value;
			}
		}
		public FIXTMessage1_1(string version, string sender, string target) : base(version) {
			this.target = target;
			this.sender = sender;
		}
		
		/// <summary>
		/// 98 Encryption. 0= NO encryption
		/// </summary>
		public void SetEncryption(int value) {
			Append(98,value);  
		}
		/// <summary>
		/// 7 BeginSeqNumber
		/// </summary>
		public void SetBeginSeqNum(int value) {
			Append(7,value);  
		}
		/// <summary>
		/// 16 EndSeqNumber
		/// </summary>
		public void SetEndSeqNum(int value) {
			Append(16,value);  
		}
		/// <summary>
		/// 43 Possible Duplicate
		/// </summary>
		public void SetDuplicate(bool value) {
			if( value) {
				Append(43,"Y");  
			} else {
				Append(43,"N");  
			}
		}
		/// <summary>
		/// 108 HeartBeatInterval. In seconds.
		/// </summary>
		public void SetHeartBeatInterval(int value) {
			Append(108,value); 
		}
		/// <summary>
		/// 141 Reset sequence number
		/// </summary>
		public void ResetSequence() {
			Append(141,"Y"); 
		}
		/// <summary>
		/// 347 encoding.  554_H1 for MBTrading hashed password.
		/// </summary>
		public void SetEncoding(string encoding) {
			this.encoding = encoding;
			Append(347,encoding); // Message Encoding (for hashed password)
		}
		/// <summary>
		/// 554 password in plain text. Will be hashed automatically.
		/// </summary>
		public void SetPassword(string password) {
			if( encoding == "554_H1") {
				password = Hash(password);
			}
			Append(554,password);
		}
		public override void AddHeader(string type)
		{
			header.Append(35,type);
			header.Append(49,sender);
			header.Append(56,target);
			header.Append(34,Sequence);
			header.Append(52,TimeStamp.UtcNow);
		}
		
		public static string Hash(string password) {
			SHA256 hash = new SHA256Managed();
			char[] chars = password.ToCharArray();
			byte[] bytes = new byte[chars.Length];
			for( int i=0; i<chars.Length; i++) {
				bytes[i] = (byte) chars[i];
			}
			byte[] hashBytes = hash.ComputeHash(bytes);
			string hashString = BitConverter.ToString(hashBytes);
			return hashString.Replace("-","");
		}
		
		public string Sender {
			get { return sender; }
		}
	}
}
