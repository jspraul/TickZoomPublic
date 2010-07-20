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
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using TickZoom;
using TickZoom.Api;
using ZedGraph;

namespace TickZoom
{
	/// <summary>
	/// Description of UserControl1.
	/// </summary>
    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
    [DefaultEvent("ActiveContentChanged")]
    public partial class ChartControl : UserControl, TickZoom.Api.Chart
	{
    	Log log;
	    TimeStamp firstTime;
		StockPointList spl;
		List<PointPairList> lineList;
		List<IndicatorInterface> indicators; 
		object chartLocker = new Object();
		bool showPriceGraph = true;
		static Interval initialInterval;
		int objectId = 0;
		float _clusterWidth = 0;
		StrategyInterface strategy;
		bool isDynamicUpdate = false;
		Color[] colors = { Color.Black, Color.Red, Color.FromArgb(194,171,213), Color.FromArgb (250,222,130) } ;
		bool isAudioNotify = false;
		bool isAutoScroll = true;
		bool isCompactMode = false;
		ChartType chartType = ChartType.Bar;
		Interval intervalChartBar = initialInterval;
		string storageFolder;
		SymbolInfo symbol;
		bool showTradeTips = true;
		
		public bool ShowTradeTips {
			get { return showTradeTips; }
			set { showTradeTips = value; }
		}
		
	    public ChartControl()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
		    spl = new StockPointList();
		    lineList = new List<PointPairList>();
		    indicators = new List<IndicatorInterface>();
	    }
	    
		void ChartControlLoad(object sender, EventArgs e)
		{
			if( !DesignMode) {
				log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
				initialInterval = Factory.Engine.DefineInterval(BarUnit.Default,0);
	       		storageFolder = Factory.Settings["AppDataFolder"];
	       		if( storageFolder == null) {
	       			throw new ApplicationException( "Must set AppDataFolder property in app.config");
	       		}
				ChartLoad();
			}
		}
		
		public void ChartLoad() {
		   DrawChart();
		   // Size the control to fill the form with a margin
		   SetSize();			
		}
		public void ChartResizeEvent(object sender, EventArgs e)
		{
			SetSize();
		}
		
		public void SetSize()
		{
		   dataGraph.Location = new System.Drawing.Point( 0, 0 );
		   // Leave a small margin around the outside of the control
		   
		   dataGraph.Size = new Size( ClientRectangle.Width - 10,
		                           ClientRectangle.Height - 20);
		}
		private delegate void WriteLineDelegate(string text);		
		
		public void WriteLine(string text) {
		}
		
		public void AudioNotify(Audio clip) {
			if( isAudioNotify) {
				string fileName = "";
				try {
					switch( clip) {
						case Audio.RisingVolume:
							fileName = storageFolder + @"\Media\risingVolume.wav";
						    SoundPlayer simpleSound = new SoundPlayer(fileName);
						    simpleSound.Play();
						    break;
						case Audio.IntervalChime:
						    fileName = storageFolder + @"\Media\intervalChime.wav";
						    simpleSound = new SoundPlayer(fileName);
						    simpleSound.Play();
						    break;
					}
				} catch( Exception e) {
					log.Notice("Error playing " + fileName + ": " + e);
				}
			}
		}
	
		private void InitializeChart( )
		{
			MasterPane master = dataGraph.MasterPane;
	
			// Fill background
			master.Fill = new Fill( Color.White, Color.FromArgb( 220, 220, 255 ), 45.0f );
			// Clear out the initial GraphPane
			master.PaneList.Clear();
			
			//Show masterpane title.
			master.Title.IsVisible = false;
			
			// Leave a margin around the masterpane, but only small gap between panes.
			master.Margin.All = 10;
			master.InnerPaneGap = 5;
			
		    priceGraphPane = createPane();
			
			string[] yLables = { "Fun 1", "Fun 2", "Fun 3" };
			ColorSymbolRotator rotator = new ColorSymbolRotator();
			
		}
	
		void setLayout() {
			layoutLastPane();
			MasterPane master = dataGraph.MasterPane;
			master.IsFontsScaled = false;
	//			PaneList paneList = master.PaneList;
			using ( Graphics g = this.CreateGraphics() )
			{
				float[] proportions = new float[master.PaneList.Count];
				int[] layout = new int[master.PaneList.Count];
				layout[0] = 1;
				proportions[0] = Math.Max(4,master.PaneList.Count);
				for( int i = 1; i< master.PaneList.Count; i++) {
					layout[i] = 1;
					proportions[i] = 1;
				}
				master.IsCommonScaleFactor = true;
				master.SetLayout(g, true, layout, proportions);
				
				// Synchronize the Axes
				dataGraph.IsAutoScrollRange = true;
				dataGraph.IsShowHScrollBar = true;
				dataGraph.IsSynchronizeXAxes = true;
			}
		}
		
		GraphPane createPane() {
			MasterPane master = dataGraph.MasterPane;
			// Create a new graph -- dimensions to be set later by MasterPane Layout
			GraphPane myPaneT = new GraphPane( new Rectangle( 10, 10, 10, 10 ),
				"",
				"Time, Days",
				"USD/JPY" );
			
			myPaneT.Fill.IsVisible = false;
			
			// pretty it up a little
	//			myPaneT.Chart.Fill = new Fill( Color.White, Color.LightGoldenrodYellow, 45.0f );
			myPaneT.Chart.Fill = new Fill( Color.White, Color.White, 45.0f );
			myPaneT.Border = new Border(true, Color.Black, 2);
			// set the dimension so fonts look bigger.
			myPaneT.BaseDimension = 3.0F;
			
			// Hide the titles
			myPaneT.XAxis.Title.IsVisible = false;
			if( isCompactMode) {
				myPaneT.YAxis.Title.IsVisible = false;
				myPaneT.YAxis.Scale.IsVisible = false;
				myPaneT.YAxis.MinSpace = 10;
			} else {
				myPaneT.YAxis.Title.IsVisible = true;
				myPaneT.YAxis.Scale.IsVisible = true;
				myPaneT.YAxis.MinSpace = 80;
			}
			myPaneT.YAxis.Title.IsOmitMag = false;
			
			myPaneT.Legend.IsVisible = false;
			myPaneT.Border.IsVisible = false;
			myPaneT.Title.IsVisible = false;
			
			// Get rid of the tics that are outside the chart rect
			myPaneT.XAxis.MajorTic.IsOutside = false;
			myPaneT.XAxis.MinorTic.IsOutside = false;
			
			myPaneT.XAxis.Scale.IsVisible = false;
			
			// Show the X grids
			myPaneT.XAxis.MajorGrid.IsVisible = false;
			myPaneT.XAxis.MinorGrid.IsVisible = false;
			// Remove all margins
			myPaneT.Margin.All = 0;
	    
			// Use DateAsOrdinal to skip weekend gaps
			if( chartType == ChartType.Bar) {
				myPaneT.XAxis.Type = AxisType.DateAsOrdinal;
				_clusterWidth = 1.25F;
			} else {
				myPaneT.XAxis.Type = AxisType.Date;
			    XDate size = new XDate(0d);
			    size.AddSeconds(intervalChartBar.Seconds);
			    _clusterWidth = size;
			}
	   
			// Except, leave some top margin on the first GraphPane
			if ( master.PaneList.Count == 0 )
				myPaneT.Margin.Top = 20;
			
			
			if ( master.PaneList.Count > 0 ) {
				 myPaneT.YAxis.Scale.IsSkipLastLabel = true;
			}
			
			// This sets the minimum amount of space for the left and right side, respectively
			// The reason for this is so that the ChartRect's all end up being the same size.
				
			myPaneT.IsFontsScaled = false;
			master.Add( myPaneT);
			return myPaneT;
		}
		
		void layoutLastPane() {
			MasterPane master = dataGraph.MasterPane;
			GraphPane myPaneT = master.PaneList[master.PaneList.Count-1];
			myPaneT.Margin.Bottom = 10;
		}
	
		[Browsable(false)]
		public List<IndicatorInterface> Indicators {
			get { return indicators; }
		}
		
		public int DrawText(string text, Color color, int bar, double y, Positioning orient) {
			double x = barToXAxis(bar);
			TextObj textObj = new TextObj(text, x,y);
			textObj.IsClippedToChartRect=true;
			switch( orient) {
				case Positioning.UpperLeft:
					textObj.Location.AlignH = AlignH.Left;
					textObj.Location.AlignV = AlignV.Top;
					break;
				case Positioning.LowerLeft:
					textObj.Location.AlignH = AlignH.Left;
					textObj.Location.AlignV = AlignV.Bottom;
					break;
				case Positioning.LowerRight:
					textObj.Location.AlignH = AlignH.Right;
					textObj.Location.AlignV = AlignV.Bottom;
					break;
				case Positioning.UpperRight:
					textObj.Location.AlignH = AlignH.Right;
					textObj.Location.AlignV = AlignV.Top;
					break;
			}
			textObj.FontSpec.FontColor = color;
			textObj.FontSpec.Border.IsVisible = false;
			textObj.FontSpec.Fill.IsVisible = false;
			objectId++;
		   
			textObjs.Add(objectId,textObj);
		    priceGraphPane.GraphObjList.Add(textObj);
		   
			return objectId;
		}
		
		public int DrawBox(Color color, int bar, double y) {
			double width = 0.25;
			double height = 10;
			double x = barToXAxis(bar);
			double xwidth = barToXAxis(x+width) - barToXAxis(x);
			BoxObj box = new BoxObj(x-xwidth/2,y+height/2,xwidth,height,color,color);
			box.IsClippedToChartRect=true;
			objectId++;
			
			graphObjs.Add(objectId,box);
		    priceGraphPane.GraphObjList.Add(box);
			return objectId;
		}
		
		public int DrawBox(Color color, int bar, double y, double width, double height) {
			double x = barToXAxis(bar);
			BoxObj box = new BoxObj(x,y,width,height,color,color);
			box.IsClippedToChartRect=true;
			box.ZOrder = ZOrder.E_BehindCurves;
			objectId++;
			graphObjs.Add(objectId,box);
		    priceGraphPane.GraphObjList.Add(box);
			return objectId;
		}
		
		Dictionary<int,GraphObj> graphObjs = new Dictionary<int,GraphObj>();
		Dictionary<int,GraphObj> textObjs = new Dictionary<int,GraphObj>();
		
		public int DrawTrade(LogicalOrder order, double fillPrice, double resultingPosition)
		{
			return 0;
		}
		public int DrawArrow( Color color, float size, int bar1, double y1, int bar2, double y2) {
			ArrowObj arrow = CreateArrow(color,size,bar1,y1,bar2,y2);
			objectId++;
			graphObjs.Add(objectId,arrow);
		    priceGraphPane.GraphObjList.Add(arrow);
			return objectId;
		}
		
		private ArrowObj CreateArrow( Color color, float size, int bar1, double y1, int bar2, double y2) {
			double x1 = barToXAxis(bar1);
			double x2 = barToXAxis(bar2);
			ArrowObj arrow = new ArrowObj(color,size,x1,y1,x2,y2);
			arrow.IsClippedToChartRect=true;
			arrow.Location.CoordinateFrame = CoordType.AxisXYScale;
			return arrow;
		}
	
		public int DrawLine( Color color, Point p1, Point p2, LineStyle style) {
			LineObj line = CreateLine(color,p1.X,p1.Y,p2.X,p2.Y,style);
			objectId++;
			graphObjs.Add(objectId,line);
		    priceGraphPane.GraphObjList.Add(line);
			return objectId;
		}
		
		private float ClusterWidth {
			get { return _clusterWidth; }
		}
		
		public double barToXAxis(double bar) {
			double ret = 0;
			if( priceGraphPane.XAxis.Scale.IsAnyOrdinal) {
				ret = bar;
			} else {
				ret = spl[spl.Count-1].X + (bar-(spl.Count-1))*ClusterWidth;
				ret-=ClusterWidth;
			}
			return ret;
		}
		
		public int DrawLine( Color color, int bar1, double y1, int bar2, double y2, LineStyle style) {
			
			double x1 = barToXAxis(bar1);
			double x2 = barToXAxis(bar2);
			LineObj line = CreateLine(color,x1,y1,x2,y2,style);
			objectId++;
			graphObjs.Add(objectId,line);
		    priceGraphPane.GraphObjList.Add(line);
			return priceGraphPane.GraphObjList.Count-1;
		}
		
		public void ChangeLine( int lineId, Color color, int bar1, double y1, int bar2, double y2, LineStyle style) {
			LineObj line = CreateLine(color,barToXAxis(bar1),y1,barToXAxis(bar2),y2,style);
	//			graphObjs[lineId] = line;
			priceGraphPane.GraphObjList[lineId] = line;
		}
		
		private LineObj CreateLine( Color color, double x1, double y1, double x2, double y2, LineStyle style) {
			LineObj line = new LineObj(color,x1,y1,x2,y2);
			line.IsClippedToChartRect=true;
			line.Location.CoordinateFrame = CoordType.AxisXYScale;
			switch( style) {
				case LineStyle.Dashed:
					line.Line.Style = System.Drawing.Drawing2D.DashStyle.Custom;
					line.Line.DashOn = 7;
					line.Line.DashOff = 5;
					break;
				case LineStyle.Solid:
					line.Line.Style = System.Drawing.Drawing2D.DashStyle.Solid;
					break;
				case LineStyle.Dotted:
					line.Line.Style = System.Drawing.Drawing2D.DashStyle.Dot;
					break;
			}
			line.Line.Width = 2;
			return line;
		}
		
		Dictionary<ModelInterface,PointPairList> repeatList = new Dictionary<ModelInterface,PointPairList>();
		
		int lastColorValue = 2;
		/// <summary>
		/// Obsolete. Please use only chartBars argument instead
		/// </summary>
		[Obsolete("Please use only chartBars argument instead.",true)]
		public void AddBar( Bars updateSeries, Bars displaySeries) {
			throw new NotImplementedException();
		}
		
		public void AddBar( Bars chartBars) {
			lock( chartLocker) {
				double time = chartBars.Time[0].ToOADate();
	    		if( firstTime == TimeStamp.MinValue) {
	    			firstTime = chartBars.Time[0];
	    		}
				// Set the default bar color
	        	lastColorValue = 2;
		        //if price is increasing color=black, else color=red
		        lastColorValue = chartBars.Close[0] > chartBars.Open[0] ? 0 : 1;
				
				string paintBarName = "";
			
			    // Update lines for all indicators.
			    for( int j = 0; j < indicators.Count; j++) {
	    			IndicatorInterface indicator = indicators[j];
	    			if( lineList.Count <= j) {
						lineList.Add( new PointPairList());
	    			}
	    			if( indicator.Count > 0) { 
						while( lineList[j].Count <= chartBars.CurrentBar) {
							lineList[j].Add(double.NaN,double.NaN);
						}
	    				double val = indicator[0];
	    				int colorIndex = 1;
						try {
		    				colorIndex = indicator.Drawing.ColorIndex;
						} catch ( ApplicationException) {
						}
						PointPair ppair;
						switch( indicator.Drawing.GraphType ) {
	    					case GraphType.PaintBar:
		    					if( paintBarName.Length > 0) {
		    						throw new ApplicationException( "Only one paint bar: " + paintBarName + 
		    						                           ". Found conflicting paint bar: " + indicator.Name);
		    					} else {
		    						paintBarName = indicator.Name;
		    					}
		    					lastColorValue = (int) indicator[0];
		    					break;
		    				case GraphType.Histogram:
		    					int startBar = 0; 
			    				ppair = lineList[j][chartBars.CurrentBar];
			    				ppair.X = time;
			    				ppair.Y = val;
			    				ppair.Z = colorIndex;
			    				startBar ++;
		    					PointPairList ppList;
	    						if( repeatList.TryGetValue(indicator, out ppList) == false) {
	    							ppList = new PointPairList();
	    							repeatList[indicator] = ppList;
		    					}
								while( ppList.Count <= chartBars.CurrentBar) {
									ppList.Add(double.NaN,double.NaN);
								}
		    					// TODO: Make pplist an array. Wrap Indicator in IndicatorGraph object.
	//		    					 for( int repeat=1; repeat < indicator.GraphRepeat && repeat < indicator.Count; repeat++, startBar++) {
	//				    				val = indicator[startBar];
	//				    				colorIndex = 1;
	//									if( formula.checkWaitTillReady()) {
	//										try {
	//						    				colorIndex = indicator.GetColorIndex(startBar);
	//										} catch ( BeyondCircularException e) {
	//											formula.WaitTillReady.Add( e);
	//										}
	//				    				}
	//				    				ppair = ppList[series.CurrentBar];
	//				    				ppair.X = time;
	//				    				ppair.Y = val;
	//				    				ppair.Z = colorIndex;
	//		    					}
		    					break;
		    				default:
			    				ppair = lineList[j][chartBars.CurrentBar];
			    				ppair.X = time;
			    				ppair.Y = val;
			    				ppair.Z = colorIndex;
			    				break;
	    				}
	    			}
	    		}			
			    
			    if( showPriceGraph) {
		    		StockPt pt;
		    		if( spl.Count <= chartBars.CurrentBar ) {
		    			time = chartBars.Time[0].ToOADate();
						double high = chartBars.High[0];
				        double low = chartBars.Low[0];
				        double open = chartBars.Open[0];
				        double close = chartBars.Close[0];
						pt = new StockPt( time, chartBars.High[0],
				                                 chartBars.Low[0],
				                                 chartBars.Open[0],
				                                 chartBars.Close[0], 10000 );
				        //if price is increasing color=black, else color=red
				        pt.ColorValue = lastColorValue;
						spl.Add( pt );
		    		} else {
					    pt = (StockPt) spl.GetAt(chartBars.CurrentBar);
					    // Update the bar on the chart.
						pt.High = chartBars.High[0];
						pt.Low = chartBars.Low[0];
						pt.Open = chartBars.Open[0];
				        pt.Close = chartBars.Close[0];
				        pt.ColorValue = lastColorValue;
		    		}
		    		UpdateScaleCheck( chartBars );
		
			    }
			}
		}
		
		public void OnInitialize() {
		    InitializeChart( );
			if( !priceGraphPane.XAxis.Scale.IsAnyOrdinal) {
//				SetDefaultScale( tick.Price, (double) tick.Time);
			}
		}
		
		private void SetDefaultScale(double price, double time) {
			double yHeight = 800;
			Scale yScale = priceGraphPane.YAxis.Scale;
			yScale.Max = price + yHeight/2;
			yScale.Min = price - yHeight/2;
			double xWidth = ClusterWidth * 40;
			Scale xScale = priceGraphPane.XAxis.Scale;
			xScale.Max = time + xWidth * .30;
			xScale.Min = time - xWidth * .70;
			SetCommonXScale( xScale.Min, xScale.Max);
			dataGraph.AxisChange();
			dataGraph.Invalidate();
		}
		
		double lastHigh;
		double lastLow;
		double lastTime;
		double lastBar;
		
		void UpdateScaleCheck(Bars series) {
			lastHigh = series.High[0];
			lastLow = series.Low[0];
			lastTime = series.Time[0].ToOADate();
			lastBar = series.CurrentBar;	
		}
	
		bool KeepWithinScale() {
			Scale yScale = priceGraphPane.YAxis.Scale;
			if( lastLow > yScale.Max || lastHigh < yScale.Min) {
				dataGraph.AxisChange();
				return false;
			}
			double yHeight = yScale.Max - yScale.Min;
			double yUpperLimit = yScale.Max - yHeight/4;
			double yLowerLimit = yScale.Min + yHeight/4;
			bool reset = false;
			if( lastHigh > yUpperLimit ) {
				double yMax = MoveByPixels(yScale,yScale.Max,-1);
				double yMin = MoveByPixels(yScale,yScale.Min,-1);
				if( !double.IsNaN(yMax) && !double.IsNaN(yMin)) {
					yScale.Max = yMax;
					yScale.Min = yMin;
					reset = true;
				}
	        }
			if( lastLow < yLowerLimit ) {
				double yMax = MoveByPixels(yScale,yScale.Max,1);
				double yMin = MoveByPixels(yScale,yScale.Min,1);
				if( !double.IsNaN(yMax) && !double.IsNaN(yMin)) {
					yScale.Max = yMax;
					yScale.Min = yMin;
					reset = true;
				}
			}
			Scale xScale = priceGraphPane.XAxis.Scale;
			double xCurrent;
			if( priceGraphPane.XAxis.Scale.IsAnyOrdinal) {
				xCurrent = lastBar;
			} else {
				xCurrent = lastTime;
			}
			if( xCurrent > xScale.Max || xCurrent < xScale.Min) {
				dataGraph.AxisChange();
				return false;
			}
			double xWidth = xScale.Max - xScale.Min;
			double xUpperLimit = xScale.Max - xWidth/6;
			double xLowerLimit = xScale.Max - xWidth/3;
			if( xCurrent > xUpperLimit) {
				resetXScale = true;
			}
			if( resetXScale && xCurrent > xLowerLimit) {
				double xMin = MoveByPixels(xScale,xScale.Min,1);
				double xMax = MoveByPixels(xScale,xScale.Max,1);
				SetCommonXScale( xMin, xMax);
				reset = true;
			} else {
				resetXScale = false;
			}
			return reset;
		}
		
		double MoveByPixels( Scale scale, double value, float movePixels) {
			float rawPixels = scale.Transform(value);
			float currPixels = (float) Math.Round(rawPixels);
			return scale.ReverseTransform(currPixels+movePixels);
		}
		
		void SetCommonXScale(double min, double max) {
			if( !double.IsNaN(max) && !double.IsNaN(min) ) {
				if(  dataGraph != null && dataGraph.MasterPane != null) {
					PaneList list = dataGraph.MasterPane.PaneList;
					for( int i=0; i<list.Count; i++) {
						list[i].XAxis.Scale.Max = max;
						list[i].XAxis.Scale.Min = min;
					}
				}
			}
		}
		
		bool resetXScale = false;
	
		OHLCBarItem ohlcCurve;
		GraphPane priceGraphPane;
		
		public void DrawChart() {
		   priceGraphPane.IsBoundedRanges=true;
		   // Setup the gradient fill...
		   // Use Red for negative days and black for positive days
		   Fill myFill = new Fill( colors );
		   myFill.Type = FillType.GradientByColorValue;
		   myFill.SecondaryValueGradientColor = Color.Empty;
		   myFill.RangeMin = 0;
		   myFill.RangeMax = colors.Length-1;
	
		   //Create the OHLC and assign it a Fill
		   ohlcCurve = priceGraphPane.AddOHLCBar( "Price", spl, Color.Empty );
		   
		   ohlcCurve.Bar.GradientFill = myFill;
	   	   ohlcCurve.Bar.Size = ClusterWidth;
		   ohlcCurve.Bar.IsAutoSize = true;
		   
		   if( priceGraphPane != null && dataGraph.MasterPane != null) {
				CreateIndicators();
		   }
		   
		   setLayout();
		   // Calculate the Axis Scale Ranges
		   dataGraph.AxisChange();
		}
		
	    Dictionary<string,GraphPane> signalPaneList = new Dictionary<string,GraphPane>();
	    Dictionary<string,GraphPane> secondaryPaneList = new Dictionary<string,GraphPane>();
	    
	    bool layoutChange = false;
		void CreateIndicators() {
		   //Create the indicators
		   for( int i = 0; i < indicators.Count; i++) {
	   			   ModelInterface indicator = indicators[i];
			   if( lineList.Count <= i) {
				   lineList.Add( new PointPairList());
			   } 
		   	   PointPairList pplist = lineList[i];
		   	   if(!indicator.Drawing.AlreadyDrawn) {
					Color color = indicator.Drawing.Color;
					GraphPane gp;
					string groupName = indicator.Drawing.GroupName;
					switch( indicator.Drawing.GraphType ) {
						case GraphType.FilledLine:
						case GraphType.Line:
					   	   	switch( indicator.Drawing.PaneType )
					   	   	{
					   	   		case PaneType.Primary:
									LineItem indCurve = priceGraphPane.AddCurve( indicator.Name, pplist, indicator.Drawing.Color, SymbolType.None);
									indCurve.IsY2Axis = false;
							   	   	indicator.Drawing.AlreadyDrawn = true;
							   	   	layoutChange = true;
							   	   	if( indicator.Drawing.GraphType == GraphType.FilledLine) {
								   	   	indCurve.Line.Fill = new Fill( indicator.Drawing.Color );
							   	   	}
									break;
								case PaneType.Secondary:
						   	   		if( secondaryPaneList.TryGetValue( groupName, out gp) == false) {
						   	   			gp = createPane();
						   	   			gp.YAxis.Title.Text = groupName;
							   	   		secondaryPaneList[groupName] = gp;
						   	   		}
									gp.IsBoundedRanges=true;
							   	   	indicator.Drawing.AlreadyDrawn = true;
							   	   	layoutChange = true;
						   	   		indCurve = gp.AddCurve( indicator.Name, pplist, color, SymbolType.None);
									if( ! Double.IsNaN(indicator.Drawing.ScaleMax)) {
										gp.YAxis.Scale.Max = indicator.Drawing.ScaleMax;
									}
									if( ! Double.IsNaN(indicator.Drawing.ScaleMin)) {
										gp.YAxis.Scale.Min = indicator.Drawing.ScaleMin;
									}
							   	   	if( indicator.Drawing.GraphType == GraphType.FilledLine) {
								   	   	indCurve.Line.Fill = new Fill( indicator.Drawing.Color );
							   	   	}
						   	   		break;
						   	   	case PaneType.OverlayPrimary:
									indCurve = priceGraphPane.AddCurve( indicator.Name, pplist, indicator.Drawing.Color, SymbolType.None);
									indCurve.IsY2Axis = true;
							   	   	indicator.Drawing.AlreadyDrawn = true;
							   	   	layoutChange = true;
									if( ! Double.IsNaN(indicator.Drawing.ScaleMax)) {
										priceGraphPane.Y2Axis.Scale.Max = indicator.Drawing.ScaleMax;
									}
									if( ! Double.IsNaN(indicator.Drawing.ScaleMin)) {
										priceGraphPane.Y2Axis.Scale.Min = indicator.Drawing.ScaleMin;
									}
							   	   	if( indicator.Drawing.GraphType == GraphType.FilledLine) {
								   	   	indCurve.Line.Fill = new Fill( indicator.Drawing.Color );
							   	   	}
									break;
					   	   	}
					   	   	break;
					   	case GraphType.Histogram:
				   	   		if( secondaryPaneList.TryGetValue( groupName, out gp) == false) {
				   	   			gp = createPane();
				   	   			gp.YAxis.Title.Text = groupName;
					   	   		secondaryPaneList[groupName] = gp;
				   	   		}
							gp.IsBoundedRanges=true;
					   	   	indicator.Drawing.AlreadyDrawn = true;
					   	   	layoutChange = true;
					   	   	// Setup to either red or Black fill color.
				   	   		Fill myFill = new Fill( colors);
							myFill.Type = FillType.GradientByZ;
							myFill.RangeMin = 1;
							myFill.RangeMax = 4;
							myFill.RangeDefault = 1;
							
							myFill.SecondaryValueGradientColor = Color.Empty;
							
							BarItem indBar;
							
		   					PointPairList ppList;
	    						if( repeatList.TryGetValue(indicator, out ppList) == false) {
	    							ppList = new PointPairList();
	    							repeatList[indicator] = ppList;
	    					}
		   					
				   	   		indBar = gp.AddBar( indicator.Name, pplist, color);
						    gp.BarSettings.ClusterScaleWidth = ClusterWidth;
						    BarSettings settings = gp.BarSettings;
							indBar.Bar.Fill = myFill;
							indBar.Bar.Border.IsVisible=false;
							
							gp.BarSettings.ClusterScaleWidthAuto=false;
							if( ! Double.IsNaN(indicator.Drawing.ScaleMax)) {
								gp.YAxis.Scale.Max = indicator.Drawing.ScaleMax;
							}
							if( ! Double.IsNaN(indicator.Drawing.ScaleMin)) {
								gp.YAxis.Scale.Min = indicator.Drawing.ScaleMin;
							}
							break;
					}
	
		   	   }
		   }
		   
		   //Create the signals
		   for( int i = 0; i < lineList.Count; i++) {
	   			   ModelInterface indicator = indicators[i];
		   	   PointPairList pplist = lineList[i];
		   	   if( !indicator.Drawing.AlreadyDrawn) {
				   switch( indicator.Drawing.PaneType ) {
			   	   	case PaneType.Signal:
			   	   		GraphPane gp;
			   	   		if( signalPaneList.TryGetValue( indicators[i].Name, out gp) == false) {
			   	   			gp = createPane();
			   	   			gp.YAxis.Title.Text = indicator.Drawing.GroupName;
				   	   		signalPaneList[indicators[i].Name] = gp;
			   	   		}
						gp.AddCurve( indicators[i].Name, pplist, indicators[i].Drawing.Color, SymbolType.None);
			   	   		indicator.Drawing.AlreadyDrawn = true;
				   	   	layoutChange = true;
						break;
		   	   		}
		   	   }
		   }
		}
		
		private bool DataGraphMouseMoveEvent(ZedGraph.ZedGraphControl sender, System.Windows.Forms.MouseEventArgs e)
		{
			try { 
		        GraphPane myPane = dataGraph.GraphPane;
				// Save the mouse location
				PointF mousePt = new PointF( e.X, e.Y );
				int dragIndex;
				StockPt startPair;
				
				GraphPane.Default.NearestTol = 200.00f;
				
				// find the point that was clicked, and make sure the point list is editable
				// and that it's a primary Y axis (the first Y or Y2 axis)
				double curX, curY;
		        myPane.ReverseTransform( mousePt, out curX, out curY);
	        	dragIndex = 0;
				// save the starting point information
				if( spl.Count > 0 ) {
					if( priceGraphPane.XAxis.Scale.IsAnyOrdinal) {
						dragIndex = Math.Min(spl.Count-1,Math.Max(0,(int) Math.Round(curX-1)));
					} else {
						for( int i = 0; i<spl.Count; i++) {
							if( spl[i].X-(ClusterWidth/2) <= curX) {
								dragIndex = i;
							}
						}
					}
					startPair = (StockPt) spl[dragIndex];
					TimeStamp time = new TimeStamp(startPair.X);
					MainForm mainForm = MainForm.Instance;
					if( isCompactMode) {
						mainForm.ToolStripStatusXY.Text = time.ToString() + ", " +
							startPair.Close.ToString("f0");
					} else {
						    mainForm.ToolStripStatusXY.Text = time.ToString() + " " + 
							time.ToString() + ", " +
							"O:" + startPair.Open.ToString(",0.000") + ", " + 
							"H:" + startPair.High.ToString(",0.000") + ", " + 
							"L:" + startPair.Low.ToString(",0.000") + ", " + 
							"C:" + startPair.Close.ToString(",0.000") + ", " +
							"Bar: " + (dragIndex+1) + ", " +
							"Period: " + intervalChartBar;
					}
				}
			} catch( Exception ex) {
				log.Notice(ex.ToString());
			}
		   // Return false to indicate we have not processed the MouseMoveEvent
		   // ZedGraphControl should still go ahead and handle it
		   return false;
		}
		
		void refreshTick(object sender, EventArgs e)
		{
			try {
				if( Visible) {
					lock( chartLocker) {
						System.Windows.Forms.Timer timer = (System.Windows.Forms.Timer) sender;
						if( layoutChange) {
							setLayout();
							layoutChange = false;
						}
						if( spl.Count > 0 && isDynamicUpdate ) {
							if( isAutoScroll || !priceGraphPane.XAxis.Scale.IsAnyOrdinal) {
								if( KeepWithinScale()) {
									timer.Interval = 15;
								} else {
									timer.Interval = 100;
								}
							}
							dataGraph.AxisChange();
							dataGraph.Invalidate();
						}
					}
				}
			} catch( Exception ex) {
				log.Notice( ex.ToString());
			}
		}
		
		public bool ShowPriceGraph {
			get { return showPriceGraph; }
			set { showPriceGraph = value; }
		}
		
		[Browsable(false)]
		public StrategyInterface StrategyForTrades {
			get { return strategy; }
			set { strategy = value; }
		}
		
		[Browsable(false)]
		public bool IsDynamicUpdate {
			get { return isDynamicUpdate; }
			set { isDynamicUpdate = value; }
		}
		
		
		public bool ProcessKeys(Keys keyData) {
			bool blnProcess = false;
			if( keyData == Keys.Up ) {
				strategy.Position.Change(1);
				blnProcess = true;
			}
			if( keyData == Keys.Down ) {
				strategy.Position.Change(-1);
				blnProcess = true;
			}
			if( keyData == Keys.Right) {
				strategy.Position.Change(- strategy.Position.Current);
				blnProcess = true;
			}
			if( keyData == Keys.Insert || keyData == Keys.D0 || keyData == Keys.NumPad0 ) {
				// Go flat.
				strategy.Position.Change(0);
				blnProcess = true;
			}
			return blnProcess;
		}
		
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (ProcessKeys(keyData) == true) {
				return true;
			} else {
				return base.ProcessCmdKey(ref msg, keyData);
			}
		}
		
		// TODO: Move the TopMost setting to the form
//		void CheckBoxOnTopCheckStateChanged(object sender, EventArgs e)
//		{
//			if( checkBoxOnTop.Checked) {
//				TopMost = true;
//			} else {
//				TopMost = false;
//			}
//		}
		
		
		void ButtonVolumeTestClick(object sender, EventArgs e)
		{
		}
		
//		void AudioNotifyCheckStateChanged(object sender, EventArgs e)
//		{
//			if( audioNotify.Checked) {
//				isAudioNotify = true;
//				// For volume test.
//				AudioNotify( Audio.RisingVolume);
//				Thread.Sleep(5000);
//				AudioNotify( Audio.IntervalChime);
//			} else {
//				isAudioNotify = false;
//			}
//		}
		
		void DataGraphContextMenuBuilder(ZedGraphControl sender, ContextMenuStrip menuStrip, Point mousePt, ZedGraph.ZedGraphControl.ContextMenuObjectState objState)
		{
			try { 
				ToolStripMenuItem item;
				if( IsDynamicUpdate) {
					// create a new menu item
					item = new ToolStripMenuItem();
					// This is the user-defined Tag so you can find this menu item later if necessary
					item.Name = "zoom_last";
					item.Tag = "zoom_last";
					// This is the text that will show up in the menu
					item.Text = "Zoom to Last";
					// Add a handler that will respond when that menu item is selected
					item.Click += new System.EventHandler( SetDefaultScale );
					// Add the menu item to the menu
					menuStrip.Items.Add( item );
				}
				if( IsDynamicUpdate) {
					// create a new menu item
					item = new ToolStripMenuItem();
					// This is the user-defined Tag so you can find this menu item later if necessary
					item.Name = "auto_scroll";
					item.Tag = "auto_scroll";
					// This is the text that will show up in the menu
					if( isAutoScroll) {
						item.Text = "Disable Auto Scroll";
					} else {
						item.Text = "Enable Auto Scroll";
					}
					// Add a handler that will respond when that menu item is selected
					item.Click += new System.EventHandler( ToggleAutoScroll );
					// Add the menu item to the menu
					menuStrip.Items.Add( item );
				}
				
				// create a new menu item
				item = new ToolStripMenuItem();
				// This is the user-defined Tag so you can find this menu item later if necessary
				item.Name = "compact_mode";
				item.Tag = "compact_mode";
				// This is the text that will show up in the menu
				if( isCompactMode) {
					item.Text = "Disable Compact";
				} else {
					item.Text = "Enable Compact";
				}
				// Add a handler that will respond when that menu item is selected
				item.Click += new System.EventHandler( ToggleCompactMode );
				// Add the menu item to the menu
				menuStrip.Items.Add( item );
			} catch( Exception ex) {
				log.Notice(ex.ToString());
			}
		}
		
		void ToggleAutoScroll(object sender, EventArgs e) {
			isAutoScroll = !isAutoScroll;	
		}
			
		void ToggleCompactMode(object sender, EventArgs e) {
			isCompactMode = !isCompactMode;	
			for( int i=0; i<dataGraph.MasterPane.PaneList.Count; i++) {
				GraphPane myPaneT = dataGraph.MasterPane.PaneList[i];
				if( isCompactMode) {
					myPaneT.YAxis.Title.IsVisible = false;
					myPaneT.YAxis.Scale.IsVisible = false;
					myPaneT.YAxis.MinSpace = 10;
				} else {
					myPaneT.YAxis.Title.IsVisible = true;
					myPaneT.YAxis.Scale.IsVisible = true;
					myPaneT.YAxis.MinSpace = 80;
				}
			}
		}
		
		void SetDefaultScale(object sender, EventArgs e) {
			SetDefaultScale( spl[spl.Count-1].Y, spl[spl.Count-1].X);
		}
	
		public ChartType ChartType {
			get { return chartType; }
			set { chartType = value; }
		}
		
		/// <summary>
		/// Obsolete. Please use only ChartBars instead
		/// </summary>
		[Obsolete("Please use only ChartBars instead.",true)]
		public Bars UpdateBars {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException();  }
		}

		/// <summary>
		/// Obsolete. Please use only ChartBars instead
		/// </summary>
		[Obsolete("Please use only ChartBars instead.",true)]
		public Bars DisplayBars {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException();  }
		}
		
		Bars chartBars;
		[Browsable(false)]
		public Bars ChartBars {
			get { return chartBars; }
			set { chartBars = value; }
		}

		public Interval IntervalChartBar {
			get { return intervalChartBar; }
			set { intervalChartBar = value; }
		}
		
		/// <summary>
		/// Obsolete. Please use only IntervalChartBar instead.
		/// </summary>
		[Obsolete("Please use only IntervalChartBar instead.",true)]
		public Interval IntervalChartDisplay {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		
		/// <summary>
		/// Obsolete. Please use only IntervalChartBar instead.
		/// </summary>
		[Obsolete("Please use only IntervalChartBar instead.",true)]
		public Interval IntervalChartUpdate {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		
		public SymbolInfo Symbol {
			get { return symbol; }
			set { symbol = value; }
		}
    	
		public int DrawArrow(TickZoom.Api.ArrowDirection direction, Color color, float size, int bar, double price)
		{
			throw new NotImplementedException();
		}
	}
}
