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
using System.Threading;
using TickZoom.Api;

namespace TickZoom.TickUtil
{

	/// <summary>
	/// Description of TickArray.
	/// </summary>
	public class TickWriterDefault : TickWriter
	{
		BackgroundWorker backgroundWorker;
   		int maxCount = 0;
   		SymbolInfo symbol = null;
		string fileName = null;
		Thread appendThread = null;
		protected TickQueue writeQueue;
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(TickWriter));
		private static readonly bool debug = log.IsDebugEnabled;
		private static readonly bool trace = log.IsTraceEnabled;
		bool keepFileOpen = false;
		bool eraseFileToStart = false;
		bool logProgress = false;
		FileStream fs = null;
		MemoryStream memory = null;
		bool isInitialized = false;
		bool isPaused = false;
		string storageFolder;
		Progress progress = new Progress();
		
		public TickWriterDefault(bool eraseFileToStart)
		{
			this.eraseFileToStart = eraseFileToStart;
			writeQueue = Factory.TickUtil.TickQueue(typeof(TickWriter));
			writeQueue.StartEnqueue = Start;
       		storageFolder = Factory.Settings["AppDataFolder"];
       		if( storageFolder == null) {
       			throw new ApplicationException( "Must set AppDataFolder property in app.config");
       		}
		}
		
		public void Start() {
			
		}
		
		public void Pause() {
			log.Notice("Disk I/O for " + symbol + " is temporarily paused.");
			isPaused = true;	
		}
		
		public void Resume() {
			log.Notice("Disk I/O for " + symbol + " has resumed.");
			isPaused = false;
		}
		
		bool CancelPending {
			get { return backgroundWorker !=null && backgroundWorker.CancellationPending; }
		}
		
		public void Initialize(string folderOrfile, string _symbol) {
			SymbolInfo symbolInfo = Factory.Symbol.LookupSymbol(_symbol);
			symbol = Factory.Symbol.LookupSymbol(_symbol);
			if( Directory.Exists(storageFolder + @"\" + folderOrfile)) {
				fileName = storageFolder + @"\" + folderOrfile + @"\" + symbol.Symbol.StripInvalidPathChars() + ".tck";
			} else {
       			fileName = folderOrfile;
			}
    		log.Notice("TickWriter fileName: " + fileName);
			Directory.CreateDirectory( Path.GetDirectoryName(FileName));
			if( eraseFileToStart) {
    			File.Delete( fileName);
    			log.Notice("TickWriter file was erased to begin writing.");
    		}
			if( keepFileOpen) {
    			fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
   				log.Debug("keepFileOpen - Open()");
    			memory = new MemoryStream();
			}
     		if( !CancelPending ) {
				StartAppendThread();
			}
			isInitialized = true;
		}
		
		[Obsolete("Please pass string symbol instead of SymbolInfo.",true)]
		public void Initialize(string _folder, SymbolInfo _symbol) {
			isInitialized = false;
		}
		
		[Obsolete("Please call Initialize( folderOrfile, symbo) instead.",true)]
		public void Initialize(string filePath) {
			isInitialized = false;
		}

		protected virtual void StartAppendThread() {
			string baseName = Path.GetFileNameWithoutExtension(fileName);
	        appendThread = new Thread(AppendDataLoop);
	        appendThread.Name = baseName + " writer";
	        appendThread.IsBackground = true;
	        appendThread.Start();
		}
		
		TickBinary tick = new TickBinary();
		TickIO tickIO = new TickImpl();
		protected virtual void AppendDataLoop() {
			try { 
				while( AppendData());
			} catch( Exception ex) {
				log.Error( ex.GetType() + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
			}
		}
		
		protected virtual bool AppendData() {
			try {
				while( isPaused) {
					Thread.Sleep(1);
				}
				if( writeQueue.Count > 0) {
					if( !keepFileOpen) {
		    			fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
		    			if( trace) log.Trace("!keepFileOpen - Open()");
		    			memory = new MemoryStream();
					}
					while( writeQueue.TryDequeue(ref tick)) {
						tickIO.Inject(tick);
						if( trace) {
							log.Trace("Writing to file: " + tickIO);
						}
						WriteToFile(memory, tickIO);
					}
					if( !keepFileOpen) {
			    		fs.Close();
			    		if( trace) log.Trace("!keepFileOpen - Close()");
			    		fs = null;
			    	}
				} else {
					Thread.Sleep(1);
				}
	    		return true;
		    } catch (QueueException ex) {
				if( ex.EntryType == EventType.Terminate) {
					log.Debug("Exiting, queue terminated.");
					if( fs != null) {
						fs.Close();
	    				log.Debug("Terminate - Close()");
					}
					return false;
				} else {
					Exception exception = new ApplicationException("Queue returned unexpected: " + ex.EntryType);
					writeQueue.Terminate(exception);
					throw ex;
				}
			} catch( Exception ex) {
				writeQueue.Terminate(ex);
				if( fs != null) {
					fs.Close();
				}
				throw;
    		}
		}
		
		private void SetupHeader( MemoryStream memory) {
			memory.GetBuffer()[0] = (byte) memory.Length;
		}
		
		private int origSleepSeconds = 3;
		private int currentSleepSeconds = 3;
		private void WriteToFile(MemoryStream memory, ReadWritable<TickBinary> tick) {
			int errorCount = 0;
			int count=0;
			do {
			    try { 
			    	tick.Compress(memory);
			    	fs.Write(memory.GetBuffer(),0,(int)memory.Position);			    	
			    	memory.Position = 0;
		    		if( errorCount > 0) {
				    	log.Notice(symbol + ": Retry successful."); 
		    		}
		    		errorCount = 0;
		    		currentSleepSeconds = origSleepSeconds;
			    } catch(IOException e) { 
	    			errorCount++;
			    	log.Debug(symbol + ": " + e.Message + "\nPausing " + currentSleepSeconds + " seconds before retry."); 
			    	Factory.Parallel.Sleep(3000);
			    } 
				count++;
			} while( errorCount > 0);
		}
		
		public void Add(TickIO tick) {
			while( !TryAdd(tick)) {
				Thread.Sleep(1);
			}
		}
		
		public bool TryAdd(TickIO tickIO) {
			if( !isInitialized) {
				throw new ApplicationException("Please initialized TickWriter first.");
			}
			TickBinary tick = tickIO.Extract();
			return writeQueue.TryEnqueue(ref tick);
		}
		
		[Obsolete("Please discontinue use of CanReceive() and simple check the return value of TryAdd() instaed to find out if the add was succesful.",true)]
		public bool CanReceive {
			get {
				return true;
			}
		}
		
		public virtual void Close() {
			if( !isInitialized) {
				throw new ApplicationException("Please initialized TickWriter first.");
			}
			if( debug) log.Debug("Entering Close()");
    		if( appendThread != null && writeQueue != null) {
				while( !writeQueue.TryEnqueue(EventType.Terminate, symbol)) {
					Thread.Sleep(1);
				}
				appendThread.Join();
			}
			if( keepFileOpen && fs!=null ) {
	    		fs.Close();
	    		log.Debug("keepFileOpen - Close()");
	    		fs = null;
	    	}
			if( debug) log.Debug("Exiting Close()");
		}
		
		public bool LogTicks = false;
		
		void progressCallback( string text, Int64 current, Int64 final) {
			if( backgroundWorker != null && backgroundWorker.WorkerReportsProgress) {
				progress.UpdateProgress(text,current,final);
				backgroundWorker.ReportProgress(0, progress);
			}
		}
		
 		public BackgroundWorker BackgroundWorker {
			get { return backgroundWorker; }
			set { backgroundWorker = value; }
		}
		
		public string FileName {
			get { return fileName; }
		}
	    
		public SymbolInfo Symbol {
			get { return symbol; }
		}
		
		public bool LogProgress {
			get { return logProgress; }
			set { logProgress = value; }
		}
   		
		public int MaxCount {
			get { return maxCount; }
			set { maxCount = value; }
		}
		
		public bool KeepFileOpen {
			get { return keepFileOpen; }
			set { keepFileOpen = value; }
		}
		
		public TickQueue WriteQueue {
			get { return writeQueue; }
		}
	}
}
