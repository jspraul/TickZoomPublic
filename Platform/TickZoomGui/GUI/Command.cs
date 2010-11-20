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
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Media;
    using System.Threading;
    using System.Windows.Forms;

    using TickZoom.Api;

    public abstract class Command : INotifyPropertyChanged
    {
        #region Fields

        private bool enabled;
        private string key, displayName;

        #endregion Fields

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public string DisplayName
        {
            get { return displayName; }
            protected set {
                if (displayName != value)
                {
                    displayName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("DisplayName"));
                }
            }
        }

        public bool Enabled
        {
            get { return enabled; }
            set {
                if (enabled != value) {
                    enabled = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Enabled"));
                }
            }
        }

        public string Key
        {
            get { return key; }
            protected set { key = value; }
        }

        #endregion Properties

        #region Methods

        public abstract void Execute();

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            handler(this, e);
        }

        #endregion Methods
    }
}