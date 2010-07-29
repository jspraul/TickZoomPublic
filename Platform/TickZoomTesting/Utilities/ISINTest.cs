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
using NUnit.Framework;
using TickZoom.Api;

namespace TickZoom.Utilities
{
	/// <summary>
	/// Description of ISINTest.
	/// </summary>
	[TestFixture]
	public class ISINTest
	{
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public ISINTest()
		{
		}
		
		[TestFixtureSetUpAttribute]
		public void Setup() {
		}
		
		[Test]
		public void NumberTest() {
			Assert.AreEqual(0,AlphaNumToDigit('0'));
			Assert.AreEqual(9,AlphaNumToDigit('9'));
			Assert.AreEqual(10,AlphaNumToDigit('A'));
			Assert.AreEqual(35,AlphaNumToDigit('Z'));
		}
		
		[Test]
		public void ConvertISIN() {
			// Google
			Assert.AreEqual( 546483516028279289, ISINToSecurityId("US38259P5089"));
		}
		
		[Test]
		public void ConvertSecurityId() {
			// Google
			Assert.AreEqual( "US38259P5089", SecurityIdToISIN(546483516028279289));
		}
		
		[Test]
		public void MaxISIN() {
			// Google                            US38259P5089
			ulong securityId = ISINToSecurityId("ZZZZZZZZZZZ9");
			string binaryString = Convert.ToString((long)securityId,2);
			log.Debug("Binary Max Security ID = " + binaryString);
			for( int i=0;i<9;i++) {
				binaryString = Convert.ToString((long)AlphaNumToDigit((char)('0'+i)),2);
				log.Debug("Security ID = " + binaryString);
			}
			for( int i=0;i<26;i++) {
				binaryString = Convert.ToString((long)AlphaNumToDigit((char)('A'+i)),2);
				log.Debug("Security ID = " + binaryString);
			}
			Assert.AreEqual( 686545307078492159, securityId);
		}
		
		[Test,ExpectedExceptionAttribute(typeof(ApplicationException))]
		public void ExceptionTest() {
			Assert.AreEqual(0,AlphaNumToDigit('a'));
		}
		
		public ulong ISINToSecurityId( string ISIN) {
			if( ISIN.Length != 12) {
				throw new ApplicationException("ISIN must be 12 digits");
			}
			ulong checkDigit = AlphaNumToDigit( ISIN[ISIN.Length-1]);
			ulong securityId = checkDigit;
			int first = ISIN.Length-2;
			ulong digit;
			ulong power=10;
			for( int i=first; i>=2; i--) {
				digit = AlphaNumToDigit( ISIN[i]);
				log.Debug("Digit " + ISIN[i]+ " = " + digit);
				securityId += digit * power;
				log.Debug("Security Id = " + securityId);
				power*=36;
			}
			digit = AlphaNumToDigit(ISIN[1])-10;
			log.Debug("Country Code Char 2 " + ISIN[1] + " = " + digit);
			securityId += digit * power;
			log.Debug("Security Id = " + securityId);
			power*=26;
			digit = AlphaNumToDigit(ISIN[0])-10;
			log.Debug("Country Code Char 1 " + ISIN[0] + " = " + digit);
			securityId += digit * power;
			log.Debug("Security Id = " + securityId);
			return securityId;
		}
		
		private int SumDigits(int digits) {
			int sum = 0;
			while (digits != 0) {
			    sum += digits % 10;
			    digits /= 10;
			}
			return sum;
		}

		public string SecurityIdToISIN( ulong securityId) {
			string retVal = "";
			int digit;
			char alphaNum;
			log.Debug("SecurityIdToISIN");
			
			// First get the check digit.
			int checkDigit = (int) (securityId % 10);
			alphaNum = DigitToAlphaNum(checkDigit);
			retVal = alphaNum + retVal;
			securityId/=10;
			
			// Get the 9 alpa numeric digits.
			for( int i=0; i<9; i++) {
				log.Debug("Security Id = " + securityId);
				digit = (int) (securityId % 36);
				alphaNum = DigitToAlphaNum(digit);
				log.Debug("Digit " + alphaNum + " = " + digit);
				retVal = alphaNum + retVal;
				securityId /= 36;
			}
			
			// Get the 2 letters for country code
			log.Debug("Security Id = " + securityId);
			digit = (int) (securityId % 26);
			alphaNum = (char) ('A'+digit);
			log.Debug("Digit " + alphaNum + " = " + digit);
			retVal = alphaNum + retVal;
			securityId /= 26;
			
			log.Debug("Security Id = " + securityId);
			digit = (int) securityId;
			alphaNum = (char) ('A'+digit);
			log.Debug("Digit " + alphaNum + " = " + digit);
			retVal = alphaNum + retVal;
			
			return retVal;
		}
		
		public ulong AlphaNumToDigit(char digit) {
			if( digit >= '0' && digit <= '9') {
				return (ulong) (digit-'0');
			}
			if( digit >= 'A' && digit <= 'Z') {
				return (ulong) (digit-'A'+10);
			}
			throw new ApplicationException("Invalid ISIN digit");
		}
		
		public char DigitToAlphaNum(int digit) {
			if( digit < 10) {
				return (char) ('0'+digit);
			}
			if( digit < 36) {
				return (char) ('A'+digit-10);
			}
			throw new ApplicationException("Invalid ISIN digit: " + digit);
		}
	}
}
