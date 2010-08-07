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
	public class Debug {
		[Conditional("DEBUG")]
		public static void Assert( Log log, bool condition) {
			if( !condition) {
				throw new Exception("assertion failed");
			}
		}
	}

	public interface Log 
    {
        [Obsolete("Use Notice method instead. ")]
        void WriteLine(string msg);
        
        [Obsolete("Log4Net handles this internally.")]
        void Clear();
        
        [Obsolete("Use Info or Debug methods instead. See those for more info.")]
        void WriteFile(string msg);
        
        bool HasLine {
        	get;
        }
        
        LogEvent ReadLine();
        
 		string FileName {
			get;
			set;
		}

        void Assert(bool test);
        
        TimeStamp TimeStamp {
        	get;
        	set;
        }
        
        void Indent();
        
		void Outdent();
        
		/* Test if a level is enabled for logging */
		bool IsTraceEnabled { get; }
		bool IsDebugEnabled { get; }
		bool IsInfoEnabled { get; }
		bool IsNoticeEnabled { get; }
		bool IsWarnEnabled { get; }
		bool IsErrorEnabled { get; }
		bool IsFatalEnabled { get; }
		
		/* Log a message object */
        void Trace(object message);
		void Debug(object message);
		void Info(object message);
		void Notice(object message);
		void Warn(object message);
		void Error(object message);
		void Fatal(object message);
		
		/* Log a message object and exception */
        void Trace(object message, Exception t);
		void Debug(object message, Exception t);
		void Info(object message, Exception t);
		void Notice(object message, Exception t);
		void Warn(object message, Exception t);
		void Error(object message, Exception t);
		void Fatal(object message, Exception t);
		
		/* Log a message string using the System.String.Format syntax */
		void TraceFormat(string format, params object[] args);
		void DebugFormat(string format, params object[] args);
		void InfoFormat(string format, params object[] args);
		void NoticeFormat(string format, params object[] args);
		void WarnFormat(string format, params object[] args);
		void ErrorFormat(string format, params object[] args);
		void FatalFormat(string format, params object[] args);
		
		/* Log a message string using the System.String.Format syntax */
		void TraceFormat(IFormatProvider provider, string format, params object[] args);
		void DebugFormat(IFormatProvider provider, string format, params object[] args);
		void InfoFormat(IFormatProvider provider, string format, params object[] args);
		void NoticeFormat(IFormatProvider provider, string format, params object[] args);
		void WarnFormat(IFormatProvider provider, string format, params object[] args);
		void ErrorFormat(IFormatProvider provider, string format, params object[] args);
		void FatalFormat(IFormatProvider provider, string format, params object[] args);
 	}
}
