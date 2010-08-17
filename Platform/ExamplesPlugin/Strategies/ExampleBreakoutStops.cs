using System;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Examples
{
	public class ExampleBreakoutStops : Strategy
	{	
	    public ExampleBreakoutStops()
	    {
	
	    }
	
	    public override void OnInitialize()
	    {
	    }

	    public override bool OnIntervalClose()
	    {
			//Example breakout buy code
			if (this.Position.IsFlat)
			{
				this.Orders.Enter.NextBar.BuyStop(Formula.Highest(Bars.High, 25) + (1 * this.Data.SymbolInfo.MinimumTick), 1);
			} else if( !this.Position.IsShort) {
				this.Orders.Exit.NextBar.SellStop(Formula.Lowest(Bars.Low, 25) - (1 * this.Data.SymbolInfo.MinimumTick));
			}
			//-------------------------------
			//Example breakout sell code
			if (this.Position.IsFlat)
			{
				this.Orders.Enter.NextBar.SellStop(Formula.Lowest(Bars.Low, 25) - (1 * this.Data.SymbolInfo.MinimumTick), 1);
			} else if( !this.Position.IsLong ) {
				this.Orders.Exit.NextBar.BuyStop(Formula.Highest(Bars.High, 25) + (1 * this.Data.SymbolInfo.MinimumTick));
			}
			return true;
		}
	}
}