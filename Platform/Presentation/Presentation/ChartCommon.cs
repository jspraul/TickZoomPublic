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
using System.Drawing;
using System.Text;
using System.Threading;
using TickZoom.Api;

namespace TickZoom.Presentation
{
    public class ChartCommon : Chart
    {
        private static Log log;
        private bool isVisible;
        private static bool debug;
        private static bool trace;
        private object chartRenderLock = new Object();
        
        private readonly SynchronizationContext context;
        private readonly Dictionary<int, ChartItem> graphObjs = new Dictionary<int, ChartItem>();
        private readonly List<IndicatorInterface> indicators;
        private readonly string storageFolder;
        private readonly Dictionary<int, ChartItem> textObjs = new Dictionary<int, ChartItem>();
        private ChartType chartType = ChartType.Bar;
        private Color[] colors = {Color.Black, Color.Red, Color.FromArgb(194, 171, 213), Color.FromArgb(250, 222, 130)};
        private double lastBar;
        private double lastClose;
        private double lastHigh;
        private double lastLow;
        private double lastTime;
        private int objectId;
        private bool showPriceGraph = true;
        private bool showTradeTips = true;
        private SymbolInfo symbol;

        public ChartCommon()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            context = SynchronizationContext.Current;
            if (context == null)
            {
                context = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(context);
            }

            indicators = new List<IndicatorInterface>();
            try
            {
                log = Factory.SysLog.GetLogger(typeof (ChartCommon));
                debug = log.IsDebugEnabled;
                trace = log.IsTraceEnabled;
                Interval intervalChartDisplay = Factory.Engine.DefineInterval(BarUnit.Day, 1);
                Interval intervalChartBar = Factory.Engine.DefineInterval(BarUnit.Day, 1);
                Interval intervalChartUpdate = Factory.Engine.DefineInterval(BarUnit.Day, 1);
                storageFolder = Factory.Settings["AppDataFolder"];
                if (storageFolder == null)
                {
                    throw new ApplicationException("Must set AppDataFolder property in app.config");
                }
            }
            catch (Exception)
            {
                // This exception means we're running inside the form designer.
                // TODO: find a better way to determine if running in form designer mode.
            }
        }

        public bool IsVisible
        {
            get { return isVisible; }
            set { IsVisible = value;  }
        }

        public bool IsValid
        {
            get { return ChartBars != null; }
        }

        public ChartType ChartType
        {
            get { return chartType; }
            set { chartType = value; }
        }

		public object ChartRenderLock {
			get { return chartRenderLock; }
			set { chartRenderLock = value; }
		}
        #region Chart Members

        public void Hide()
        {
            isVisible = false;
        }

        public void Show()
        {
            isVisible = true;
        }

        public List<IndicatorInterface> Indicators
        {
            get { return indicators; }
        }

        /// <inheritdoc />
        public int DrawText(string text, Color color, int bar, double y, Positioning orient)
        {
            var textObj = new ChartText(text, bar, y, color);
            textObjs.Add(objectId, textObj);
            objectId++;
            return objectId;
        }

        public void WriteLine(string text)
        {
            // Not implemented.
        }

        public int DrawBox(Color color, int bar, double price)
        {
            var chartBox = new ChartBox(bar, price, 0, 0, color);
            objectId++;
            graphObjs.Add(objectId, chartBox);
            return objectId;
        }

        public int DrawBox(Color color, int bar, double price, double width, double height)
        {
            var chartBox = new ChartBox(bar, price, width, height, color);
            objectId++;
            graphObjs.Add(objectId, chartBox);
            return objectId;
        }

        public int DrawArrow(Color color, float size, int bar1, double y1, int bar2, double y2)
        {
            return 0;
        }

        public int DrawArrow(ArrowDirection direction, Color color, float size, int bar, double price)
        {
            ChartArrow chartArrow = CreateArrow(direction, color, size, bar, price);
            objectId++;
            graphObjs.Add(objectId, chartArrow);
            return objectId;
        }

        /// <summary>
        /// Draws a trade and annotateg with hover message if possible.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="fillPrice"></param>
        /// <param name="resultingPosition"></param>
        /// <returns></returns>
        public int DrawTrade(LogicalOrder order, double fillPrice, double resultingPosition)
        {
            Color color = Color.Empty;
            ArrowDirection direction = ArrowDirection.Up;
            switch (order.Type)
            {
                case OrderType.BuyLimit:
                case OrderType.BuyStop:
                case OrderType.BuyMarket:
                    color = Color.Green;
                    direction = ArrowDirection.Up;
                    break;
                case OrderType.SellLimit:
                case OrderType.SellStop:
                case OrderType.SellMarket:
                    color = Color.Red;
                    direction = ArrowDirection.Down;
                    break;
                default:
                    throw new ApplicationException("Unknown OrderType " + order.Type + " for drawing a trade.");
            }
            if (order.TradeDirection == TradeDirection.Exit ||
                order.TradeDirection == TradeDirection.ExitStrategy)
            {
                color = Color.Black;
            }
            // One ticket open on tickzoom is to draw arrows to scale based
            // on the price range of the chart. This numbers for size and position
            // were hard code, calibrated to Forex prices.
            ChartArrow chartArrow = CreateArrow(direction, color, 12.5f, ChartBars.BarCount, fillPrice);
            var sb = new StringBuilder();
            if (order.Tag != null)
            {
                sb.AppendLine(order.Tag.ToString());
            }
            sb.Append(order.TradeDirection);
            sb.Append(" ");
            sb.AppendLine(order.Type.ToString());
            if (order.Price > 0)
            {
                sb.Append("at ");
                sb.AppendLine(order.Price.ToString());
            }
            sb.Append("size ");
            sb.AppendLine(order.Position.ToString());
            sb.Append("filled ");
            sb.AppendLine(fillPrice.ToString());
            sb.Append("new positions ");
            sb.AppendLine(resultingPosition.ToString());
            chartArrow.Tag = sb.ToString();
            objectId++;
            graphObjs.Add(objectId, chartArrow);
            return objectId;
        }

        public int DrawLine(Color color, int bar1, double y1, int bar2, double y2, LineStyle style)
        {
            var chartLine = new ChartLine(color, bar1, y1, bar2, y2, style);
            objectId++;
            graphObjs.Add(objectId, chartLine);
            return objectId;
        }

        public void ChangeLine(int lineId, Color color, int bar1, double y1, int bar2, double y2, LineStyle style)
        {
            var chartLine = new ChartLine(color, bar1, y1, bar2, y2, style);
            graphObjs[lineId] = chartLine;
        }

        /// <summary>
        /// Obsolete. Please use only chartBars argument instead
        /// </summary>
        [Obsolete("Please use only chartBars argument instead.", true)]
        public void AddBar(Bars updateSeries, Bars displaySeries)
        {
            throw new NotImplementedException();
        }

        public void AddBar(Bars chartBars)
        {
        }

        public void OnInitialize()
        {
        }

        public bool ShowPriceGraph
        {
            get { return showPriceGraph; }
            set { showPriceGraph = value; }
        }

        public StrategyInterface StrategyForTrades { get; set; }

        public bool IsDynamicUpdate { get; set; }

        /// <summary>
        /// Obsolete. Please use only ChartBars instead
        /// </summary>
        [Obsolete("Please use only ChartBars instead.", true)]
        public Bars UpdateBars
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Obsolete. Please use only ChartBars instead
        /// </summary>
        [Obsolete("Please use only ChartBars instead.", true)]
        public Bars DisplayBars
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Bars ChartBars { get; set; }

        /// <summary>
        /// Obsolete. Please use only IntervalChartBar instead.
        /// </summary>
        [Obsolete("Please use only IntervalChartBar instead.", true)]
        public Interval IntervalChartDisplay
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Obsolete. Please use only IntervalChartBar instead.
        /// </summary>
        [Obsolete("Please use only IntervalChartBar instead.", true)]
        public Interval IntervalChartUpdate
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Interval IntervalChartBar { get; set; }

        public SymbolInfo Symbol
        {
            get { return symbol; }
            set { symbol = value; }
        }

        public bool ShowTradeTips
        {
            get { return showTradeTips; }
            set { showTradeTips = value; }
        }


        public void AudioNotify(Audio clip)
        {
            throw new NotImplementedException();
        }

        #endregion

        private ChartArrow CreateArrow(ArrowDirection direction, Color color, float size, int bar, double price)
        {
            double tipPrice = price;
            int tipBar = bar;
            switch (direction)
            {
                case ArrowDirection.Up:
                    price -= symbol.MinimumTick;
                    break;
                case ArrowDirection.Down:
                    price += symbol.MinimumTick;
                    break;
            }
            var chartArrow = new ChartArrow(color, size, bar, price, tipBar, tipPrice);
            return chartArrow;
        }

        public int DrawLine(Color color, Point p1, Point p2, LineStyle style)
        {
            var chartLine = new ChartLine(color, p1.X, p1.Y, p2.X, p2.Y, style);
            objectId++;
            graphObjs.Add(objectId, chartLine);
            return objectId;
        }

        private void UpdateScaleCheck(Bars series)
        {
            lastClose = series.Close[0];
            lastHigh = series.High[0];
            lastLow = series.Low[0];
            lastTime = series.Time[0].ToOADate();
            lastBar = series.CurrentBar;
        }
    }
}