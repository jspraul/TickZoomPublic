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
using NUnit.Framework;
using TickZoom.Api;

namespace TickZoom.TickData
{
	[TestFixture]
	public class TickQueueTest
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(TickQueueTest));
		TickQueue queue;
		
		[SetUp]
		public void Setup() {
			queue = Factory.TickUtil.TickQueue(typeof(TickQueueTest));
		}
		
		[Test]
		public void EnQueueItemTest()
		{
			TickBinary tick = new TickBinary();
			TickIO tickIO = Factory.TickUtil.TickIO();
			long start = Factory.TickCount;
			for(int i=0; i<10; i++) {
				Assert.IsTrue(queue.TryEnQueue(ref tick));
			}
			long stop = Factory.TickCount;
			log.Notice("Enqueue elapsed time is "+(stop-start)+"ms");
			
			start = Factory.TickCount;
			for(int i=0; i<10; i++) {
				Assert.IsTrue(queue.TryDequeue(ref tick));
			}
			stop = Factory.TickCount;
			log.Notice("Dequeue elapsed time is "+(stop-start)+"ms");
			queue.LogStats();
		}
	}
}
