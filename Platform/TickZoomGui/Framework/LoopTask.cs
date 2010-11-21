namespace TickZoom.GUI.Framework
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Windows;
	public class Loop
	{	
		Func<bool> function;
		public Loop( Func<bool> action) {
			this.function = action;
		}
	    #region Methods
	
	    public bool Execute() {
	    	return function();
	    }
	    
	    #endregion Methods
	}
}
