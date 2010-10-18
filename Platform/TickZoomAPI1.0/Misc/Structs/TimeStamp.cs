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
using System.Threading;

namespace TickZoom.Api
{

	
	public struct TimeStamp : IComparable<TimeStamp>
	{
		private long _timeStamp;
		private static readonly TimeStamp maxValue = new TimeStamp(DoubleDayMax);
		private static readonly TimeStamp minValue = new TimeStamp(DoubleDayMin);
		
		public static TimeStamp MaxValue { 
			get { return maxValue; }
		}
		public static TimeStamp MinValue { 
			get { return minValue; }
		}
		public const double XLDay1 = 2415018.5;

		public const double JulianDayMin = 0.0;
		public const double JulianDayMax = 5373483.5;
		public const double DoubleDayMin = JulianDayMin - XLDay1;
		public const double DoubleDayMax = JulianDayMax - XLDay1;
		public const long MonthsPerYear = 12L;
		public const long HoursPerDay = 24L;
		public const long MinutesPerHour = 60L;
		public const long SecondsPerMinute = 60L;
		public const long MinutesPerDay = 1440L;
		public const long SecondsPerDay = 86400L;
		public const long MillisecondsPerSecond = 1000L;
		public const long MillisecondsPerMinute = 60000L;
		public const long MillisecondsPerHour = 3600000L;
		public const long MillisecondsPerDay = 86400000L;
        public const double MinimumTick = 1 / MillisecondsPerDay;
		public const string DefaultFormatStr = "yyyy-MM-dd HH:mm:ss.fff";
		
		public void Assign( int year, int month, int day, int hour, int minute, int second, int millis) {
			_timeStamp = CalendarDateTotimeStamp( year, month, day, hour, minute, second, millis );
		}
		
		public static TimeStamp FromOADate(double value) {
			value *= MillisecondsPerDay;
			return new TimeStamp( (long) value);
		}
		
		private static long DoubleToLong( double value) {
			value *= MillisecondsPerDay;
			value += 0.5;
			return (long) value;
		}
		
		private static double LongToDouble( long value) {
			return value / (double) MillisecondsPerDay;
		}
		
//		#if FIXED
		public Elapsed TimeOfDay {
			get { int other;
				  int hour;
				  int minute;
				  int second;
				  int millis;
				  GetDate(out other,out other,out other,out hour,out minute,out second,out millis);
				  return new Elapsed(millis+
				  		second*MillisecondsPerSecond+
				  		minute*MillisecondsPerMinute+
				  		hour*MillisecondsPerHour);
			}
		}
		
		public int Year {
			get { int other;
				  int year;
				  GetDate(out year,out other,out other,out other,out other,out other,out other);
				  return year;
			}
		}
		
		public WeekDay WeekDay {
			get { return (WeekDay) timeStampToWeekDay(_timeStamp / (double) MillisecondsPerDay); }
		}
		public int Month {
			get { int other;
				  int month;
				  GetDate(out other,out month,out other,out other,out other,out other,out other);
				  return month;
			}
		}
		public int Day {
			get { int other;
				  int day;
				  GetDate(out other,out other,out day,out other,out other,out other,out other);
				  return day;
			}
		}
		public int Hour {
			get { int other;
				  int hour;
				  GetDate(out other,out other,out other,out hour,out other,out other,out other);
				  return hour;
			}
		}
		
		public int Minute {
			get { int other;
				  int minute;
				  GetDate(out other,out other,out other,out other,out minute,out other,out other);
				  return minute;
			}
		}
		
		public int Second {
			get { int other;
				  int second;
				  GetDate(out other,out other,out other,out other,out other,out second,out other);
				  return second;
			}
		}
		
		public int Millisecond {
			get { int other;
				  int millisecond;
				  GetDate(out other,out other,out other,out other,out other,out other,out millisecond);
				  return millisecond;
			}
		}
		
//		#endif
		private static void Synchronize() {
			lock(locker) {
				Thread.CurrentThread.Priority = ThreadPriority.Highest;
	        	lastDateTime = DateTime.UtcNow.Ticks;
	        	long currentDateTime;
        		long frequency = System.Diagnostics.Stopwatch.Frequency;
        		stopWatchFrequency = frequency;
	        	do {
	        		lastStopWatch = System.Diagnostics.Stopwatch.GetTimestamp();
	        		currentDateTime = DateTime.UtcNow.Ticks;
	        	} while( lastDateTime == currentDateTime);
	        	lastDateTime = currentDateTime;
	        	Thread.CurrentThread.Priority = ThreadPriority.Normal;
			}
		}
		private static object locker = new object();
		private static long lastStopWatch;
		private static long stopWatchFrequency = 1L;
		private static long lastDateTime;
		private static long tickFrequency = 10000000L;
		
		public static TimeStamp Parse(string value) {
			return new TimeStamp(value);
		}

		private static DateTime GetAdjustedDateTime() {
        	DateTime nowUtcTime = DateTime.UtcNow;
        	long dateTimeDelta = nowUtcTime.Ticks - lastDateTime;
        	long stopWatchDelta = (System.Diagnostics.Stopwatch.GetTimestamp() - lastStopWatch) * tickFrequency / stopWatchFrequency ;
        	long timeDelta = stopWatchDelta - dateTimeDelta;
        	// Check is system time was change.
        	if( timeDelta > 1000000L || timeDelta < -1000000L) {
        		Synchronize();
        		return new DateTime(lastDateTime);
        	} else {
        		return nowUtcTime.AddTicks(timeDelta);
        	}
		}
		
		public static TimeStamp UtcNow {
			get { 
				DateTime dateTimeNow = GetAdjustedDateTime();
				return new TimeStamp(dateTimeNow.ToOADate());
			}
		}
		
	    private static int Occurrences(string text, char chr)
	    {
	        // Loop through all instances of the string 'text'.
	        int count = 0;
	        int i = 0;
	        while ((i = text.IndexOf(chr, i)) != -1)
	        {
	            i ++;
	            count++;
	        }
	        return count;
	    }
	    
		public TimeStamp( string timeString) {
	    	timeString = timeString.Replace("  "," ").Trim();
			int spaceCount = Occurrences(timeString,' ');
			int hypenCount = Occurrences(timeString,'-');
			int slashCount = Occurrences(timeString,'/');
			char[] dateTimeSeparator;
			char[] dateSeparator;
			if( hypenCount == 2) {
				dateSeparator = new char[] {'-'};
			} else if( slashCount == 2) {
				dateSeparator = new char[] {'/'};
			} else {
				dateSeparator = null;
			}
			if( spaceCount == 1) {
				dateTimeSeparator = new char[] {' '};
			} else if( hypenCount == 1) {
				dateTimeSeparator = new char[] {'-'};
			} else {
				dateTimeSeparator = null;
			}
			string[] strings = dateTimeSeparator != null ? timeString.Split(dateTimeSeparator) : new string[] { timeString };
			string date = strings[0];
			int hour=0, minute=0, second=0, millis=0;
			if( strings.Length > 1) {
				string time = strings[1];
				strings = time.Split(new char[] {':'});
				hour = Convert.ToInt32(strings[0]);
				minute = Convert.ToInt32(strings[1]);
				strings = strings[2].Split(new char[] {'.'});
				second = Convert.ToInt32(strings[0]);
				if( strings.Length>1) {
					millis = Convert.ToInt32(strings[1]);
				}
			}
			if( dateSeparator != null) {
				strings = date.Split(dateSeparator);
			} else {
				if( date.Length == 8) {
					strings = new string[3];
					strings[0] = date.Substring(0,4);
					strings[1] = date.Substring(4,2);
					strings[2] = date.Substring(6,2);
				} else {
					throw new ApplicationException("Unknown date separator for " + timeString);
				}
			}
			int year = Convert.ToInt32(strings[0]);
			int month = Convert.ToInt32(strings[1]);
			int day = Convert.ToInt32(strings[2]);
			if( day > 1000) {
				month = Convert.ToInt32(strings[0]);
				day = Convert.ToInt32(strings[1]);
				year = Convert.ToInt32(strings[2]);
			}
			_timeStamp = CalendarDateTotimeStamp( year, month, day, hour, minute, second, millis );
		}
		
		public TimeStamp( long timeStamp )
		{
			_timeStamp = timeStamp;
		}
		
		public TimeStamp( double timeStamp )
		{
			_timeStamp = DoubleToLong(timeStamp);
		}
		public TimeStamp( DateTime dateTime )
		{
			_timeStamp = CalendarDateTotimeStamp( dateTime.Year, dateTime.Month,
							dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second,
							dateTime.Millisecond );
		}
		public TimeStamp( int year, int month, int day )
		{
			_timeStamp = CalendarDateTotimeStamp( year, month, day, 0, 0, 0 );
		}
		public TimeStamp( int year, int month, int day, int hour, int minute, int second )
		{
			_timeStamp = CalendarDateTotimeStamp( year, month, day, hour, minute, second );
		}
//		public TimeStamp( int year, int month, int day, int hour, int minute, int second )
//		{
//			_timeStamp = CalendarDateTotimeStamp( year, month, day, hour, minute, second );
//		}
		public TimeStamp( int year, int month, int day, int hour, int minute, int second, int millisecond )
		{
			_timeStamp = CalendarDateTotimeStamp( year, month, day, hour, minute, second, millisecond );
		}
		public TimeStamp( TimeStamp rhs )
		{
			_timeStamp = rhs._timeStamp;
		}
	
		public long Internal
		{
			get { return _timeStamp; }
			set { _timeStamp = value; }
		}
		
		public double ToOADate() {
			return _timeStamp / (double) MillisecondsPerDay;
		}

		public bool IsValidDate
		{
			get { return _timeStamp >= DoubleDayMin && _timeStamp <= DoubleDayMax; }
		}
		
		public DateTime DateTime
		{
			get { return timeStampToDateTime( _timeStamp / (double) MillisecondsPerDay ); }
			set { _timeStamp = DateTimeTotimeStamp( value ); }
		}
		
		public double JulianDay
		{
			get { return timeStampToJulianDay( _timeStamp ); }
			set { _timeStamp = DoubleToLong(JulianDayTotimeStamp( value )); }
		}
		
		public double DecimalYear
		{
			get { return timeStampToDecimalYear( _timeStamp ); }
			set { _timeStamp = DecimalYearTotimeStamp( value ); }
		}
	
		private static bool CheckValidDate( double timeStamp )
		{
			return timeStamp >= DoubleDayMin && timeStamp <= DoubleDayMax;
		}

		public static double MakeValidDate( double timeStamp )
		{
			if ( timeStamp < DoubleDayMin )
				timeStamp = DoubleDayMin;
			if ( timeStamp > DoubleDayMax )
				timeStamp = DoubleDayMax;
			return timeStamp;
		}
		
		public void GetDate( out int year, out int month, out int day )
		{
			int hour, minute, second;
			
			timeStampToCalendarDate( _timeStamp, out year, out month, out day, out hour, out minute, out second );
		}
		
		public void GetDate( out int year, out int month, out int day,
						out int hour, out int minute, out int second, out int millis)
		{
			timeStampToCalendarDate( _timeStamp / (double) MillisecondsPerDay, out year, out month, out day, out hour, out minute, out second, out millis );
		}
		
		public void SetDate( int year, int month, int day )
		{
			_timeStamp = CalendarDateTotimeStamp( year, month, day, 0, 0, 0 );
		}
		
		public void GetDate( out int year, out int month, out int day,
						out int hour, out int minute, out int second )
		{
			timeStampToCalendarDate( _timeStamp, out year, out month, out day, out hour, out minute, out second );
		}

		public void SetDate( int year, int month, int day, int hour, int minute, int second )
		{
			_timeStamp = CalendarDateTotimeStamp( year, month, day, hour, minute, second );
		}
		
		public double GetDayOfYear()
		{
			return timeStampToDayOfYear( _timeStamp );
		}

		public int GetDayOfWeek()
		{
			return timeStampToWeekDay( _timeStamp / (double) MillisecondsPerDay);
		}
		
		public static long CalendarDateTotimeStamp( int year, int month, int day,
			int hour, int minute, int second, int millisecond )
		{
			// Normalize the data to allow for negative and out of range values
			// In this way, setting month to zero would be December of the previous year,
			// setting hour to 24 would be the first hour of the next day, etc.
			//double dsec = second + (double) millisecond / MillisecondsPerSecond;
			int ms = millisecond;
			NormalizeCalendarDate( ref year, ref month, ref day, ref hour, ref minute, ref second,
						ref ms );
		
			return _CalendarDateTotimeStamp( year, month, day, hour, minute, second, ms );
		}
		
		public static long CalendarDateTotimeStamp( int year, int month, int day,
			int hour, int minute, int second )
		{
			// Normalize the data to allow for negative and out of range values
			// In this way, setting month to zero would be December of the previous year,
			// setting hour to 24 would be the first hour of the next day, etc.
			int ms = 0;
			NormalizeCalendarDate( ref year, ref month, ref day, ref hour, ref minute,
					ref second, ref ms );
		
			return _CalendarDateTotimeStamp( year, month, day, hour, minute, second, ms );
		}
		
//		public static long CalendarDateTotimeStamp( int year, int month, int day,
//			int hour, int minute, int second )
//		{
//			// Normalize the data to allow for negative and out of range values
//			// In this way, setting month to zero would be December of the previous year,
//			// setting hour to 24 would be the first hour of the next day, etc.
//			int sec = (int)second;
//			int ms = ( second - sec ) * MillisecondsPerSecond;
//			NormalizeCalendarDate( ref year, ref month, ref day, ref hour, ref minute, ref sec,
//					ref ms );
//		
//			return _CalendarDateTotimeStamp( year, month, day, hour, minute, sec, ms );
//		}
		
		public static long CalendarDateToJulianDay( int year, int month, int day,
			int hour, int minute, int second )
		{
			// Normalize the data to allow for negative and out of range values
			// In this way, setting month to zero would be December of the previous year,
			// setting hour to 24 would be the first hour of the next day, etc.
			int ms = 0;
			NormalizeCalendarDate( ref year, ref month, ref day, ref hour, ref minute,
				ref second, ref ms );
		
			double value = _CalendarDateToJulianDay( year, month, day, hour, minute, second, ms );
			return DoubleToLong(value);
		}
		
		public static double CalendarDateToJulianDay( int year, int month, int day,
			int hour, int minute, int second, int millisecond )
		{
			// Normalize the data to allow for negative and out of range values
			// In this way, setting month to zero would be December of the previous year,
			// setting hour to 24 would be the first hour of the next day, etc.
			int ms = millisecond;

			NormalizeCalendarDate( ref year, ref month, ref day, ref hour, ref minute,
						ref second, ref ms );
		
			return _CalendarDateToJulianDay( year, month, day, hour, minute, second, ms );
		}

//		public void RoundTime()
//        {
//	    	long numberOfTicks = (long)( (_timeStamp * MillisecondsPerDay) + 0.5);
//            _timeStamp = numberOfTicks * MinimumTick;
//		}
	    
	    private static void NormalizeCalendarDate( ref int year, ref int month, ref int day,
											ref int hour, ref int minute, ref int second,
											ref int millisecond )
		{
			// Normalize the data to allow for negative and out of range values
			// In this way, setting month to zero would be December of the previous year,
			// setting hour to 24 would be the first hour of the next day, etc.

			// Normalize the milliseconds and carry over to seconds
			int carry = (int)Math.Floor( millisecond / (double) MillisecondsPerSecond );
			millisecond -= carry * (int)MillisecondsPerSecond;
			second += carry;

			// Normalize the seconds and carry over to minutes
			carry = (int)Math.Floor( (double) second / SecondsPerMinute );
			second -= carry * (int)SecondsPerMinute;
			minute += carry;
		
			// Normalize the minutes and carry over to hours
			carry = (int) Math.Floor( (double) minute / MinutesPerHour );
			minute -= carry * (int) MinutesPerHour;
			hour += carry;
		
			// Normalize the hours and carry over to days
			carry = (int) Math.Floor( (double) hour / HoursPerDay );
			hour -= carry * (int) HoursPerDay;
			day += carry;
		
			// Normalize the months and carry over to years
			carry = (int) Math.Floor( (double) month / MonthsPerYear );
			month -= carry * (int) MonthsPerYear;
			year += carry;
		}
		
		private static long _CalendarDateTotimeStamp( int year, int month, int day, int hour,
					int minute, int second, int millisecond )
		{
		double timeStamp = _CalendarDateToJulianDay( year, month, day, hour, minute,
	                                          second, millisecond );
		double value = JulianDayTotimeStamp( timeStamp  );
			return DoubleToLong(value);
		}
		
		private static double _CalendarDateToJulianDay( int year, int month, int day, int hour,
					int minute, int second, int millisecond )
		{
			// Taken from http://www.srrb.noaa.gov/highlights/sunrise/program.txt
			// routine calcJD()
		
			if ( month <= 2 )
			{
				year -= 1;
				month += 12;
			}
		
			double A = Math.Floor( (double) year / 100.0 );
			double B = 2 - A + Math.Floor( A / 4.0 );
		
			double value = Math.Floor( 365.25 * ( (double) year + 4716.0 ) ) +
					Math.Floor( 30.6001 * (double) ( month + 1 ) ) +
					(double) day + B - 1524.5 +
					(double) hour  / HoursPerDay + (double) minute / MinutesPerDay + (double) second / SecondsPerDay +
					(double) millisecond / MillisecondsPerDay;
			
			return value;
		
		}

		public static void timeStampToCalendarDate( double timeStamp, out int year, out int month,
			out int day, out int hour, out int minute, out int second )
		{
			double jDay = timeStampToJulianDay( timeStamp );
			
			JulianDayToCalendarDate( jDay, out year, out month, out day, out hour,
				out minute, out second );
		}
		
		public static void timeStampToCalendarDate( double timeStamp, out int year, out int month,
			out int day, out int hour, out int minute, out int second, out int millisecond )
		{
			double jDay = timeStampToJulianDay( timeStamp );
			int ms;
			JulianDayToCalendarDate( jDay, out year, out month, out day, out hour,
				out minute, out second, out ms );
			millisecond = (int)ms;
		}
		
		public static void timeStampToCalendarDate( double timeStamp, out int year, out int month,
			out int day, out int hour, out int minute, out double second )
		{
			double jDay = timeStampToJulianDay( timeStamp );
			
			JulianDayToCalendarDate( jDay, out year, out month, out day, out hour,
				out minute, out second );
		}
		
		public static void JulianDayToCalendarDate( double jDay, out int year, out int month,
			out int day, out int hour, out int minute, out int second )
		{
			int ms = 0;

			JulianDayToCalendarDate( jDay, out year, out month,
					out day, out hour, out minute, out second, out ms );
		}

		public static void JulianDayToCalendarDate( double jDay, out int year, out int month,
			out int day, out int hour, out int minute, out double second )
		{
			int sec;
			int ms;

			JulianDayToCalendarDate( jDay, out year, out month,
					out day, out hour, out minute, out sec, out ms );

			second = sec + ms / MillisecondsPerSecond;
		}

		public static void JulianDayToCalendarDate( double jDay, out int year, out int month,
			out int day, out int hour, out int minute, out int second, out int millisecond )
		{
			// add 5 ten-thousandths of a second to the day fraction to avoid roundoff errors
			jDay += 0.0005 / SecondsPerDay;

			double z = Math.Floor( jDay + 0.5);
			double f = jDay + 0.5 - z;
		
			double alpha = Math.Floor( ( z - 1867216.25 ) / 36524.25 );
			double A = z + 1.0 + alpha - Math.Floor( alpha / 4 );
			double B = A + 1524.0;
			double C = Math.Floor( ( B - 122.1 ) / 365.25 );
			double D = Math.Floor( 365.25 * C );
			double E = Math.Floor( ( B - D ) / 30.6001 );
		
			day = (int) Math.Floor( B - D - Math.Floor( 30.6001 * E ) + f );
			month = (int) ( ( E < 14.0 ) ? E - 1.0 : E - 13.0 );
			year = (int) ( ( month > 2 ) ? C - 4716 : C - 4715 );
		
			double fday =  ( jDay - 0.5 ) - Math.Floor( jDay - 0.5 );
		
			fday = ( fday - (long) fday ) * HoursPerDay;
			hour = (int) fday;
			fday = ( fday - (long) fday ) * MinutesPerHour;
			minute = (int) fday;
			fday = ( fday - (long) fday ) * SecondsPerMinute;
			second = (int) fday;
			fday = ( fday - (long) fday ) * MillisecondsPerSecond;
			millisecond = (int) fday;
		}
		
		public static double timeStampToJulianDay( double timeStamp )
		{
			return timeStamp + XLDay1;
		}
		
		public static double JulianDayTotimeStamp( double jDay )
		{
			return jDay - XLDay1;
		}
		
		public static double timeStampToDecimalYear( double timeStamp )
		{
			int year, month, day, hour, minute, second;
			
			timeStampToCalendarDate( timeStamp, out year, out month, out day, out hour, out minute, out second );
			
			double jDay1 = CalendarDateToJulianDay( year, 1, 1, 0, 0, 0 );
			double jDay2 = CalendarDateToJulianDay( year + 1, 1, 1, 0, 0, 0 );
			double jDayMid = CalendarDateToJulianDay( year, month, day, hour, minute, second );
			
			
			return (double) year + ( jDayMid - jDay1 ) / ( jDay2 - jDay1 );
		}
		
		public static long DecimalYearTotimeStamp( double yearDec )
		{
			int year = (int) yearDec;
			
			long jDay1 = CalendarDateToJulianDay( year, 1, 1, 0, 0, 0 );
			long jDay2 = CalendarDateToJulianDay( year + 1, 1, 1, 0, 0, 0 );
			
			long jDay = (long) (( yearDec - (double) year ) * ( jDay2 - jDay1 ) + jDay1);
			double value = JulianDayTotimeStamp( jDay );
			return DoubleToLong(value);
		}
		
		public static double timeStampToDayOfYear( double timeStamp )
		{
			int year, month, day, hour, minute, second;
			timeStampToCalendarDate( timeStamp, out year, out month, out day,
									out hour, out minute, out second );
			return timeStampToJulianDay( timeStamp ) - CalendarDateToJulianDay( year, 1, 1, 0, 0, 0 ) + 1.0;
		}
		
		public static int timeStampToWeekDay( double timeStamp )
		{
			return (int) ( timeStampToJulianDay( timeStamp ) + 1.5 ) % 7;
		}
		
		public static DateTime timeStampToDateTime( double timeStamp )
		{
			int year, month, day, hour, minute, second, millisecond;
			timeStampToCalendarDate( timeStamp, out year, out month, out day,
									out hour, out minute, out second, out millisecond );
			return new DateTime( year, month, day, hour, minute, second, millisecond );
		}
		
		public static long DateTimeTotimeStamp( DateTime dt )
		{
			return CalendarDateTotimeStamp( dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second,
										dt.Millisecond );
		}

		public void Sync() {
			int year, month, day, hour, minute, second, millisecond;
 		    GetDate(out year,out month,out day,out hour,out minute,out second,out millisecond);
			
			Assign(year,month,day,hour,minute,second,millisecond);
		}
		
		public void AddMilliseconds( long dMilliseconds )
		{
			_timeStamp += dMilliseconds;
		}

		public void AddSeconds( long dSeconds )
		{
			_timeStamp += dSeconds * MillisecondsPerSecond;
		}

		public void AddMinutes( long dMinutes )
		{
			_timeStamp += dMinutes * MillisecondsPerMinute;
		}
		
		public void AddHours( long dHours )
		{
			_timeStamp += dHours * MillisecondsPerHour;
		}
		
		public void AddDays( long dDays )
		{
			_timeStamp += dDays * MillisecondsPerDay;
		}
		
		public void AddMonths( int dMonths )
		{
			int iMon = (int) dMonths;
			double monFrac = Math.Abs( dMonths - (double) iMon );
			int sMon = Math.Sign( dMonths );
			
			int year, month, day, hour, minute, second;
			
			timeStampToCalendarDate( LongToDouble(_timeStamp), out year, out month, out day, out hour, out minute, out second );
			if ( iMon != 0 )
			{
				month += iMon;
				_timeStamp = CalendarDateTotimeStamp( year, month, day, hour, minute, second );
			}
			
			if ( sMon != 0 )
			{
				long timeStamp2 = CalendarDateTotimeStamp( year, month+sMon, day, hour, minute, second );
				_timeStamp += (long) ((timeStamp2 - _timeStamp) * monFrac);
			}
		}
		
		public void AddYears( double dYears )
		{
			int iYear = (int) dYears;
			double yearFrac = Math.Abs( dYears - (double) iYear );
			int sYear = Math.Sign( dYears );
			
			int year, month, day, hour, minute, second;
			
			timeStampToCalendarDate( LongToDouble(_timeStamp), out year, out month, out day, out hour, out minute, out second );
			if ( iYear != 0 )
			{
				year += iYear;
				_timeStamp = CalendarDateTotimeStamp( year, month, day, hour, minute, second );
			}
			
			if ( sYear != 0 )
			{
				long timeStamp2 = CalendarDateTotimeStamp( year+sYear, month, day, hour, minute, second );
				_timeStamp += (long) ((timeStamp2 - _timeStamp) * yearFrac);
			}
		}

		public static Elapsed operator -( TimeStamp lhs, TimeStamp rhs )
		{
			return new Elapsed( lhs._timeStamp - rhs._timeStamp);
		}
		
		public static TimeStamp operator -( TimeStamp lhs, Elapsed rhs )
		{
			lhs._timeStamp -= rhs.Internal;
			return lhs;
		}
		
		public static TimeStamp operator +( TimeStamp lhs, Elapsed rhs )
		{
			lhs._timeStamp += rhs.Internal;
			return lhs;
		}
		
//		public static explicit operator TimeStamp( long timeStamp)
//		{
//			return new TimeStamp(timeStamp);
//		}
		
//		public static explicit operator long( TimeStamp TimeStamp )
//		{
//			return TimeStamp._timeStamp;
//		}
//		
//		public static explicit operator double( TimeStamp TimeStamp )
//		{
//			return (double)TimeStamp._timeStamp / MillisecondsPerDay;
//		}
		
		public static explicit operator DateTime( TimeStamp TimeStamp )
		{
			
			return timeStampToDateTime( TimeStamp.ToOADate());
		}
		
		public static explicit operator TimeStamp( DateTime dt )
		{
			
			return new TimeStamp( DateTimeTotimeStamp( dt ) / (double) MillisecondsPerDay );
		}
		
		public static bool operator >=( TimeStamp lhs, TimeStamp rhs)
		{
			return lhs.CompareTo(rhs) >= 0;
		}
		
		public static bool operator ==( TimeStamp lhs, TimeStamp rhs)
		{
			return lhs.CompareTo(rhs) == 0;
		}
		
		public static bool operator !=( TimeStamp lhs, TimeStamp rhs)
		{
			return lhs.CompareTo(rhs) != 0;
		}
		
		public static bool operator <=( TimeStamp lhs, TimeStamp rhs)
		{
			return lhs.CompareTo(rhs) <= 0;
		}
		
		public static bool operator >( TimeStamp lhs, TimeStamp rhs)
		{
			return lhs.CompareTo(rhs) > 0;
		}
		
		public static bool operator <( TimeStamp lhs, TimeStamp rhs)
		{
			return lhs.CompareTo(rhs) < 0;
		}
		
		public override bool Equals( object obj )
		{
			if ( obj is TimeStamp )
			{
				return CompareTo((TimeStamp) obj)==0;
			}
			else if ( obj is long )
			{
				return ((long) obj) == _timeStamp;
			}
			else
				return false;
		}
		
		public override int GetHashCode()
		{
			return _timeStamp.GetHashCode();
		}

		public int CompareTo( TimeStamp target )
		{
			long value = _timeStamp - target._timeStamp;
			return value == 0 ? 0 : value > 0 ? 1 : -1;
		}

		public string ToString( double timeStamp )
		{
			return ToString( timeStamp, DefaultFormatStr );
		}
		
		public override string ToString()
		{
			return ToString( _timeStamp / (double) MillisecondsPerDay, DefaultFormatStr );
		}
		
		public string ToString( string fmtStr )
		{
			return ToString( this._timeStamp / (double) MillisecondsPerDay, fmtStr );
		}
		
		public void Add( Elapsed elapsed) {
			_timeStamp += elapsed.Internal;
		}

		public static string ToString( double timeStamp, string _fmtStr )
		{
			int	year, month, day, hour, minute, second, millisecond;

			StringBuilder fmtStr = new StringBuilder(_fmtStr);
			if ( !CheckValidDate( timeStamp ) )
				return "Date Error";

			timeStampToCalendarDate( timeStamp, out year, out month, out day, out hour, out minute,
											out second, out millisecond );
			fmtStr.Replace( "yyyy", year.ToString("d4") );
			fmtStr.Replace( "MM", month.ToString("d2") );
			fmtStr.Replace( "dd", day.ToString("d2") );
			if ( year <= 0 )
			{
				year = 1 - year;
				fmtStr.Append(" (BC)");
			}

			fmtStr.Replace( "HH", hour.ToString("d2") );
			fmtStr.Replace( "mm", minute.ToString("d2") );
			fmtStr.Replace( "ss", second.ToString("d2") );
			fmtStr.Replace( "fff", ((int)millisecond).ToString("d3") );
			
//			if ( _fmtStr.IndexOf("d") >= 0 )
//			{
//				fmtStr = fmtStr.Replace( "dd", ((int) timeStamp).ToString("d2") );
//				fmtStr = fmtStr.Replace( "d", ((int) timeStamp).ToString("d") );
//				timeStamp -= (int) timeStamp;
//			}
//			if ( _fmtStr.IndexOf("h") >= 0 )
//			{
//				fmtStr = fmtStr.Replace( "hh", ((int) (timeStamp * 24)).ToString("d2") );
//				fmtStr = fmtStr.Replace( "h", ((int) (timeStamp * 24)).ToString("d") );
//				timeStamp = ( timeStamp * 24 - (int) (timeStamp * 24) ) / 24.0;
//			}
//			if ( _fmtStr.IndexOf("m") >= 0 )
//			{
//				fmtStr = fmtStr.Replace( "mm", ((int) (timeStamp * 1440)).ToString("d2") );
//				fmtStr = fmtStr.Replace( "m", ((int) (timeStamp * 1440)).ToString("d") );
//				timeStamp = ( timeStamp * 1440 - (int) (timeStamp * 1440) ) / 1440.0;
//			}
//			if ( _fmtStr.IndexOf("s") >= 0 )
//			{
//				fmtStr = fmtStr.Replace( "ss", ((int) (timeStamp * 86400)).ToString("d2") );
//				fmtStr = fmtStr.Replace( "s", ((int) (timeStamp * 86400)).ToString("d") );
//				timeStamp = ( timeStamp * 86400 - (int) (timeStamp * 86400) ) / 86400.0;
//			}
//			if ( _fmtStr.IndexOf("f") >= 0 ) {
//				fmtStr = fmtStr.Replace( "fffff", ((int) (timeStamp * 8640000000)).ToString("d") );
//				fmtStr = fmtStr.Replace( "ffff", ((int) (timeStamp * 864000000)).ToString("d") );
//				fmtStr = fmtStr.Replace( "fff", ((int) (timeStamp * 86400000)).ToString("d") );
//				fmtStr = fmtStr.Replace( "ff", ((int) (timeStamp * 8640000)).ToString("d") );
//				fmtStr = fmtStr.Replace( "f", ((int) (timeStamp * 864000)).ToString("d") );
//			}

			return fmtStr.ToString();
		}
	}
}
