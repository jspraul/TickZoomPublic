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
		private static readonly Log log = Factory.Log.GetLogger(typeof(TransactionPairsPages));
		PagePool<TransactionPairsPage> pagePool = new PagePool<TransactionPairsPage>();
		private PageStore tradeData;
		
		private object dirtyLocker = new object();
		private volatile int pageCount = 0;
		List<TransactionPairsPage> dirtyPages = new List<TransactionPairsPage>();
		Dictionary<int,long> offsets = new Dictionary<int,long>();
		
		internal TransactionPairsPages(PageStore tradeData) {
			this.tradeData = tradeData;
			if( asynchronous) {
				this.tradeData.AddPageWriter( PageWriter);
			}
		}
		
		internal void TryRelease(TransactionPairsPage page) {
			bool contains = false;
			lock( dirtyLocker) {
				contains = dirtyPages.Contains(page);
			}
			if( page != null && !contains) {
				pagePool.Free(page);
			}
		}
		
		internal TransactionPairsPage GetPage(int pageNumber) {
			lock( dirtyLocker) {
				if( pageNumber >= pageCount) {
					foreach( var unwrittenPage in dirtyPages) {
						if( unwrittenPage.PageNumber == pageNumber) {
							return unwrittenPage;
						}
					}
					throw new ApplicationException("Page number " + pageNumber + " was out side number of pages: " + pageCount + " and not found in unwritten page list.\n" + this);
				}
			}
			long offset = 0L;
			lock( dirtyLocker) {
				offset = offsets[pageNumber];
			}
			TransactionPairsPage page = pagePool.Create();
			int pageSize = tradeData.GetPageSize(offset);
			page.SetPageSize(pageSize);
			page.PageNumber = pageNumber;
			tradeData.Read(offset,page.Buffer,0,page.Buffer.Length);
			return page;
		}
		
		private volatile int maxPageNumber = 0;
		
		internal TransactionPairsPage CreatePage(int pageNumber, int capacity) {
			if( pageNumber < pageCount) {
				throw new ApplicationException("Page number " + pageNumber + " already exists.");
			}
			TransactionPairsPage page = pagePool.Create();
			page.SetCapacity(capacity);
			page.PageNumber = pageNumber;
			lock( dirtyLocker) {
				dirtyPages.Add(page);
			}
			maxPageNumber = Math.Max(maxPageNumber,pageNumber);
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
					offsets.Add(page.PageNumber,offset);
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
				pageCount++;
				dirtyPages.Remove(page);
			}
			pagePool.Free(page);
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
			sb.AppendLine("Page Count: " + pageCount);
			sb.AppendLine("Max Page Number: " + maxPageNumber);
			sb.Append("Page Offsets: ");
			lock( dirtyLocker) {
				foreach( var kvp in offsets) {
					sb.Append( kvp.Key);
					sb.Append( ", ");
					sb.Append( kvp.Value);
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

		internal PageStore TradeData {
			get { return tradeData; }
		}
	
		
	}
}
