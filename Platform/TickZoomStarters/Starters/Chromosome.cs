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
using System.Text;
using TickZoom.Api;

namespace TickZoom.Starters
{
	/// <summary>
	/// Description of Chromosome.
	/// </summary>
	public class Chromosome : IComparable
	{
		Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		int[] genome;
		bool fitnessAssigned = false;
		double fitness=0;
		static Random random = new Random();

		public Chromosome(int[] genome)
		{
			this.genome = new int[genome.Length];
			Array.Copy(genome, this.genome,genome.Length);
		}
		public Chromosome(Chromosome other)
		{
			genome = other.genome;
			fitness=0;
		}
		public void SetSeed(int seed) {
			random = new Random(seed);
		}
		
		public int[] Genome {
			get { return genome; }
		}

		public double Fitness {
			get { return fitness; }
			set { log.Debug("Fitness Assigned: " + value);
				  fitnessAssigned = true;
				  fitness = value; }
		}
		
		public void CrossOver( Chromosome other) {
			int crossover = random.Next(genome.Length);
			CrossOver(other,crossover);
		}
		
		void CrossOver( Chromosome other,int crossover) {
			int[] newGenome1 = new int[genome.Length];
			int[] newGenome2 = new int[genome.Length];
			
			Array.Copy(genome,0,newGenome1,0,crossover);
			Array.Copy(other.genome,crossover,newGenome1,crossover,genome.Length-crossover);
			
			Array.Copy(other.genome,0,newGenome2,0,crossover);
			Array.Copy(genome,crossover,newGenome2,crossover,genome.Length-crossover);
			
			genome = newGenome1;
			other.genome = newGenome2;
		}
		
		public void DoubleCrossOver( Chromosome other) {
			int crossover = random.Next(genome.Length);
			int crossover2;
			do { 
				crossover2 = random.Next(genome.Length);
			} while( genome.Length > 1 && crossover == crossover2);
			CrossOver(other,crossover);
			CrossOver(other,crossover2);
		}
		
		public override bool Equals(object obj)
		{
			Chromosome other = (Chromosome) obj;
			for(int i=0; i<genome.Length; i++) {
				if( genome[i] != other.genome[i]) {
					return false;
				}
			}
			return true;
		}
		
		public override int GetHashCode()
		{
			string retVal = "";
			for(int i=0; i<genome.Length; i++) {
				retVal += genome[i].ToString();
			}
			return retVal.GetHashCode();
		}
		
		public bool FitnessAssigned {
			get { return fitnessAssigned; }
		}
		public override string ToString()
		{
			string retVal = "";
			for( int i=0; i<genome.Length; i++) {
				retVal += "[" + genome[i].ToString().PadLeft(3) + "] ";
			}
			
			return retVal +  Math.Round(fitness,2) + " ";
		}
		
		int IComparable.CompareTo(object obj)
		{
			Chromosome other = (Chromosome) obj;
			return fitness < other.fitness ? 1 : fitness > other.fitness ? -1 : 0;
		}
	}
}
