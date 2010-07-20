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
using System.ComponentModel;

namespace TickZoom.Api
{
    public delegate void ShowChartCallback();
    
    public delegate Chart CreateChartCallback();

    public interface Starter {
    	
		BackgroundWorker BackgroundWorker {
			get;
			set;
		}
		
		ShowChartCallback ShowChartCallback {
			get;
			set;
		}
    	
		CreateChartCallback CreateChartCallback {
			get;
			set;
		}
   		
    	string ProjectFile {
			get;
			set;
		}
    	
    	TickQueue RealTimeTickQueue {
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
		
		string DataFolder {
			get;
			set;
		}
    	
		void Run();
		void Run(ModelInterface model);
		void Run(ModelLoaderInterface loader);
		
		void Wait();
		
		ProjectProperties ProjectProperties {
			get;
			set;
		}
		
		void Release();
	}
}
