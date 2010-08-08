// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1965 $</version>
// </file>

using System;

namespace TickZoom.Loader
{
	public class IconDescriptor 
	{
		Extension extension;
		
		public string Id {
			get {
				return extension.Id;
			}
		}
		
		public string Language {
			get {
				return extension.Properties["language"];
			}
		}
		
		public string Resource {
			get {
				return extension.Properties["resource"];
			}
		}
		
		public string[] Extensions {
			get {
				return extension.Properties["extensions"].Split(';');
			}
		}
		
		public IconDescriptor(Extension extension)
		{
			this.extension = extension;
		}
	}
}
