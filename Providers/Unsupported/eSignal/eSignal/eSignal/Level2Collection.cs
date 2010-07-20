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
using System.Collections.ObjectModel;
using TickZoom.Api;

namespace TickZoom.Common.Provider
{
	/// <summary>
	/// Description of LevelIICollection.
	/// </summary>
	public class LEVEL2RECORD {
		public enumMarketSide side;
		public double dPrice;
		public int lSize;
		public int bClosed;
		public string bstrMMID;
	}
	public enum enumMarketSide {
		msAsk,
		msBid
	}
	public class Level2Collection : Collection<LEVEL2RECORD>
	{
		Log log = Factory.Log.GetLogger(typeof(Level2Collection));
		enumMarketSide side;
		double lastPrice = 0;
		int lastSize = 0;
		SymbolInfo instrument;
		Action<bool,TradeSide,double,long> logChange;
		
		public Level2Collection(SymbolInfo instrument, enumMarketSide side)
		{
			this.instrument = instrument;
			this.side = side;
		}
		
		long lastTotalSize = 0;
		public bool HasChanged {
			get {
				if( lastTotalSize == 0) {
					lastTotalSize = TotalSize();
				}
				if( TotalSize() != lastTotalSize) {
					return true;
				} else {
					return false;
				}
			}
		}
		
		public void UpdateTotalSize() {
			lastTotalSize = TotalSize();
		}
		
		string MarketOrderSide(enumMarketSide limitOrderSide)  {
			return limitOrderSide == enumMarketSide.msAsk ? "Buy" : "Sell";
		}
		
        public void Process(LEVEL2RECORD pRec)
        {       	
	        AddRecord(pRec);
        	if( pRec.side != side) {
        		throw new ApplicationException("Mismatch in market side");
        	}
	        
       		UpdateDepth();
       		if( this.Count > 0 ) {
				if( lastPrice == this[0].dPrice) {
		        	int volumeChange = (int) ((lastSize - this[0].lSize)/instrument.Level2LotSize);
		        	if( volumeChange > 0 ) {
		        		logChange(true, side == enumMarketSide.msAsk ? TradeSide.Buy : TradeSide.Sell,
		        		                     lastPrice,volumeChange);
		        	}
		        } else if( lastPrice > 0) {
		        	if( (side == enumMarketSide.msAsk && lastPrice < this[0].dPrice) || 
		        	   	(side == enumMarketSide.msBid && lastPrice > this[0].dPrice ) ) {
		        		logChange(true, side == enumMarketSide.msAsk ? TradeSide.Buy : TradeSide.Sell,
		        		                     lastPrice,lastSize/instrument.Level2LotSize);
		        	}
		        }
	        	lastSize = this[0].lSize;
	            lastPrice = this[0].dPrice;
       		}
        }
        
        ushort[] depthSizes = new ushort[0];
        long[] depthPrices = new long[0];
        
        public int AveragePrice() {
        	long totalPrices=0;
        	long totalSizes= 0;
        	for( int i=0; i< depthSizes.Length && i< depthPrices.Length; i++) {
        		totalPrices+=depthPrices[i]*depthSizes[i];
        		totalSizes+=depthSizes[i];
        	}
        	if( totalSizes > 0) {
	        	return (int) ((totalPrices/totalSizes) - depthPrices[0]);
        	} else {
	        	return 0;
        	}
        }
        
        public void UpdateDepth() {
        	if( depthSizes.Length > 0) {
        		for( int i=0; i<depthSizes.Length; i++) {
        			depthSizes[i] = 0;
        			depthPrices[i] = 0;
        		}
        	} else {
				depthSizes = new ushort[5];
				depthPrices = new long[5];
        	}
        	
        	if( this.Count > 0) {
	        	int i=0,j=0;
	        	long firstPrice = this[0].dPrice.ToLong();
	        	long increment = instrument.Level2Increment.ToLong();
	        	while( j<depthPrices.Length&&i<this.Count) {
	        		depthPrices[j] = firstPrice + increment*j*(side==enumMarketSide.msAsk?1:-1);
	        		long price = this[i].dPrice.ToLong();
		        	ushort size = (ushort) (this[i].lSize/instrument.Level2LotSize);
		        	if( price == depthPrices[j]) {
	        			depthSizes[j] += size;
	        			i++;
		        	} else {
		        		j++;
		        	}
	        	}
        	}
        }
        
        public long TotalSize() {
        	long totalSize = 0;
        	ushort[] array = DepthSizes;
        	for( int i=0; i<array.Length; i++) {
        		totalSize += array[i];
        	}
        	return totalSize;
        }
        
        public void LogMarketChange(long volumeChange) {
        	log.Notice( MarketOrderSide(side) + ": " +
        	                      lastPrice + ", " +
        	                      volumeChange + "  ");
        }

        public void AddRecord(LEVEL2RECORD pRec) {
        	ushort size = (ushort) (pRec.lSize/instrument.Level2LotSize);

            // Remove any deleted or closed items.
            if (pRec.dPrice == 0 || pRec.lSize == 0 || pRec.bClosed == -1) {
	            for (int j = 0; j < Count; j++) {
            		if (pRec.bstrMMID.Equals(this[j].bstrMMID)) {
	                	RemoveAt(j);
	                	return;
            		}
	            }
            	// deleted or closed item not found.
            	return;
            }
            // Search for any changes to existing records.
            for (int i = 0; i < Count; i++) {
            	if (pRec.bstrMMID.Equals(this[i].bstrMMID)) {
            		if(pRec.dPrice == this[i].dPrice) {
			        	if( size < instrument.Level2LotSizeMinimum) {
	            			RemoveAt(i);
            			} else {
            				this[i] = pRec;
            			}
	            		return;
            		} else {
            			// Match found, but the price change.
            			// So reinsert it below at the correct position.
            			RemoveAt(i);
            		}
            	}
            	// No existing record found or price changed.
            }

            // Don't insert any smallish orders.
        	if( size < instrument.Level2LotSizeMinimum) { return; }
        	
            // Insert new records in price order.
            for (int i = 0; i < Count; i++) {
            	if (pRec.side == enumMarketSide.msAsk ) {
                    if(pRec.dPrice < this[i].dPrice) {
            			Insert(i, pRec);
            			return;
            		}
            	}
            	else if (pRec.side == enumMarketSide.msBid ) {
                    if(pRec.dPrice > this[i].dPrice) {
            			Insert(i, pRec);
            			return;
            		}
            	}
            }
            // If still not inserted, then it goes at the end of the list.
            if (pRec.dPrice != 0 && pRec.lSize != 0 && pRec.bClosed != 1) {
	           	Add(pRec);
            }
        	return;
        }

		public double LastPrice {
			get { return lastPrice; }
		}
		
		public long LastSize {
			get { return lastSize/instrument.Level2LotSize; }
		}

        public ushort[] DepthSizes {
			get { return depthSizes; }
		}

        public long[] DepthPrices {
			get { return depthPrices; }
		}
		
		public Action<bool, TradeSide, double, long> LogChange {
			get { return logChange; }
			set { logChange = value; }
		}
	}
}
