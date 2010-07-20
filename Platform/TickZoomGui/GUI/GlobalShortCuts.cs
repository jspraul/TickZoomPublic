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
using Utilities;
using System.Windows.Forms;

namespace TickZoom
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public class GlobalShortCuts {
		globalKeyboardHook gkh = new globalKeyboardHook();
		bool isLControlDown = false;
		bool isLAltDown = false;
		public event KeyEventHandler CtrlAltUp;
		public event KeyEventHandler CtrlAltDown;
		public event KeyEventHandler CtrlAltRight;
		public event KeyEventHandler CtrlAltNumPad0;
		
		public GlobalShortCuts() {
			gkh.HookedKeys.Add(Keys.LControlKey);
			gkh.HookedKeys.Add(Keys.LMenu);
			gkh.HookedKeys.Add(Keys.Up);
			gkh.HookedKeys.Add(Keys.Down);
			gkh.HookedKeys.Add(Keys.Right);
			gkh.HookedKeys.Add(Keys.NumPad0);
			gkh.KeyDown += new KeyEventHandler(gkh_KeyDown);
			gkh.KeyUp += new KeyEventHandler(gkh_KeyUp);
		}

		void gkh_KeyUp(object sender, KeyEventArgs e) {
			switch( e.KeyCode) {
				case Keys.LControlKey:
					if( isLControlDown) {
						isLControlDown = false;
					}
					break;
				case Keys.LMenu:
					if( isLAltDown) {
						isLAltDown = false;
					}
					break;
			}
		}

		void gkh_KeyDown(object sender, KeyEventArgs e) {
			switch( e.KeyCode) {
				case Keys.LControlKey:
					if( !isLControlDown) {
						isLControlDown = true;
					}
					break;
				case Keys.LMenu:
					if( !isLAltDown) {
						isLAltDown = true;
					}
					break;
			}
			if( isLControlDown && isLAltDown) {
				switch( e.KeyCode) {
					case Keys.Up:
			 			CtrlAltUp(sender, e);
			 			e.Handled = true;
						break;
					case Keys.Down:
			 			CtrlAltDown(sender, e);
			 			e.Handled = true;
						break;
					case Keys.Right:
			 			CtrlAltRight(sender, e);
			 			e.Handled = true;
						break;
					case Keys.NumPad0:
			 			CtrlAltNumPad0(sender, e);
			 			e.Handled = true;
						break;
				}
			}
		}
	}
}
