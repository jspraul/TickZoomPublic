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
	/// <summary>
	/// Description of TickEngine.
	/// </summary>
	public interface TickEngine 
	{
		ShowChartCallback ShowChartCallback {
			get;
			set;
		}
		
		CreateChartCallback CreateChartCallback {
			get;
			set;
		}
		
		BackgroundWorker BackgroundWorker {
			get;
			set;
		}
		
		ModelInterface Model {
			get;
			set;
		}
		
		RunMode RunMode {
			get;
			set;
		}
		
 		bool EnableTickFilter {
			get;
			set;
		}
		
		Provider[] Providers {
			get;
			set;
		}
		
		SymbolInfo[] SymbolInfo {
			get;
			set;
		}
		
		ChartProperties ChartProperties {
			get;
			set;
		}
		
		Interval IntervalDefault {
			get;
			set;
		}
		
		[Obsolete("Use OnGetFitness() instead.")]
		FitnessAware FitnessCallback {
			get;
			set;
		}
		
		double Fitness {
			get;
		}
		
		string OptimizeHeader {
			get;
		}
		
		string OptimizeResult {
			get;
		}
		
		ReportWriter ReportWriter {
			get;
			set;
		}
		
    	/// <summary>
    	/// System tests use StartCount to constrain the number
    	/// of ticks read during the test. You may find this useful
    	/// for your tests but never outside of testing.
    	/// </summary>
    	int StartCount {
    		get;
    		set;
    	}
    	
    	/// <summary>
    	/// System tests use EndCount to constrain the number
    	/// of ticks read during the test. You may find this useful
    	/// for your tests but never outside of testing.
    	/// </summary>
    	long EndCount {
    		get;
    		set;
    	}
    	
    	void QueueTask();
		
		int BreakAtBar {
			get;
			set;
		}
    	
    	int OptimizePass {
			get;
			set;
		}
    	
    	int MaxBarsBack {
    		get;
    		set;
    	}
    		
    	int MaxTicksBack {
    		get;
    		set;
    	}
    		
		int TickReplaySpeed {
			get;
			set;
		}
		
		int BarReplaySpeed {
			get;
			set;
		}
    	
    	void WaitTask();
    	
    	void Run();
    	
    	/// <summary>
    	/// Forces all information logging to file. Especially used by
    	/// optimize starters to log status of the optimizing instead of
    	/// the repetitive logging of each historical test pass.
    	/// </summary>
    	bool QuietMode {
    		get;
    		set;
    	}
    	
    	/// <summary>
    	/// Used in automated tests to disable real time output
    	/// like writing statistics and updating charts for every tick
    	/// for faster performance during simulation of real time 
    	/// environment for integration test. Default is true.
    	/// Setting to false disables real time output.
    	/// </summary>
    	bool RealtimeOutput {
    		get;
    		set;
    	}
    	
    	long TickCount {
    		get;
    	}
    	
    	bool IsFree {
    		get;
    	}
    	
    	TimeStamp StartTime {
    		get;
    		set;
    	}
    	
    	TimeStamp EndTime {
    		get;
    		set;
    	}
    	
    	bool SimulateRealTime {
    		get;
    		set;
    	}
    	
	}
}
