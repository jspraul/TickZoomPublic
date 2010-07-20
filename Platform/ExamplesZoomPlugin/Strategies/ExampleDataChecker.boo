

namespace TickZoom

import System
import TickZoom.Api
import TickZoom.Common

class ExampleDataChecker(StrategyCommon):

	private countBarMsgs = 0

	private barMsgLimit = 3

	private countTickMsgs = 0

	private tickMsgLimit = 3

	private msg as string

	
	def constructor():
		pass

	
	override def OnInitialize():
		msg = 'DataChecker.OnInitialize: '
		Log.Notice(msg)

	
	override def OnProcessTick(tick as Tick) as bool:
		if countTickMsgs < tickMsgLimit:
			msg = ((((((((((('DataCheck.OnProcessTick: ' + 'time = ') + tick.Time.ToString()) + ', ') + 'tick price = ') + tick.Price.ToString()) + ', ') + 'bid = ') + tick.Bid.ToString()) + ', ') + 'ask = ') + tick.Ask.ToString())
			
			Log.Notice(msg)
			
			countTickMsgs += 1
		
		return true

	
	override def OnIntervalClose() as bool:
		if countBarMsgs < barMsgLimit:
			msg = ((((((((((((((((((((((('DataCheck.OnIntervalClose: ' + 'bar unit = ') + Bars.Interval.BarUnit) + ', ') + 'Time = ') + Bars.Time[0]) + ', ') + 'EndTime = ') + Bars.EndTime[0]) + ', ') + 'open = ') + Bars.Open[0]) + ', ') + 'high = ') + Bars.High[0]) + ', ') + 'low = ') + Bars.Low[0]) + ', ') + 'close = ') + Bars.Close[0]) + ', ') + 'volume = ') + Bars.Volume[0])
			
			Log.Notice(msg)
			
			countBarMsgs += 1
		// for hourly interval testing for input tick prices outside range of resulting bar
		if (Bars.Time[0].TimeOfDay >= cast(double,Elapsed(9, 0, 0))) and (Bars.Time[0].TimeOfDay < cast(double,Elapsed(10, 0, 0))):
			msg = ((((((((((((((((((((((('DataCheck.OnIntervalClose: ' + 'bar unit = ') + Bars.Interval.BarUnit) + ', ') + 'Time = ') + Bars.Time[0]) + ', ') + 'EndTime = ') + Bars.EndTime[0]) + ', ') + 'open = ') + Bars.Open[0]) + ', ') + 'high = ') + Bars.High[0]) + ', ') + 'low = ') + Bars.Low[0]) + ', ') + 'close = ') + Bars.Close[0]) + ', ') + 'volume = ') + Bars.Volume[0])
			
			Log.Notice(msg)
			
		
		return true
	

