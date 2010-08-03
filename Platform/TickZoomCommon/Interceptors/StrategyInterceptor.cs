#region Copyright
/*
 * Copyright 2008 M. Wayne Walter
 * Software: TickZoom Trading Platform
 * User: Wayne Walter
 * 
 * You can use and modify this software under the terms of the
 * TickZOOM General Public License Version 1.0 or (at your option)
 * any later version.
 * 
 * Businesses are restricted to 30 days of use.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * TickZOOM General Public License for more details.
 *
 * You should have received a copy of the TickZOOM General Public
 * License along with this program.  If not, see
 * 
 * 
 *
 * User: Wayne Walter
 * Date: 8/3/2010
 * Time: 12:00 PM
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using TickZoom.Api;

namespace TickZoom.Interceptors
{
	/// <summary>
	/// Description of StrategyInterceptor.
	/// </summary>
	public class StrategyInterceptor : StrategyInterceptorInterface
	{
		public event Action<EventInterceptor> OnActiveChange;
		private int order = 0;
		private bool isActive = true;
		
		public StrategyInterceptor()
		{
		}
		
		public int Order {
			get { return order; }
			set { order = value; }
		}
		
		public virtual bool IsActive {
			get { return isActive; }
			set { if( isActive != value) {
					isActive = value;
					if( OnActiveChange != null) {
						OnActiveChange(this);
					}
				}
			}
		}
		
		public virtual void Intercept(EventContext context, EventType eventType, object eventDetail)
		{
			throw new NotImplementedException("The event " + (EventType) eventType + " was never implemented.");
		}
	}
}
