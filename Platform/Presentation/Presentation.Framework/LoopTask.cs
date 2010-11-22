namespace TickZoom.Presentation.Framework
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
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
