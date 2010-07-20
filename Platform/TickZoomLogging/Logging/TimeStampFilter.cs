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
using log4net.Core;
using TickZoom.Api;
using log4net.Filter;

namespace TickZoom.Logging
{
	/// <summary>
	/// Description of TimeStampFilter.
	/// </summary>
	public class TimeStampFilter : FilterSkeleton
	{
		private string beginTimeStr;
		private string endTimeStr;
		
		private TimeStamp beginTime;
		private TimeStamp endTime;
		
		private void ConvertTime() {
			beginTime = TimeStamp.MinValue;
			if( beginTimeStr != null) {
				beginTimeStr = beginTimeStr.Trim();
				if( beginTimeStr.Length > 0) {
					beginTime = new TimeStamp(beginTimeStr);
				}
			}
			endTime = TimeStamp.MaxValue;
			if( endTimeStr != null) {
				endTimeStr = endTimeStr.Trim();
				if( endTimeStr.Length > 0) {
					endTime = new TimeStamp(endTimeStr);
				}
			}
		}

		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			string timeStampStr = (string) loggingEvent.Properties["TimeStamp"];
			if( timeStampStr != null && timeStampStr.Length>0) {
				TimeStamp timeStamp = new TimeStamp(timeStampStr);
				if( timeStamp >= beginTime && timeStamp <= endTime) {
					return FilterDecision.Neutral;
				}
				else
				{
					return FilterDecision.Deny;
				}
			} else {
				return FilterDecision.Accept;
			}
		}
		
		public string BeginTime {
			get { return beginTimeStr; }
			set { beginTimeStr = value; 
				ConvertTime(); }
		}
		
		public string EndTime {
			get { return endTimeStr; }
			set { endTimeStr = value;
				ConvertTime(); }
		}
	}
}
