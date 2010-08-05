#region Copyright
#endregion

using System;
using System.Collections.Generic;
using TickZoom.Api;

namespace TickZoom.Examples
{
	internal class RenkoSeries : BarLogic {
		long stepSize, reversalStepSize;
	    TimeStamp dateTime;
	    long open;
	    long high;
	    long low;
	    int volume;
	    int tickCount;
	    bool? uptrend = null;
	    bool needsNewBar = false;
	    
		internal RenkoSeries( SymbolInfo symbol, double stepSize, double reversalStepSize) {
	    	this.stepSize = (stepSize*symbol.MinimumTick).ToLong();
	    	this.reversalStepSize = (reversalStepSize*symbol.MinimumTick).ToLong();
		}
			
	    public void InitializeTick( Tick tick, BarData data) {
	        dateTime = tick.Time;
	        long price = tick.IsTrade ? tick.lPrice : (tick.lAsk + tick.lBid) / 2;
	        open = price;
	        high = price;
	        low = price;
	        volume = 0;
	        needsNewBar = false;
		}
	    
	    public bool IsNewBarNeeded(Tick tick) {
	    	needsNewBar = false;
            long price = tick.IsTrade ? tick.lPrice : (tick.lBid + tick.lAsk) / 2;
            
	        if (uptrend == null) {                                                          //3
	            //////////////////
	
	            if (price - open >= stepSize) {                                        //4	
	                uptrend = true;
                    return true;
	            } else if (open - price >= stepSize) {                                 //7	
	                uptrend = false;
			    	needsNewBar = true;
                    return true;
	            } else {
                    return false;
	            }
	        } else if (uptrend == true) {                                                   //12
	
	            //////////////////
	            //Uptrend
	            if (price - open >= stepSize) {                                        //13
	                //Uptrend continues
			    	needsNewBar = true;
                    return true;
	            } else if (open - price >= reversalStepSize) {                         //16
			    	needsNewBar = true;
                    return true;
	            } else {
                    return false;
	            }
	        } else {
	            //////////////////
	            //Downtrend
	
	            if (open - price >= stepSize) {                                        //22
			    	needsNewBar = true;
                    return true;
	            } else if (price - open >= reversalStepSize) {                         //25
			    	needsNewBar = true;
                    return true;
	            } else {
	            	return false;
	            }
	        }
	    }
		
	    public void ProcessTick(Tick tick, BarData data) {
	
            long price = tick.IsTrade ? tick.lPrice : (tick.lBid + tick.lAsk) / 2;
            
            int tickVolume = tick.IsTrade ? tick.Volume : 0;
            
            if( !needsNewBar) {
                //Update existing bar
                dateTime = tick.Time;

                volume += tickVolume;
                high = price > high ? price : high;                           //29
                low = price < low ? price : low;                        //30
                tickCount ++;

                data.UpdateBar( high, low, price, volume, dateTime, tickCount);
                return;
            }

            
	        if (uptrend == null) {                                                          //3
	            //////////////////
	
	            if (price - open >= stepSize) {                                        //4
	
	                uptrend = true;
	                while (price - open >= stepSize) {                                 //5
	
	                	tickCount = 1;
	                	data.NewBar( open, open+stepSize, low, open+stepSize, volume, dateTime, tickCount);
	
	                    dateTime = tick.Time;
	                    open = low = open + stepSize;
	                    high = price < open + stepSize ? price : open;            //6
	
	                    volume = 0;
	
	                    return;
	                }
	
	
	                volume = tickVolume;
	            } else if (open - price >= stepSize) {                                 //7
	
	                uptrend = false;
	                while (open - price >= stepSize) {                                 //8
	
	                	tickCount = 1;
	                	data.NewBar( open, high, open+stepSize, open-stepSize, volume, dateTime, tickCount);
	
	                    dateTime = tick.Time;
	                    open = high = open - stepSize;
	                    low = price > open - stepSize ? price : open;             //9
	
	                    volume = 0;
	
	                    return;
	                }
	
	
	                volume = tickVolume;
	            } else {
	                //Update existing bar
	                dateTime = tick.Time;
	
	                high = price > high ? price : high;                           //10
	                low = price < low ? price : low;                              //11
	
	                volume += tickVolume;
	                
	                data.UpdateBar( high, low, price, volume, dateTime, tickCount);
	            }
	        } else if (uptrend == true) {                                                   //12
	
	            //////////////////
	            //Uptrend
	            if (price - open >= stepSize) {                                        //13
	
	                //Uptrend continues
	                while (price - open >= stepSize) {                                 //14
	
	                	tickCount = 1;
	                	data.NewBar( open, open+stepSize, low, open+stepSize, volume, dateTime, tickCount);
	
	                    dateTime = tick.Time;
	                    open = low = open + stepSize;
	                    high = price < open + stepSize ? price : open;            //15
	
	                    volume = 0;
	
	                    return;
	                }
	
	
	                volume = tickVolume;
	            } else if (open - price >= reversalStepSize) {                         //16
	
	                //Uptrend has reversed to downtrend
	                uptrend = false;
	
	                //Reversal bar
	
	                tickCount = 1;
                	data.NewBar( open, high, open - reversalStepSize, open - reversalStepSize, volume, dateTime, tickCount);
	
	                dateTime = tick.Time;
	                open = high = low = open - reversalStepSize;
	                volume = open - price < stepSize ? tickVolume : 0;                //17
	
	
	                return;
	
	                while (open - price >= stepSize) {                                 //18
	
	                	tickCount = 1;
	                	data.NewBar( open, high, open - stepSize, open - stepSize, volume, dateTime, tickCount);
	
	                    dateTime = tick.Time;
	                    open = high = open - stepSize;
	                    low = price > open - stepSize ? price : open;             //19
	
	                    volume = 0;
	
	                    return;
	                }
	
	
	                volume = tickVolume;
	            } else {
	                //Update existing bar
	                dateTime = tick.Time;
	
	                volume += tickVolume;
	                high = price > high ? price : high;                           //20
	                low = price < low ? price : low;                              //21
	
	                data.UpdateBar( high, low, price, volume, dateTime, tickCount);
	            }
	        } else {
	            //////////////////
	            //Downtrend
	
	            if (open - price >= stepSize) {                                        //22
	                //Downtrend continues
	
	                while (open - price >= stepSize) {                                 //23
	                	tickCount = 1;
	                	data.NewBar( open, high, open - stepSize, open - stepSize, volume, dateTime, tickCount);
	
	                    dateTime = tick.Time;
	                    open = high = open - stepSize;
	                    low = price > open - stepSize ? price : open;             //24
	
	                    volume = 0;
	
	                    return;
	                }
	
	
	                volume = tickVolume;
	            } else if (price - open >= reversalStepSize) {                         //25
	
	                //Downtrend has reversed to uptrend
	                uptrend = true;
	
	                //Reversal bar
	
                	tickCount = 1;
                	data.NewBar( open, open + reversalStepSize, low, open + reversalStepSize, volume, dateTime, tickCount);
	
	                dateTime = tick.Time;
	                open = high = low = open + reversalStepSize;
	                volume = (price - open) < stepSize ? tickVolume : 0;              //26
	
	
	                return;
	
	                while (price - open >= stepSize) {                                 //27
	
	                	tickCount = 1;
	                	data.NewBar( open, open + stepSize, low, open + stepSize, volume, dateTime, tickCount);
	
	                    dateTime = tick.Time;
	                    open = low = open + stepSize;
	                    high = price < open + stepSize ? price : open;            //28
	
	                    volume = 0;
	
	                    return;
	                }
	
	
	                volume = tickVolume;
	            }
	        }
		
	        return;
	    }
	    
	    public void Dispose() {
	    	
	    }
			
	}
}
