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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

using TickZoom.Api;
using TickZoom.Common;
using TickZoom.TickUtil;

namespace TickZoom.Starters
{
	/// <summary>
	/// Description of Test.
	/// </summary>
	public class GeneticStarter : StarterCommon
	{
		int generationCount = 4;
		int totalTasks=0;
		int tasksRemaining=0;
		List<Chromosome> generation;
		List<Chromosome> alreadyTried;
		Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		List<TickEngine> engineIterations = new List<TickEngine>();
		ModelLoaderInterface loader;
		List<ModelProperty> optimizeVariables;
		
		public GeneticStarter() {
			random = new Random();
		}
		
		/// <summary>
		/// This method only useful for testing since
		/// setting a fix random seed will force the optimizer
		/// to always have the same results for the input and
		/// therefore eliminate the randomness.
		/// </summary>
		/// <param name="randomSeed"></param>
		public void SetRandomSeed(int randomSeed) {
			random = new Random(randomSeed);
		}
		
		public static List<int> GetIndexes(ModelProperty var) {
			List<int> list = new List<int>(var.Count);
			for( int i = 0; i<var.Count; i++) {
				list.Add(i);
			}
			return list;
		}
		
		public static double GetValue( ModelProperty var, int index) {
			return var.Start + (index * var.Increment);
		}
		
		public static List<int> GetRandomIndexes(ModelProperty var) {
			List<int> input = GetIndexes(var);
			List<int> randomList = new List<int>(input.Count);
			while( input.Count > 0) {
				int i = random.Next(input.Count);
				randomList.Add(input[i]);
				input.RemoveAt(i);
			}
			return randomList;
		}
		
		public override void Run(ModelInterface model) {
			throw new MustUseLoaderException("Must set only ModelLoader instead of Model for Genetic Optimization");
		}
		
		public override void Run(ModelLoaderInterface loader)
		{
			this.loader = loader;

    		try {
    			if( loader.OptimizeOutput == null) {
		    		Directory.CreateDirectory( Path.GetDirectoryName(FileName));
		    		File.Delete(FileName);
    			}
    		} catch( Exception ex) {
    			log.Error("Error while creating directory and deleting '" + FileName + "'.",ex);
    			return;
    		}
			
			log.Notice( "Beginning Genetic Optimize of: ");
			log.Notice( loader.Name + " model loader. Type: " + loader.GetType().Name);
			loader.QuietMode = true;
			
			loader.OnInitialize(ProjectProperties);
			
			optimizeVariables = new List<ModelProperty>();
			for( int i=0; i<loader.Variables.Count; i++) {
				ModelProperty var = loader.Variables[i];
				if( var.Optimize) {
					optimizeVariables.Add(var);
				}
			}
			
			// Get Total Number of Bits
			int totalBits=0;
			for( int i=0; i<optimizeVariables.Count; i++) {
				ModelProperty var = optimizeVariables[i];
				int bits = Convert.ToString(var.Count-1,2).Length;
				totalBits+=bits;
			}
			
			// Get the highest count.
			int populationCount = 0;
			for(int i=0; i<optimizeVariables.Count; i++) {
				ModelProperty var = optimizeVariables[i];
				populationCount = var.Count > populationCount ? var.Count : populationCount;
			}
			if(optimizeVariables.Count == 1) {
				generationCount = 1;
			} 
			
			totalTasks = populationCount * generationCount;
			tasksRemaining = totalTasks;

			log.Notice( "Assigning genomes.");
			
			// Create initial set of random chromosomes.
			generation = new List<Chromosome>();
			// This list assures we never retry a previous one twice.
			alreadyTried = new List<Chromosome>();
			
			// Create a genome holder.
			int[] genome = new int[optimizeVariables.Count];

			// Indexes for going through randomList
			int[] indexes = new int[optimizeVariables.Count];

			for( int repeat=0; repeat < Math.Min(optimizeVariables.Count,2); repeat++) {
				
				//Get random values for each.
				List<List<int>> randomLists = new List<List<int>>();
				for( int i=0; i< optimizeVariables.Count; i++) {
					randomLists.Add( GetRandomIndexes(optimizeVariables[i]));
				}
				
				// Create initial population
				for(int loop=0; loop<populationCount; loop++) {
				
					// Set the genome from the randomLists using the indexes.
					for( int i=0; i<optimizeVariables.Count; i++) {
						genome[i] = randomLists[i][indexes[i]];
					}
					
					Chromosome chromosome = new Chromosome( genome);
					log.Debug( chromosome.ToString() );
					generation.Add( chromosome);
					alreadyTried.Add( chromosome);
					for(int i = 0; i<indexes.Length; i++) {
						indexes[i]++;
						ModelProperty var = optimizeVariables[i];
						if( indexes[i] >= var.Count) {
							indexes[i] = 0;
						}
					}
				}
			}

			#if CLRPROFILER
	        CLRProfilerControl.LogWriteLine("Entering Genetic Loop"); 
	        CLRProfilerControl.AllocationLoggingActive = true;
			CLRProfilerControl.CallLoggingActive = false;
			#endif
			
			for( int genCount =0; genCount < generationCount && !CancelPending; genCount++) {
				
				ModelInterface topModel = new Portfolio();
				// Assign fitness values
				for( int i=generation.Count-1; i>=0;i--) {
					Chromosome chromosome = generation[i];
					if( !chromosome.FitnessAssigned ) {
						ModifyVariables( chromosome);
						ModelInterface model = ProcessLoader(loader);
						topModel.Chain.Dependencies.Add(model.Chain);
					} else {
						tasksRemaining--;
						log.Debug("Saves processing on " + chromosome + "!");
					}
				}
				TickEngine engine = ProcessHistorical(topModel,true);
				engineIterations.Add(engine);

				// Let threads all finish to calculate total fitness.
				ReportProgress( "Optimizing...", 0, totalTasks);

				GetEngineResults();

				WriteEngineResults(loader,engineIterations);
				
				engineIterations.Clear();				
				
				ReportProgress( "Optimizing Complete", totalTasks-tasksRemaining, totalTasks);
				
				generation.Sort();
				
				log.Notice("After sorting generation...");
				double maxFitness = 0;
				for(int i=0; i<generation.Count; i++) {
					log.Debug( generation[i].ToString() );
					maxFitness = Math.Max(generation[i].Fitness,maxFitness);
				}
				// If none of the genes in the chromosome
				// had a positive fitness, stop here.
				if( maxFitness <= 0) { break; }
				
				List<Chromosome> newGeneration = new List<Chromosome>();
				log.Notice("Crossover starting...");
				while( newGeneration.Count < populationCount) {

					Chromosome chromo1 = Roulette();
					Chromosome chromo2;
					do { 
						chromo2 = Roulette();
					} while( chromo2.Equals(chromo1));
					
					log.Debug("Before: " + chromo1 + " - " + chromo2);
					chromo1.DoubleCrossOver(chromo2);
					log.Debug("After: " + chromo1 + " - " + chromo2);
					
					if( alreadyTried.Contains(chromo1)) {
						chromo1 = alreadyTried[alreadyTried.IndexOf(chromo1)];
					} else {
						alreadyTried.Add(chromo1);
					}
					if( alreadyTried.Contains(chromo2)) {
						chromo2 = alreadyTried[alreadyTried.IndexOf(chromo2)];
					} else {
						alreadyTried.Add(chromo2);
					}
					newGeneration.Add(chromo1);
					newGeneration.Add(chromo2);
				}
				generation = newGeneration;
			}

			GetEngineResults();
			
			WriteEngineResults(loader,engineIterations);
			
			engineIterations.Clear();				
			
			ReportProgress( "Optimizing Complete", totalTasks-tasksRemaining, totalTasks);
			
			#if CLRPROFILER
	        CLRProfilerControl.AllocationLoggingActive = false;
			CLRProfilerControl.CallLoggingActive = false;
	        CLRProfilerControl.LogWriteLine("Exiting Genetic Loop"); 
	        #endif
			
			log.Notice("Genetic Algorithm Finished.");
		}
		
		public override void Wait() {
			// finishes during run
		}
		
		private void GetEngineResults() {
			for( int i=0; i<engineIterations.Count; i++) {
				TickEngine engine = engineIterations[i];
				engine.WaitTask();
				generation[i].Fitness = engine.Fitness;
				#if CLRPROFILER
		        CLRProfilerControl.LogWriteLine(tasksRemaining + " tasks remaining"); 
		        #endif
		        --tasksRemaining;
				ReportProgress( "Optimizing...", totalTasks-tasksRemaining, totalTasks);
			}
		}
	    
		static Random random;
		Chromosome Roulette() {
			// Needs generation in sorted order by Most Fit at position 0.
			double totalFitness = 0;
			for( int i=0; i<generation.Count;i++) {
				totalFitness += generation.Count-1 - i;
//				totalFitness += generation[i].Fitness;
			}
			
			double slice = random.NextDouble() * totalFitness;
			double fitnessSoFar = 0;
			int j=0;
			for(; j<generation.Count; j++) {
//				fitnessSoFar += generation[j].Fitness;
				fitnessSoFar += generation.Count-1 - j;
				if( fitnessSoFar >= slice) {
					return new Chromosome( generation[j]);
				}
			}
			return new Chromosome(generation[j-1]);
			
		}
		
		void ModifyVariables( Chromosome chromosome) {
			// Convert chromosome to values to run a test.
			for( int i=0; i<optimizeVariables.Count; i++) {
				double val = GetValue( optimizeVariables[i], chromosome.Genome[i]);
				optimizeVariables[i].Value = val.ToString();
			}
		}
		
		private bool CancelPending {
			get { if( BackgroundWorker != null) {
					return BackgroundWorker.CancellationPending;
				} else {
					return false;
				}
			}
		}
		
	}
}
