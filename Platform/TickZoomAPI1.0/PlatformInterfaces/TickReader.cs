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
using System.ComponentModel;

namespace TickZoom.Api
{
	public interface TickReader : Provider
	{
		void StartSymbol(Receiver receiver, SymbolInfo symbol, object eventDetail);
		void StopSymbol(Receiver receiver, SymbolInfo symbol);
		TickQueue ReadQueue { get; }
		int StartCount { get; set; }
		TimeStamp StartTime { get; set; }
		TimeStamp EndTime { get; set; }
		void Initialize(string _folder, SymbolInfo symbolInfo);
		void Initialize(string folderOrfile, string symbolFile);
		void Initialize(string fileName);
		TickIO GetLastTick();
		void Start(Receiver receiver);
		void Stop(Receiver receiver);
		bool IsAtStart(TickBinary tick);
		bool IsAtEnd(TickBinary tick);
		byte DataVersion { get; }
		BackgroundWorker BackgroundWorker { get; set; }
		Elapsed SessionStart { get; set; }
		Elapsed SessionEnd { get; set; }
		bool ExcludeSunday { get; set; }
		string FileName { get; }
		SymbolInfo Symbol { get; }
		bool LogProgress { get; set; }
		long MaxCount { get; set; }
		bool QuietMode { get; set; }
		bool BulkFileLoad { get; set; }
		TickIO LastTick { get; }
		void CloseAll();
	}
}
