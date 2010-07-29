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
using System.IO;
using System.Text;
using System.Threading;

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Transactions
{
	internal class TransactionPairsPages {
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(TransactionPairsPages));
		PagePool<BinaryPage> pagePool;
		private BinaryStore tradeData;
		
		private object dirtyLocker = new object();
		List<BinaryPage> dirtyPages = new List<BinaryPage>();
		public struct Page {
			public long Offset;
			public int Id;
			public Page(long offset, int id) {
				this.Offset = offset;
				this.Id = id;
			}
		}
		Dictionary<int,Page> offsets = new Dictionary<int,Page>();
		
		internal TransactionPairsPages(BinaryStore tradeData, Func<BinaryPage> constructor) {
			this.tradeData = tradeData;
			this.pagePool = new PagePool<BinaryPage>(constructor);
			if( asynchronous) {
				this.tradeData.AddPageWriter( PageWriter);
			}
		}
		
		internal void TryRelease(BinaryPage page) {
			bool contains = false;
			lock( dirtyLocker) {
				contains = dirtyPages.Contains(page);
				if( page != null && !contains) {
					try { 
						pagePool.Free(page);
					} catch( Exception ex) {
						throw new ApplicationException("Failure while freeing page: " + page.PageNumber + "(" + page.Id +") :\n" + this, ex);
					}
				}
			}
		}
		
		private bool TryGetPage(int pageNumber, out BinaryPage binaryPage) {
			Page page;
			lock( dirtyLocker) {
				foreach( var dirtyPage in dirtyPages) {
					if( dirtyPage.PageNumber == pageNumber) {
						binaryPage = dirtyPage;
						pagePool.AddReference(binaryPage);
						return true;
					}
				}
				if( !offsets.TryGetValue(pageNumber,out page)) {
					binaryPage = null;
					return false;
				}
			}
			
			long offset = page.Offset;
			int pageSize = tradeData.GetPageSize(offset);
			
			lock( dirtyLocker) {
				binaryPage = pagePool.Create();
				binaryPage.SetPageSize(pageSize);
				binaryPage.PageNumber = pageNumber;
				tradeData.Read(offset,binaryPage.Buffer,0,binaryPage.Buffer.Length);
			}
			return true;
		}
		
		internal BinaryPage GetPage(int pageNumber) {
			if( pageNumber == 0) {
				throw new ApplicationException("Found pageNumber is 0: \n" + this);
			}
			BinaryPage page;
			if( !TryGetPage(pageNumber, out page)) {
				throw new ApplicationException("Page number " + pageNumber + " was found neither in dirty page list nor in written pages: \n" + this);
			}
			return page;
		}
			   
		
		private volatile int maxPageNumber = 0;
		private volatile int maxPageId = 0;
		
		internal BinaryPage CreatePage(int pageNumber, int capacity) {
			if( pageNumber == 0) {
				throw new ApplicationException("Found pageNumber is 0: \n" + this);
			}
			BinaryPage page;
			if( TryGetPage(pageNumber,out page)) {
				throw new ApplicationException("Page number " + pageNumber + " already exists.");
			}
			lock( dirtyLocker) {
				page = pagePool.Create();
				page.SetCapacity(capacity);
				page.PageNumber = pageNumber;
				dirtyPages.Add(page);
				if( pageNumber > maxPageNumber) {
					maxPageNumber = pageNumber;
					maxPageId = page.Id;
				}
				if( page.PageNumber == 0) {
					throw new ApplicationException("Found pageNumber is 0: \n" + this);
				}
			}
			return page;
		}
		
		private bool asynchronous = true;
		
		private object writeLocker = new object();
		private Queue<TransactionPairsPage> writeQueue = new Queue<TransactionPairsPage>();
		internal void WritePage(TransactionPairsPage page) {
			if( asynchronous) {
				lock( writeLocker) {
					writeQueue.Enqueue(page);
				}
			} else {
				WritePageInternal(page);
			}
		}
		
		private void WritePageInternal( TransactionPairsPage page) {
			// Go to end of file.
			long offset = tradeData.Write(page.Buffer,0,page.Buffer.Length);
			lock( dirtyLocker) {
				try {
					if( page.PageNumber == 0) {
						throw new ApplicationException("Found pageNumber is 0: \n" + this);
					}
					offsets.Add(page.PageNumber,new Page(offset,page.Id));
				} catch( Exception ex) {
					string message = "Error while adding PageNumber " + page.PageNumber + ": " + ex.Message;
					log.Error(message, ex);
					try { 
						log.Error( message + "\n" + this, ex);
					} catch( Exception ex2) {
						log.Error("Exception while logging ToString of PairsPages: " + ex.Message, ex2);
					}
					throw new ApplicationException(message + "\n" + this, ex);
				}
				try { 
					pagePool.Free(page);
				} catch( Exception ex) {
					throw new ApplicationException("Failure while freeing page: " + page.PageNumber + "(" + page.Id +") :\n" + this, ex);
				}
				dirtyPages.Remove(page);
			}
		}
					
		private bool PageWriter() {
			bool result = false;
			while( writeQueue.Count > 0) {
				result = true;
				TransactionPairsPage page;
				lock( writeLocker) {
					page = writeQueue.Dequeue();
				}
				WritePageInternal(page);
			}
			return result;
		}
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Max Page Number: " + maxPageNumber + "(" + maxPageId + ")");
			sb.Append("Page Offsets: ");
			lock( dirtyLocker) {
				foreach( var kvp in offsets) {
					sb.Append( kvp.Key);
					sb.Append( "(");
					sb.Append( kvp.Value.Id);
					sb.Append( ")");
					sb.Append( ", ");
					sb.Append( kvp.Value.Offset);
					sb.Append( "  ");
				}
				sb.AppendLine();
				sb.Append("Dirty Pages: ");
				foreach( var temp in dirtyPages) {
					sb.Append( temp.PageNumber);
					sb.Append( "(");
					sb.Append( temp.Id);
					sb.Append( ")");
					sb.Append( "  ");
				}
			}
			sb.AppendLine();
			sb.Append("Pages in Queue: ");
			lock(writeLocker) {
				foreach( var temp in writeQueue) {
					sb.Append( temp.PageNumber);
					sb.Append( "(");
					sb.Append( temp.Id);
					sb.Append( ")");
					sb.Append( "  ");
				}
			}
			sb.AppendLine();
			return sb.ToString();
		}

		internal BinaryStore TradeData {
			get { return tradeData; }
		}
	
		
	}
}
