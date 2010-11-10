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
using System.IO;

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Starters
{
	/// <summary>
	/// Description of Test.
	/// </summary>
	public class GeneticStarter : StarterCommon
	{
		int generationCount = 4;
		int populationCount;
		int totalPasses=200;
		int tasksRemaining=0;
		List<Chromosome> generation;
		List<Chromosome> alreadyTried;
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
		
		public List<int> GetRandomIndexes(ModelProperty var) {
			var input = GetIndexes(var);
			var randomList = new List<int>(populationCount);
			if( input.Count > populationCount) {
				var increment = (double) input.Count / (double) populationCount;
				for( var index = 0D; index < input.Count; index += increment) {
					var i = (int) index;
					randomList.Add(input[i]);
				}
			} else {
				for( int i = 0; i<populationCount; ) {
					for( int j = 0; j < input.Count && i< populationCount; j++, i++) {
						randomList.Add(input[j]);
					}
				}
			}
			
			Shuffle( randomList);
			
			return randomList;
		}
		
		private void Shuffle( List<int> list) {
			for( int i = 0; i< list.Count; i++) {
				int j;
				// avoid self-swap
				while( (j = random.Next( list.Count)) == i);
				// swap
				var temp = list[i];
				list[i] = list[j];
				list[j] = temp;
			}
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
			
			if(optimizeVariables.Count == 1) {
				generationCount = 1;
			} 
			
			// Get the highest count.
			populationCount = totalPasses / generationCount;
			tasksRemaining = totalPasses;
			
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
						if( indexes[i] >= populationCount) {
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
			
			int totalEngineCount = Environment.ProcessorCount * generationCount;
				
			// Pre-setup engines. This causes the progress
			// bar to show a complete set of information for all
			// generations.
			var engines = new Stack<TickEngine>();
			for( int i=0; i<totalEngineCount; i++) {
				engines.Push( SetupEngine( true));
			}
			
			for( int genCount =0; genCount < generationCount && !CancelPending; genCount++) {
				
				// Assign fitness values
				var topModels = new List<ModelInterface>();
				for( int i=generation.Count-1; i>=0;i--) {
					Chromosome chromosome = generation[i];
					if( !chromosome.FitnessAssigned ) {
						ModifyVariables( chromosome);
						var model = ProcessLoader(loader,i);
						topModels.Add(model);
					} else {
						tasksRemaining--;
						log.Debug("Saves processing on " + chromosome + "!");
					}
				}
				
				int tasksPerEngine = CalculateTasksPerEngine(topModels.Count);
				
				ModelInterface topModel = new Portfolio();
				int passCount = 0;
				foreach( var model in topModels) {
						topModel.Chain.Dependencies.Add(model.Chain);
					passCount++;
					if (passCount % tasksPerEngine == 0)
					{
						var engine = engines.Pop();
					    engine.Model = topModel;
						engine.QueueTask();
						engineIterations.Add(engine);
						topModel = new Portfolio();
						if (engineIterations.Count >= Environment.ProcessorCount) {
							ProcessIteration();
						}
					}
				}
					
				if (topModel.Chain.Dependencies.Count > 0)
				{
					TickEngine engine = ProcessHistorical(topModel, true);
					engine.QueueTask();
					engineIterations.Add(engine);
				}
				
				if( engineIterations.Count > 0) {
					ProcessIteration();
				}
				
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
			
			#if CLRPROFILER
	        CLRProfilerControl.AllocationLoggingActive = false;
			CLRProfilerControl.CallLoggingActive = false;
	        CLRProfilerControl.LogWriteLine("Exiting Genetic Loop"); 
	        #endif
			
			log.Notice("Genetic Algorithm Finished.");
		}
		
		private void OnSetFitness(int index, double fitness) {
			generation[index].Fitness = fitness;
		}
		
		public void ProcessIteration() {

			GetEngineResults();
			
			WriteEngineResults(loader,engineIterations,OnSetFitness);

			engineIterations.Clear();

			Release();
		}
		
		public override void Wait() {
			// finishes during run
		}
		
		private void GetEngineResults() {
			for( int i=0; i<engineIterations.Count; i++) {
				TickEngine engine = engineIterations[i];
				engine.WaitTask();
				#if CLRPROFILER
		        CLRProfilerControl.LogWriteLine(tasksRemaining + " tasks remaining"); 
		        #endif
		        --tasksRemaining;
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
				
		public int TotalPasses {
			get { return totalPasses; }
			set { totalPasses = value; }
		}
	}
}
