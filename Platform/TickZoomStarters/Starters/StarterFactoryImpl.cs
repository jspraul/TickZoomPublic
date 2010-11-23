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
using System.IO;
using System.Reflection;

using TickZoom.Api;
using TickZoom.Properties;
using TickZoom.Symbols;

namespace TickZoom.Starters
{
	/// <summary>
	/// Description of SymbolFactory.
	/// </summary>
	[Diagram(AttributeExclude=true)]
	public class StarterFactoryImpl : StarterFactory
	{
		public ModelProperty ModelProperty(string name,string value,double start,double end,double increment,bool isActive)
		{
			return new ModelPropertyCommon(name,value,start,end,increment,isActive);
		}
		/// <summary>
		/// Contructs a new Historical Starter for running a historical 
		/// test pass. 
		/// </summary>
		/// <param name="releaseResources">
		/// Pass false for the Starter it to leave the memory resources from
		/// the last Starter. Pass true for the Starter to release
		/// all memory resources from any previous Starter before beginning.
		/// NOTE: If you pass false, then your code must
		/// call Factory.Engine.Release() to release memory to
		/// avoid a memory leak.
		/// </param>
		/// <returns></returns>
		public Starter HistoricalStarter(bool releaseResources) {
			return new HistoricalStarter(releaseResources);
		}
		/// <summary>
		/// Contructs a new Historical Starter for running a historical 
		/// test pass. Releases all memory resources upon completion.
		/// </summary>
		/// <returns></returns>
		public Starter HistoricalStarter() {
			return new HistoricalStarter();
		}
		
		public ISymbolProperties SymbolProperties() {
			return new SymbolProperties();
		}
		
		public ProjectProperties ProjectProperties(string path) {
			return ProjectPropertiesCommon.Create(new StreamReader(path));
		}
		
		public Starter DesignStarter() {
			return new DesignStarter();
		}
		
		public Starter GeneticStarter() {
			return new GeneticStarter();
		}
		
		public Starter RealTimeStarter() {
			return new RealTimeStarter();
		}
		
		public Starter OptimizeStarter() {
			return new OptimizeStarter();
		}
		
		public Starter FIXSimulatorStarter() {
			return new FIXSimulatorStarter();
		}
		
		public Starter CreateStarter( string starterName) {
			var starterType = Type.GetType("TickZoom.Starters."+starterName);
			var assembly = starterType.Assembly;
			return (Starter) assembly.CreateInstance(starterType.FullName);
		}

    }
}
