/****
TRO_DYNAMIC_SR3 5:43 AM 9/8/2006 PLOT DYNAMIC SUPPORT/RESISTANCE
 
Programmer:  Avery T. Horton, Jr.  aka TheRumpledOne,  
DONATIONS AND GIFTS ACCEPTED 
P O BOX 43575, TUCSON, AZ 85733 

please include this and/or any other comment blocks and a 
description of any changes you make.
****/
/*
Converted to NeoTicker by Luke
10/25/2006
*/
using System;
using System.Drawing;
using System.Text;

using TickZoom.Api;


namespace TickZoom.Common
{
    public class DynamicSR : IndicatorCommon
    {
        #region Consts & Variables
        bool ShowMidpoint;
        double prevDynamicS = double.MaxValue;
        double prevDynamicR = double.MinValue;
        Doubles dynamicS;
        Doubles dynamicR;
        int newDynamicRCount = 0;
        int newDynamicSCount = 0;
        double DynamicM;
        int lookbackPeriod;
		Trend trend = Trend.None;
		int confirmBars;
        #endregion
        
        #region FirstCall
        public DynamicSR()
        {
            ShowMidpoint = false;
            lookbackPeriod = 4;
            confirmBars = 2;
            dynamicS = Doubles(10);
        	dynamicR = Doubles(10);
        }
        #endregion

		public override void Update()
		{
            if (Bars.CurrentBar==0)
            {
            	dynamicR.Add(Bars.High[0]);
            	dynamicS.Add(Bars.Low[0]);
            }

            double newDynamicR = Formula.Highest( Bars.High, lookbackPeriod,0);
            double newDynamicS = Formula.Lowest( Bars.Low,lookbackPeriod,0);

            if (newDynamicR != Bars.High[0] && newDynamicR < prevDynamicR ) {
            	newDynamicR = prevDynamicR;
            	newDynamicRCount++;
            	if( newDynamicR != dynamicR[0] && newDynamicRCount > confirmBars) {
            		newDynamicRCount = 0;
            		dynamicR.Add(newDynamicR);
	            	CalcTrend();
            	}
            }
            
            if (newDynamicS != Bars.Low[0] && newDynamicS > prevDynamicS ) {
                newDynamicS = prevDynamicS;
            	newDynamicSCount++;
            	if( newDynamicS != dynamicS[0] && newDynamicSCount > confirmBars) {
            		newDynamicSCount = 0;
            		dynamicS.Add(newDynamicS);
	            	CalcTrend();
            	}
           	}

            Chart.DrawBox( Color.Red, Bars.CurrentBar, dynamicS[0]);
            Chart.DrawBox( Color.Blue, Bars.CurrentBar, dynamicR[0]);

            if (ShowMidpoint)
            {
                DynamicM = (newDynamicS + newDynamicR) / 2;
	            Chart.DrawBox( Color.Yellow, Bars.CurrentBar, DynamicM);
            }

            if (IsDebug)
                DebugSummary();
            
            prevDynamicR = newDynamicR;
            prevDynamicS = newDynamicS;
            
        }
		
		private void CalcTrend() {
			trend = Trend.Flat;
			if( dynamicS.Count > 1 && dynamicR.Count > 1) {
				if( dynamicS[0] > dynamicS[1] && dynamicR[0] > dynamicR[1] ) {
					trend = Trend.Up;
				} else if( dynamicS[0] < dynamicS[1] && dynamicR[0] < dynamicR[1] ) {
					trend = Trend.Down;
				} 
			}
		}

		public Doubles DynamicS {
			get { return dynamicS; }
		}
        
		public Doubles DynamicR {
			get { return dynamicR; }
		}
		
		public Trend Trend {
			get { return trend; }
		}
		
		public int LookbackPeriod {
			get { return lookbackPeriod; }
			set { lookbackPeriod = value; }
		}

		#region General
        protected void DebugSummary()
        {
        	Log.Debug( "DYNAMIC SUPPORT:    " + dynamicS[0] );
            Log.Debug( "LOW:                " + Bars.Low[0]);
            Log.Debug( "xLL:                " + Formula.Lowest(Bars.Low,lookbackPeriod));

            Log.Debug( "DYNAMIC RESISTANCE: " + dynamicR[0] );
            Log.Debug( "HIGH:               " + Bars.High[0]);
            Log.Debug( "xHH:                " + Formula.Highest(Bars.High,lookbackPeriod));

            Log.Debug( Ticks[0].Time.ToString());
            Log.Debug( "------- TRO_DYNAMIC_SR2 ---------");
        }
        #endregion
    }
}
