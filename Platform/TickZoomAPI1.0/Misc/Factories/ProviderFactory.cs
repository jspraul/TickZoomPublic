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
using System.Reflection;

namespace TickZoom.Api
{
	public interface EventLog {
		bool CheckEnabled( Log log);
		int GetReceiverId( object component);
		void Capture( int componentId, SymbolInfo symbol, int eventType, object eventDetail);
	}
	
	[CLSCompliant(false)]
	public interface ProviderFactory {
		void Release();
		Provider AsyncProvider( Provider provider);
		AsyncReceiver AsyncReceiver( Receiver receiver);
		Provider InProcessProvider(string providerAssembly);
		Provider RemoteProvider(string address, ushort port);
		Provider ProviderProcess(string address, ushort port, string executableFileName);
		ServiceConnection ProviderService();
		ServiceConnection ConnectionManager();
		Serializers Serializers();
		Socket Socket(string name);
		Selector Selector(string address, ushort port, long timeout, Action<Exception> onException);
		Selector Selector(Action<Exception> onException);
		EventLog EventLog {
			get;
		}
	}
}
	