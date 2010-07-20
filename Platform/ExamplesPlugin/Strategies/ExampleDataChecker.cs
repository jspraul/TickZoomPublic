using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Drawing;
using System.IO;
using TickZoom.Api;
using TickZoom.Common;


namespace TickZoom.Examples
{
    public class ExampleDataChecker : Strategy
    {
        int countBarMsgs = 0;
        int barMsgLimit = 3;
        int countTickMsgs = 0;
        int tickMsgLimit = 3;
        string msg;

        public ExampleDataChecker() { }

        public override void OnInitialize()
        {
            msg = "DataChecker.OnInitialize: ";
            Log.Notice(msg);
        }

        public override bool  OnProcessTick(Tick tick)
        {
            if (countTickMsgs < tickMsgLimit)
            {
                msg = "DataCheck.OnProcessTick: " + 
                        "time = " + tick.Time.ToString() + ", " +
                        "tick price = " + tick.Price.ToString() + ", " +
                        "bid = " + tick.Bid.ToString() + ", " + 
                        "ask = " + tick.Ask.ToString();

                Log.Notice(msg);

                countTickMsgs++;
            }
            
            return true;
        }

        public override bool OnIntervalClose()
        {
            if (countBarMsgs < barMsgLimit)
            {
                msg = "DataCheck.OnIntervalClose: " +
                                    "bar unit = " + Bars.Interval.BarUnit + ", " +
                					"Time = " + Bars.Time[0] + ", " +
                					"EndTime = " + Bars.EndTime[0] + ", " +
                                    "open = " + Bars.Open[0] + ", " +
                                    "high = " + Bars.High[0] + ", " +
                                    "low = " + Bars.Low[0] + ", " +
                                    "close = " + Bars.Close[0] + ", " +
                                    "volume = " + Bars.Volume[0];

                Log.Notice(msg);

                countBarMsgs++;
            }
            TimeStamp time = Bars.Time[0];
            // for hourly interval testing for input tick prices outside range of resulting bar
            if (Bars.Time[0].TimeOfDay >= new Elapsed(9, 0, 0) && Bars.Time[0].TimeOfDay < new Elapsed(10, 0, 0) )
            {
                msg = "DataCheck.OnIntervalClose: " +
                                    "bar unit = " + Bars.Interval.BarUnit + ", " +
                					"Time = " + Bars.Time[0] + ", " +
                					"EndTime = " + Bars.EndTime[0] + ", " +
                                    "open = " + Bars.Open[0] + ", " +
                                    "high = " + Bars.High[0] + ", " +
                                    "low = " + Bars.Low[0] + ", " +
                                    "close = " + Bars.Close[0] + ", " +
                                    "volume = " + Bars.Volume[0];

                Log.Notice(msg);

            }

            return true;
        }

    }
}
