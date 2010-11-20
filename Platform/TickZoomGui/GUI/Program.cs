#region Header

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

#endregion Header

namespace TickZoom
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;

    using TickZoom.Api;
    using TickZoom.GUI.Framework;

    static class Program
    {
        #region Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try {
                DebugWriter d = new DebugWriter();
                System.Console.SetOut(d);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var vm = new ViewModel();
                var form = new Form1(vm);
                ViewModelBinder.Bind( vm, form);
                form.Visible = true;
                while( true) {
                    Application.DoEvents();
                    Execute.MessageLoop();
                    form.ProcessMessages();
                }
            } catch( Exception ex) {
                Console.WriteLine(ex.GetType() + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
                System.Diagnostics.Debug.WriteLine(ex.GetType() + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        #endregion Methods

        #region Nested Types

        public class DebugWriter : TextWriter
        {
            #region Properties

            public override System.Text.Encoding Encoding
            {
                get {
                    throw new NotImplementedException();
                }
            }

            #endregion Properties

            #region Methods

            public override void Write(char value)
            {
                //				System.Diagnostics.Debug.Write(value);
            }

            public override void WriteLine(string value)
            {
                //				System.Diagnostics.Debug.WriteLine(value);
            }

            #endregion Methods
        }

        #endregion Nested Types
    }
}