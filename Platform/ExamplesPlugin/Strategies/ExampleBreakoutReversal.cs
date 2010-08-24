using System;
using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Examples
{

    public class ExampleBreakoutReversal : Strategy
    {
        private int length1 = 25;//private field
        public int Length1 { get { return length1; } set { length1 = value; } }

        private int length2 = 25;//private field
        public int Length2 { get { return length2; } set { length1 = value; } }

        private int qtyb = 1;//private field
        public int Qtyb { get { return qtyb; } set { qtyb = value; } }

        private int qtys = 1;//private field
        public int Qtys { get { return qtys; } set { qtys = value; } }
        
        public ExampleBreakoutReversal()
        {

        }

        public override void OnInitialize()
        {
        	qtys = qtyb = Data.SymbolInfo.Level2LotSize;
        }


        //EZ Language:
        //   Buy QtyB contracts at highest(High,Length1) + 1 point stop;
        //   Sell QtyS contracts at lowest (Low,Length2) - 1 point stop;
        public override bool OnIntervalClose()
        {
            if (!this.Position.HasPosition)
            {
                this.Orders.Enter.NextBar.BuyStop(Formula.Highest(Bars.High, length1) + (1 * this.Data.SymbolInfo.MinimumTick), qtyb);
                this.Orders.Enter.NextBar.SellStop(Formula.Lowest(Bars.Low, length2) - (1 * this.Data.SymbolInfo.MinimumTick), qtys);
            }
            else
            {
                this.Orders.Reverse.NextBar.BuyStop(Formula.Highest(Bars.High, length1) + (1 * this.Data.SymbolInfo.MinimumTick), qtyb);
                this.Orders.Reverse.NextBar.SellStop(Formula.Lowest(Bars.Low, length2) - (1 * this.Data.SymbolInfo.MinimumTick), qtys);
            }
            return true;
        }
    }
}
