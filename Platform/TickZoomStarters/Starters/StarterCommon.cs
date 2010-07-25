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
using TickZoom.Properties;

namespace TickZoom.Starters
{
    
    public delegate void ProgressCallback(string fileName, Int64 BytesRead, Int64 TotalBytes);
    
	/// <summary>
	/// Description of Test.
	/// </summary>
	public abstract class StarterCommon : Starter
	{
		static readonly Log log = Factory.Log.GetLogger(typeof(StarterCommon));
		static readonly bool debug = log.IsDebugEnabled;
		BackgroundWorker backgroundWorker;
	    ShowChartCallback showChartCallback;
	    CreateChartCallback createChartCallback;
	    TickQueue realTimeTickQueue;
    	string dataFolder = "DataCache";
   		static Interval initialInterval = Factory.Engine.DefineInterval(BarUnit.Day,1);
   		long endCount = long.MaxValue;
   		int startCount = 0;
	    static object callBackLocker = new object();
	    ProjectProperties projectProperties = new ProjectPropertiesCommon();
	    string projectFile = Factory.Settings["AppDataFolder"] + @"\portfolio.tzproj";
		private List<Provider> tickProviders = new List<Provider>();
	    string fileName;
	    string storageFolder;
		Dictionary<ModelInterface,Dictionary<string,object>> optimizeValueMap = new Dictionary<ModelInterface, Dictionary<string, object>>();
		int taskCount=0;
		ProgressImpl progress = new ProgressImpl();
		
		public StarterCommon() : this(true) {
    		storageFolder = Factory.Settings["AppDataFolder"];
   			if( storageFolder == null) {
       			throw new ApplicationException( "Must set AppDataFolder property in app.config");
   			}
    		fileName = storageFolder + @"\Statistics\optimizeResults.csv";
		}
		
		public StarterCommon(bool releaseEngineCache) {
			if( releaseEngineCache) {
				Factory.Engine.Dispose();
				Factory.Provider.Release();
			}
			string dataFolder = Factory.Settings["DataFolder"];
			if( dataFolder != null) {
				this.dataFolder = dataFolder;
			}
		}
		
		public void Release() {
			Factory.Engine.Dispose();
			Factory.Provider.Release();
		}
		
		bool CancelPending {
			get { if( backgroundWorker != null) {
					return backgroundWorker.CancellationPending;
				} else {
					return false;
				}
			}
		}
		
		public void ReportProgress( string text, Int64 current, Int64 final) {
			lock( callBackLocker) {
	    		if( backgroundWorker!= null && !backgroundWorker.CancellationPending) {
					progress.UpdateProgress(text,current,final);
	    			backgroundWorker.ReportProgress(0, progress);
	    		}
	    	}
		}
		
		public virtual Provider[] SetupProviders(bool quietMode, bool singleLoad) {
			List<Provider> senderList = new List<Provider>();
			SymbolInfo[] symbols = ProjectProperties.Starter.SymbolProperties;
			string[] symbolFiles = projectProperties.Starter.SymbolArray;
			for(int i=0; i<symbols.Length; i++) {
	    		TickReader tickReader = Factory.TickUtil.TickReader();
	    		tickReader.MaxCount = EndCount;	
	    		tickReader.StartTime = ProjectProperties.Starter.StartTime;
	    		tickReader.EndTime = ProjectProperties.Starter.EndTime;
	    		tickReader.BackgroundWorker = BackgroundWorker;
	    		tickReader.LogProgress = true;
	    		tickReader.BulkFileLoad = singleLoad;
	    		tickReader.QuietMode = quietMode;
	    		try { 
		    		tickReader.Initialize(DataFolder,symbolFiles[i]);
					senderList.Add(tickReader);
	    		} catch( System.IO.FileNotFoundException ex) {
	    			throw new ApplicationException("Error: " + ex.Message);
	    		}
			}
			return senderList.ToArray();
		}
		
		public Provider[] SetupDataProviders() {
			try {
				List<Provider> senderList = new List<Provider>();
				SymbolInfo[] symbols = ProjectProperties.Starter.SymbolProperties;
				for(int i=0; i<symbols.Length; i++) {
					Provider provider = Factory.Provider.RemoteProvider();
					senderList.Add(provider);
				}
				return senderList.ToArray();
			} catch( Exception ex) {
				log.Error("Setup failed.", ex);
				throw;
			}
		}
		
		public virtual TickQueue[] SetupTickWriters() {
			List<TickQueue> queueList = new List<TickQueue>();
			SymbolInfo[] symbols = ProjectProperties.Starter.SymbolProperties;
			for(int i=0; i<symbols.Length; i++) {
				TickWriter tickWriter = Factory.TickUtil.TickWriter(false);
	    		tickWriter.Initialize(DataFolder,symbols[i].Symbol);
				queueList.Add(tickWriter.WriteQueue);
			}
			return queueList.ToArray();
		}
	    
		public abstract void Run(ModelInterface model);
		
		private static readonly string projectFileLoaderCategory = "TickZOOM";
		private static readonly string projectFileLoaderName = "Project File";
		public virtual void Run() {
			ModelLoaderInterface loader = Plugins.Instance.GetLoader(projectFileLoaderCategory + ": " + projectFileLoaderName);
			projectProperties = ProjectPropertiesCommon.Create(new StreamReader(ProjectFile));
			Run(loader);
		}
		
		public virtual void Run(ModelLoaderInterface loader) {
			if( loader.Category == projectFileLoaderCategory && loader.Name == projectFileLoaderName ) {
				log.Notice("Loading project from " + ProjectFile);
				projectProperties = ProjectPropertiesCommon.Create(new StreamReader(ProjectFile));
			}
			loader.OnInitialize(ProjectProperties);
			loader.OnLoad(ProjectProperties);
			if( !SetOptimizeValues(loader)) {
				throw new ApplicationException("Error, setting optimize variables.");
			}
			ModelInterface model = loader.TopModel;
			Run( model);
		}
		
	    public bool SetOptimizeValues(ModelLoaderInterface loader) {
			// Then set them for logging separately to optimization reports.
			Dictionary<string,object> optimizeValues = new Dictionary<string,object>();
			for( int i = 0; i < loader.Variables.Count; i++) {
				optimizeValues[loader.Variables[i].Name] = loader.Variables[i].Value;
			}
			
	    	bool retVal = true;
	    	foreach( var kvp in optimizeValues) {
	    		string name = kvp.Key;
	    		object value = kvp.Value;
	    		string[] namePairs = kvp.Key.Split('.');
	    		if( namePairs.Length < 2) {
	    			log.Error("Sorry, the optimizer variable '" + kvp.Key + "' was not found.");
	    			retVal = false;
	    			continue;
	    		}
	    		string strategyName = namePairs[0];
	    		StrategyInterface strategy = null;
	    		foreach( StrategyInterface tempStrategy in GetStrategyList(loader.TopModel.Chain.Tail)) {
	    			if( tempStrategy.Name.CompareWildcard(strategyName,false)) {
	    				strategy = tempStrategy;
			    		if( !SetOptimizeValue(strategy,name,value)) {
			    			retVal = false;
			    			break;
			    		}
	    			}
	    		}
	    		if( strategy == null) {
	    			log.Error("Sorry, a strategy matching '" + strategyName + "' was never found for optimizer variable '" + kvp.Key + "'.");
	    			retVal = false;
	    			continue;
	    		}
	    	}
			optimizeValueMap.Add(loader.TopModel,optimizeValues);
	    	return retVal;
	    }
		
		private bool SetOptimizeValue(StrategyInterface strategy, string name, object value) {
    		string[] namePairs = name.Split('.');
			string propertyName = namePairs[1];
			PropertyDescriptorCollection props = TypeDescriptor.GetProperties(strategy);
			PropertyDescriptor property = null;
			for( int i = 0; i < props.Count; i++) {
				PropertyDescriptor tempProperty = props[i];
				if( tempProperty.Name == propertyName) {
					property = tempProperty;
					break;
				}
	    	}
			if( property == null) {
    			log.Error("Sorry, the optimizer variable '" + name + "' was not found.");
    			return false;
			}
			if( namePairs.Length == 2) {
				if( !SetProperty(strategy,property,value)) {
					log.Error("Sorry, the value '" + value + "' isn't valid for optimizer variable '" + name + "'.");
	    			return false;
				}
			} else if( namePairs.Length == 3) {
				StrategyInterceptor strategySupport = property.GetValue(strategy) as StrategyInterceptor;
				if( strategySupport == null) {
	    			log.Error("Sorry, the optimizer variable '" + name + "' was not found.");
	    			return false;
				}
				string strategySupportName = namePairs[1];
				propertyName = namePairs[2];
				props = TypeDescriptor.GetProperties(strategySupport);
				property = null;
				for( int i = 0; i < props.Count; i++) {
					PropertyDescriptor tempProperty = props[i];
					if( tempProperty.Name == propertyName) {
						property = tempProperty;
						break;
					}
		    	}
				if( property == null) {
	    			log.Error("Sorry, the optimizer variable '" + name + "' was not found.");
	    			return false;
				}
				if( !SetProperty(strategySupport,property,value)) {
					log.Error("Sorry, the value '" + value + "' isn't valid for optimizer variable '" + name + "'.");
	    			return false;
				}
			}
			return true;
		}
		
		private bool SetProperty( object target, PropertyDescriptor property, object value) {
			Type type = property.PropertyType;
			TypeConverter convert = TypeDescriptor.GetConverter(type);
	        if (!convert.IsValid(value)) {
				return false;
	        }
			object convertedValue = convert.ConvertFrom(value);
			property.SetValue(target,convertedValue);
			return true;
		}
		
   		internal IEnumerable<StrategyInterface> GetStrategyList(Chain chain) {
   			StrategyInterface currentStrategy = chain.Model as StrategyInterface;
   			if( currentStrategy != null) {
   				yield return currentStrategy;
   			}
   			foreach( Chain tempChain in chain.Dependencies) {
   				foreach( StrategyInterface strategy in GetStrategyList(tempChain.Tail)) {
   					yield return strategy;
   				}
   			}
   			if( chain.Previous.Model != null) {
   				foreach( StrategyInterface strategy in GetStrategyList(chain.Previous)) {
   					yield return strategy;
   				}
   			}
   		}
		
		public void WriteEngineResults(ModelLoaderInterface loader, List<TickEngine> engines) {
			for( int i=0; i<engines.Count; i++) {
				WriteEngineResult(loader,engines[i]);
			}
		}

		bool headerWritten = false;
		public void WriteEngineResult(ModelLoaderInterface loader, TickEngine engine) {
    		try {
				loader.OptimizeOutput = new FileStream(fileName,FileMode.Append);
				using( StreamWriter fwriter = new StreamWriter(loader.OptimizeOutput)) {
					ModelInterface topModel = engine.Model;
					foreach( var chain in topModel.Chain.Dependencies) {
						StrategyInterface passModel = chain.Model as StrategyInterface;
						if( passModel == null) {
							log.Error("Model " + passModel + " must implement the StrategyInterface or PortfolioInterface for the optimizer statistics results to get recorded.");
						} else {
							WritePassStats(fwriter, passModel);
						}
					}
				}
    		} catch( Exception ex) {
    			log.Error("ERROR: Problem writing optimizer results.", ex);
    		}
		}

		public void WritePassStats(StreamWriter fwriter, StrategyInterface passModel) {
			Dictionary<string,object> optimizeValues = optimizeValueMap[passModel];
			if( !headerWritten) {
				headerWritten = true;
				string header = passModel.OnGetOptimizeHeader(optimizeValues);
				if( string.IsNullOrEmpty(header)) {
					log.Warn("The header string returned from OnGetOptimizeResults for " + passModel.Name + " was empty or null.");
				} else {
					fwriter.WriteLine(header);
				}
			}
			string stats = passModel.OnGetOptimizeResult(optimizeValues);
			if( stats != null && stats.Length > 0) {
				fwriter.WriteLine(stats);
			}
		}

		public ModelInterface ProcessLoader(ModelLoaderInterface loader) {
			taskCount++;
	    	loader.OnClear();
			loader.OnLoad(ProjectProperties);
			ModelInterface topModel = loader.TopModel;
			if( !SetOptimizeValues(loader)) {
				throw new ApplicationException("Error, setting optimize variables.");
			}
			topModel.Name += "-Pass-" + taskCount;
	    	return topModel;
		}
		
		public TickEngine ProcessHistorical(ModelInterface topModel, bool quietMode) {
			TickEngine engine = Factory.Engine.TickEngine;
			ProjectProperties.Engine.CopyProperties(engine);
			engine.Model = topModel;
			engine.ChartProperties = ProjectProperties.Chart;
			engine.SymbolInfo = ProjectProperties.Starter.SymbolProperties;
			
			engine.IntervalDefault = ProjectProperties.Starter.IntervalDefault;
			engine.EnableTickFilter = ProjectProperties.Engine.EnableTickFilter;
			
			engine.Providers = SetupProviders(quietMode,false);
			engine.BackgroundWorker = BackgroundWorker;
			engine.RunMode = RunMode.Historical;
			engine.StartCount = StartCount;
			engine.EndCount = EndCount;
			engine.StartTime = ProjectProperties.Starter.StartTime;
			engine.EndTime = ProjectProperties.Starter.EndTime;
	
			if( quietMode) {
				engine.QuietMode = true;
			} else {
		    	engine.ShowChartCallback = ShowChartCallback;
				engine.CreateChartCallback = CreateChartCallback;
			}
	
			engine.QueueTask();
			return engine;
		}
		
		public ShowChartCallback ShowChartCallback {
			get { return showChartCallback; }
			set { showChartCallback = value; }
		}
    	
		public BackgroundWorker BackgroundWorker {
			get { return backgroundWorker; }
			set { backgroundWorker = value; }
		}

		public string DataFolder {
			get { return dataFolder; }
			set { dataFolder = value; }
		}
    	
		public long EndCount {
			get { return endCount; }
			set { endCount = value; }
		}
   		
		public int StartCount {
			get { return startCount; }
			set { startCount = value; }
		}
		
		public TickQueue RealTimeTickQueue {
			get { return realTimeTickQueue; }
			set { realTimeTickQueue = value; }
		}
		
		public ProjectProperties ProjectProperties {
			get { return projectProperties; }
			set { projectProperties = value; }
		}
   		
		public CreateChartCallback CreateChartCallback {
			get { return createChartCallback; }
			set { createChartCallback = value; }
		}
		
		public string ProjectFile {
			get { return projectFile; }
			set { projectFile = value; }
		}
		
		public Provider[] DataFeeds {
			get { return tickProviders.ToArray(); }
			set { tickProviders = new List<Provider>(value); }
		}
		
		public void AddProvider(Provider provider)
		{
			tickProviders.Add(provider);
		}
		
		public abstract void Wait();
		
	    
		public string FileName {
			get { return fileName; }
			set { fileName = value; }
		}		
		
	}
}
