using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

namespace TickZoom
{
	[Serializable]
	public class PropertyDetailCollection : List<PropertyDetail> {
		public int IndexOf(string name)
		{
			int i = 0;
			foreach(PropertyDetail spec in this)
			{
				if(spec.Name == name)
					return i;
	
				i++;
			}
	
			return -1;
		}
	}
}
