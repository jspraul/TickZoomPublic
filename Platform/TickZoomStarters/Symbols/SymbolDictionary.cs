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
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

using TickZoom.Api;
using TickZoom.Common;

namespace TickZoom.Symbols
{
	/// <summary>
	/// Description of SymbolDictionary.
	/// </summary>
	public class SymbolDictionary : IEnumerable<SymbolProperties>
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(SymbolDictionary));
		private static readonly bool trace = log.IsTraceEnabled;
		private static readonly bool debug = log.IsDebugEnabled;
		private static object locker = new object();
		private SymbolProperties @default;
		private List<SymbolCategory> categories = new List<SymbolCategory>();
		
		public SymbolDictionary()
		{
			@default = new SymbolProperties();
		}

		public static SymbolDictionary Create(string name, string defaultContents) {
			lock( locker) {
				string storageFolder = Factory.Settings["AppDataFolder"];
				string dictionaryPath = storageFolder + @"\Dictionary\"+name+".tzdict";
				Directory.CreateDirectory(Path.GetDirectoryName(dictionaryPath));
				SymbolDictionary dictionary;
				if( File.Exists(dictionaryPath) ) {
					using( StreamReader streamReader = new StreamReader(new FileStream(dictionaryPath,FileMode.Open,FileAccess.Read,FileShare.Read))) {
						dictionary = SymbolDictionary.Create( streamReader);
					}
					return dictionary;
				} else {
					string contents = BeautifyXML(defaultContents);
			        using (StreamWriter sw = new StreamWriter(dictionaryPath)) 
			        {
			            // Add some text to the file.
			            sw.Write( contents);
			        }
			        Thread.Sleep(1000);
					dictionary = SymbolDictionary.Create( new StreamReader(dictionaryPath));
				}
				return dictionary;
			}
		}
		
		
		public static SymbolDictionary Create(TextReader projectXML) {
			lock( locker) {
				SymbolDictionary project = new SymbolDictionary();
				project.Load(projectXML);
				return project;
			}
		}
		
		private static string BeautifyXML(string xml)
		{
			using( StringReader reader = new StringReader(xml)) {
				XmlDocument doc = new XmlDocument();
				doc.Load( reader);
			    StringBuilder sb = new StringBuilder();
			    XmlWriterSettings settings = new XmlWriterSettings();
			    settings.Indent = true;
			    settings.IndentChars = "  ";
			    settings.NewLineChars = "\r\n";
			    settings.NewLineHandling = NewLineHandling.Replace;
			    using( XmlWriter writer = XmlWriter.Create(sb, settings)) {
				    doc.Save(writer);
			    }
			    return sb.ToString();
			}
		}
			
		public void Load(TextReader projectXML) {
			
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			settings.IgnoreWhitespace = true;
			
			using (XmlReader reader = XmlReader.Create(projectXML))
			{
				try {
					bool process = true;
					// Read nodes one at a time  
					while( process)  
					{  
						reader.Read();
					    // Print out info on node  
					    switch( reader.NodeType) {
					    	case XmlNodeType.Element:
					    		if( "category".Equals(reader.Name) ) {
				    				SymbolCategory category = new SymbolCategory();
						    		HandleCategory(category,reader);
						    		categories.Add(category);
					    		} else {
					    			Error(reader,"unexpected tag " + reader.Name );
					    		}
					    		projectXML.Close();
					    		process = false;
					    		break;
					    }
					}  				
				} catch( Exception ex) {
					Error( reader, ex.ToString());
					projectXML.Close();
				}
			}
			projectXML.Close();
			projectXML.Dispose();
		}
		
		private void HandleCategory(SymbolCategory category, XmlReader reader) {
			string tagName = reader.Name;
			category.Name = reader.GetAttribute("name");
			log.Debug("Handle category " + category.Name);
			if( reader.IsEmptyElement) { return; }
			log.Indent();
			while( reader.Read()) {
			    // Print out info on node  
			    switch( reader.NodeType) {
			    	case XmlNodeType.Element:
			    		if( "property".Equals(reader.Name) ) {
			    			string name = reader.GetAttribute("name");
			    			string value = reader.GetAttribute("value");
			    			HandleProperty(reader,category.Default, reader.GetAttribute("name"), reader.GetAttribute("value"));
			    			if( trace) log.Trace("Property " + name + " = " + value);
			    		} else if( "category".Equals(reader.Name)) {
			    			SymbolCategory subCategory = new SymbolCategory(category.Default.Copy());
			    			HandleCategory(subCategory,reader);
			    			category.Categories.Add(subCategory);
			    		} else if( "symbol".Equals(reader.Name)) {
			    			string name = reader.GetAttribute("name");
			    			string universal = reader.GetAttribute("universal");
			    			SymbolProperties symbol = category.Default.Copy();
		    				symbol.Symbol = name;
		    				if( universal != null) {
//		    					symbol.UniversalSymbol = universal;
		    				}
			    			HandleSymbol(symbol,reader);
			    			category.Symbols.Add(symbol);
			    		} else {
			    			Error(reader,"unexpected tag " + reader.Name );
			    		}
			    		break;
			    	case XmlNodeType.EndElement:
			    		if( tagName.Equals(reader.Name)) {
			    			log.Outdent();
				    		return;
			    		} else {
			    			Error(reader,"End of " + tagName + " was expected instead of end of " + reader.Name);
			    		}
			    		break;
			    } 
			}
			Error(reader,"Unexpected end of file");
			return;
		}
		
		
		private void HandleSymbol(object obj, XmlReader reader) {
			string tagName = reader.Name;
			if( trace) log.Trace("Handle " + obj.GetType().Name);
			if( reader.IsEmptyElement) { return; }			
			log.Indent();
			while( reader.Read()) {
			    // Print out info on node  
			    switch( reader.NodeType) {
			    	case XmlNodeType.Element:
			    		if( "property".Equals(reader.Name) ) {
			    			HandleProperty(reader,obj, reader.GetAttribute("name"), reader.GetAttribute("value"));
			    		} else {
			    			Error(reader,"End of " + tagName + " was expected instead of end of " + reader.Name);
			    		}
			    		break;
			    	case XmlNodeType.EndElement:
			    		if( tagName.Equals(reader.Name)) {
			    			log.Outdent();
				    		return;
			    		} else {
			    			Error(reader,"End of " + reader.Name + " tag in xml was unexpected");
			    		}
			    		break;
			    }
			}
			Error(reader,"Unexpected end of file");
		}
		
		private void HandleProperty( XmlReader reader, object obj, string name, string str) {
			PropertyInfo property = obj.GetType().GetProperty(name);
			if( property == null) {
				Warning(reader,obj.GetType() + " does not have the property: " + name);
				return;
			}
			Type propertyType = property.PropertyType;
			object value = TickZoom.Api.Converters.Convert(propertyType,str);
			property.SetValue(obj,value,null);
			if( trace) log.Trace("Property " + property.Name + " = " + value);
		}
		
		private void Error( XmlReader reader, string msg) {
			IXmlLineInfo lineInfo = reader as IXmlLineInfo;
			string lineStr = "";
			if( lineInfo != null) {
				lineStr += " on line " + lineInfo.LineNumber + " at position " + lineInfo.LinePosition;
			}
			log.Debug(msg + lineStr);
			throw new ApplicationException(msg + lineStr);
		}
		
		private void Warning( XmlReader reader, string msg) {
			IXmlLineInfo lineInfo = reader as IXmlLineInfo;
			string lineStr = "";
			if( lineInfo != null) {
				lineStr += " on line " + lineInfo.LineNumber + " at position " + lineInfo.LinePosition;
			}
			log.Warn(msg + lineStr);
		}
		
		public SymbolProperties Get(string symbol) {
			foreach( SymbolProperties properties in this) {
				if( symbol == properties.Symbol) {
					return properties;
				}
			}
			throw new ApplicationException("Symbol " + symbol + " was not found in the dictionary.");
		}
		
		
		public IEnumerator<SymbolProperties> GetEnumerator()
		{
			foreach( SymbolCategory category in categories) {
				foreach( SymbolProperties properties in category) {
					yield return properties;
				}
			}
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
#region UNIVERSAL_DICTIONARY
		public static string UniversalDictionary = @"<?xml version=""1.0"" encoding=""utf-16""?>
<category name=""Universal"">
  <category name=""Synthetic"">
    <property name=""DisplayTimeZone"" value=""Local"" />
    	<symbol name=""TimeSync""/>
  </category>
  <category name=""Stock"">
    <property name=""InstrumentType"" value=""Stock"" />
    <property name=""DisplayTimeZone"" value=""Local"" />
    <property name=""Level2LotSize"" value=""100"" />
    <property name=""Level2LotSizeMinimum"" value=""1"" />
    <property name=""Level2Increment"" value=""0.01"" />
    <property name=""FullPointValue"" value=""1"" />
    <property name=""MinimumTick"" value=""0.01"" />
    <property name=""SessionStart"" value=""08:00:00"" />
    <property name=""SessionEnd"" value=""16:30:00"" />
    <property name=""TimeAndSales"" value=""ActualTrades"" />
    <property name=""QuoteType"" value=""Level1"" />
    <category name=""Testing"">
      <symbol name=""CSCO"">
        <property name=""TimeAndSales"" value=""ActualTrades"" />
        <property name=""QuoteType"" value=""None"" />
      </symbol>
      <symbol name=""MSFT"">
        <property name=""TimeAndSales"" value=""None"" />
        <property name=""QuoteType"" value=""Level2"" />
      </symbol>
      <symbol name=""IBM"">
        <property name=""TimeAndSales"" value=""None"" />
        <property name=""QuoteType"" value=""Level1"" />
      </symbol>
      <symbol name=""GOOG"">
        <property name=""TimeAndSales"" value=""None"" />
        <property name=""QuoteType"" value=""Level1"" />
      </symbol>
      <symbol name=""Design"" />
      <symbol name=""FullTick"" />
      <symbol name=""Daily4Ticks"" />
      <symbol name=""MockFull"" />
      <symbol name=""Mock4Ticks"" />
      <symbol name=""Mock4Sim"" />
      <symbol name=""Daily4Sim"">
        <property name=""DisplayTimeZone"" value=""Local"" />
      </symbol>
      <symbol name=""Daily4Test"" />
      <symbol name=""TXF"" />
      <symbol name=""spyTestBars"" />
    </category>
  </category>
  <category name=""Forex"">
    <property name=""InstrumentType"" value=""Forex"" />
    <property name=""TimeZone"" value=""Eastern Standard Time"" />
    <property name=""DisplayTimeZone"" value=""Local"" />
    <property name=""Level2LotSize"" value=""10000"" />
    <property name=""Level2LotSizeMinimum"" value=""100"" />
    <property name=""Level2Increment"" value=""10"" />
    <property name=""FullPointValue"" value=""1"" />
    <property name=""TimeAndSales"" value=""Extrapolated"" />
    <property name=""QuoteType"" value=""Level1"" />
    <category name=""4 Pip"">
    <property name=""MinimumTick"" value=""0.0001"" />
      <symbol name=""USD/CHF"" universal=""USDCHF"">
        <property name=""QuoteType"" value=""Level2"" />
        <property name=""TimeAndSales"" value=""Extrapolated"" />
      </symbol>
      <symbol name=""USD/CAD"" universal=""USDCAD"" />
      <symbol name=""AUD/USD"" universal=""AUDUSD"" />
      <symbol name=""USD/NOK"" universal=""USDNOK"" />
      <symbol name=""EUR/USD"" universal=""EURUSD"" >
        <property name=""MinimumTick"" value=""0.00001"" />
        <property name=""UseSyntheticLimits"" value=""false"" />
        <property name=""UseSyntheticStops"" value=""false"" />
        <property name=""UseSyntheticMarkets"" value=""false"" />
      </symbol>
      <symbol name=""USD/SEK"" universal=""USDSEK"" />
      <symbol name=""USD/DKK"" universal=""USDDKK"" />
      <symbol name=""GBP/USD"" universal=""GBPUSD"" />
      <symbol name=""EUR/CHF"" universal=""EURCHF"" />
      <symbol name=""EUR/GBP"" universal=""EURGBP"" />
      <symbol name=""EUR/NOK"" universal=""EURNOK"" />
      <symbol name=""EUR/SEK"" universal=""EURSEK"" />
      <symbol name=""GBP/CHF"" universal=""GBPCHF"" />
      <symbol name=""NZD/USD"" universal=""NZDUSD"" />
      <symbol name=""AUD/CHF"" universal=""AUDCHF"" />
      <symbol name=""AUD/CAD"" universal=""AUDCAD"" />
    </category>
    <category name=""2 Pip"">
    <property name=""MinimumTick"" value=""0.001"" />
      <symbol name=""USD/JPY"" >
        <property name=""UseSyntheticLimits"" value=""false"" />
        <property name=""UseSyntheticStops"" value=""false"" />
        <property name=""UseSyntheticMarkets"" value=""false"" />
      </symbol>
      <category name=""Testing"">
        <symbol name=""USD_JPY"">
          <property name=""DisplayTimeZone"" value=""UTC"" />
          <property name=""SessionStart"" value=""01:00:00"" />
          <property name=""SessionEnd"" value=""10:00:00.000"" />
        </symbol>
        <symbol name=""USD_JPY2"" universal=""USD_JPY"">
          <property name=""DisplayTimeZone"" value=""Exchange"" />
          <property name=""SessionStart"" value=""06:00:00"" />
          <property name=""SessionEnd"" value=""15:00:00.000"" />
        </symbol>
        <symbol name=""USD_JPY_YEARS"">
          <property name=""DisplayTimeZone"" value=""UTC"" />
        </symbol>
        <symbol name=""USDJPYBenchMark"" />
        <symbol name=""USD_JPY_Volume"" />
        <symbol name=""USD_JPY_TEST"">
          <property name=""SessionEnd"" value=""09:22:13.000"" />
        </symbol>
        <symbol name=""TST_TST"" />
        <symbol name=""TST_VR2"" />
        <symbol name=""TST_VR3"" />
        <symbol name=""TST_VR4"" />
        <symbol name=""TST_VR5"" />
        <symbol name=""TST_VR6"" />
        <symbol name=""TST_VR7"" />
        <symbol name=""TST_VR8"" />
      </category>
      <symbol name=""CHF/JPY"" universal=""CHFJPY"" />
      <symbol name=""EUR/JPY"" universal=""EURJPY"" />
      <symbol name=""GBP/JPY"" universal=""GBPJPY"" />
      <symbol name=""AUD/JPY"" universal=""AUDJPY"" />
      <symbol name=""CAD/JPY"" universal=""CADJPY"" />
    </category>
  </category>
  <category name=""Futures"">
    <property name=""InstrumentType"" value=""Future"" />
    <property name=""DisplayTimeZone"" value=""Local"" />
    <property name=""Level2LotSize"" value=""1"" />
    <property name=""Level2LotSizeMinimum"" value=""1"" />
    <property name=""Level2Increment"" value=""1"" />
    <property name=""FullPointValue"" value=""50"" />
    <property name=""MinimumTick"" value=""0.25"" />
    <property name=""TimeAndSales"" value=""ActualTrades"" />
    <property name=""QuoteType"" value=""Level1"" />
    <category name=""Testing"">
      <symbol name=""ES"">
        <property name=""UseSyntheticLimits"" value=""false"" />
        <property name=""UseSyntheticStops"" value=""false"" />
        <property name=""UseSyntheticMarkets"" value=""false"" />
      </symbol>
      <symbol name=""/ESZ9"" />
      <symbol name=""/ESU0"" />
      <symbol name=""/NQU0"" />
      <symbol name=""TestException"" />
      <category name=""TradeOnly"">
        <property name=""TimeAndSales"" value=""ActualTrades"" />
        <property name=""QuoteType"" value=""None"" />
        <symbol name=""/ESH0""/>
        <symbol name=""/ESH0TradeBar""/>
      </category>
    </category>
  </category>
</category>";
#endregion

#region USER_DICTIONARY
		public static string UserDictionary = @"<?xml version=""1.0"" encoding=""utf-16""?>
<category name=""MB Trading"">
  <category name=""Stock"">
    <property name=""DisplayTimeZone"" value=""Exchange"" />
    <property name=""FullPointValue"" value=""1"" />
    <property name=""MinimumTick"" value=""0.01"" />
    <category name=""Testing"">
      <property name=""TimeZone"" value=""UTC-4"" />
      <symbol name=""FullTick"" />
      <symbol name=""Daily4Sim"" />
      <symbol name=""Mock4Sim"" >
        <property name=""UseSyntheticLimits"" value=""true"" />
        <property name=""UseSyntheticStops"" value=""true"" />
        <property name=""UseSyntheticMarkets"" value=""true"" />
      </symbol>
      <symbol name=""TXF"" />
      <symbol name=""spyTestBars"" />
    </category>
  </category>
</category>";
#endregion
	}
}
