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

namespace TickZoom.GUI
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// Description of NumericTextBox.
    /// </summary>
    public partial class NumericTextBox : TextBox
    {
        #region Fields

        bool allowDecimal = true;
        bool allowSeparator = false;
        int decimalCount = 0;
        string decimalSeparator;
        string groupSeparator;
        int[] groupSize;
        string negativeSign;
        NumberFormatInfo numberFormatInfo;
        int separatorCount = 0;

        #endregion Fields

        #region Constructors

        public NumericTextBox()
        {
            InitializeComponent();
            numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
            decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
            groupSeparator = numberFormatInfo.NumberGroupSeparator;
            groupSize = numberFormatInfo.NumberGroupSizes;
            negativeSign = numberFormatInfo.NegativeSign;
        }

        #endregion Constructors

        #region Properties

        public bool AllowDecimal
        {
            get
            {
                return allowDecimal;
            }
            set
            {
                allowDecimal = value;
            }
        }

        public bool AllowSeparator
        {
            get
            {
                return allowSeparator;
            }
            set
            {
                allowSeparator = value;
            }
        }

        public decimal DecimalValue
        {
            get
            {
                try
                {
                    return Decimal.Parse(this.Text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
                }
                catch (FormatException)
                {
                    return 0;
                }
            }
        }

        public int IntValue
        {
            get
            {
                try
                {
                    return Int32.Parse(this.Text, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
                }
                catch (FormatException)
                {
                    return 0;
                }
                catch (OverflowException)
                {
                    return 0;
                }
            }
        }

        #endregion Properties

        #region Methods

        // Checks current input characters
        // and updates control with valid characters only.
        public void UpdateText()
        {
            decimalCount = 0;
            separatorCount = 0;
            string input = this.Text;
            string updatedText = "";
            int cSize = 0;

            // char[] tokens = new char[] { decimalSeparator.ToCharArray()[0] };
            // NOTE: Supports decimalSeparator with a length == 1.
            char token = decimalSeparator.ToCharArray()[0];
            string[] groups = input.Split(token);

            // Returning input to left of decimal.
            char[] inputChars = groups[0].ToCharArray();
            // Reversing input to handle separators.
            Array.Reverse(inputChars);
            StringBuilder sb = new StringBuilder();

            bool validKey = false;

            for(int x = 0; x < inputChars.Length; x++)
            {

                if (inputChars[x].ToString().Equals(groupSeparator))
                {
                    continue;
                }

                // Checking for decimalSeparator is not required in
                // current implementation. Current implementation eliminates
                // all digits to right of extraneous decimal characters.
                if (inputChars[x].ToString().Equals(decimalSeparator))
                {
                    if (!allowDecimal | decimalCount > 0)
                    {
                        continue;
                    }
                    decimalCount++;
                    validKey = true;
                }

                if (inputChars[x].ToString().Equals(negativeSign))
                {
                    // Ignore negativeSign unless processing first character.
                    if (x < (inputChars.Length - 1))
                    {
                        continue;
                    }
                    sb.Insert(0, inputChars[x].ToString());
                    x++;
                    continue;
                }

                if (allowSeparator)
                {
                    // NOTE: Does not support multiple groupSeparator sizes.
                    if (cSize > 0 && cSize % groupSize[0] == 0)
                    {
                        sb.Insert(0, groupSeparator);
                        separatorCount++;
                    }
                }

                // Maintaining correct group size for digits.
                if (Char.IsDigit(inputChars[x]))
                {
                    // Increment cSize only after processing groupSeparators.
                    cSize++;
                    validKey = true;
                }

                if (validKey)
                {
                    sb.Insert(0, inputChars[x].ToString());
                }

                validKey = false;
            }

            updatedText = sb.ToString();

            if (allowDecimal && groups.Length > 1)
            {
                char[] rightOfDecimals = groups[1].ToCharArray();
                StringBuilder sb2 = new StringBuilder();

                foreach (char dec in rightOfDecimals)
                {
                    if (Char.IsDigit(dec))
                    {
                        sb2.Append(dec);
                    }
                }
                updatedText += decimalSeparator + sb2.ToString();
            }
            // Updates text box.
            if( groups.Length>1) {
                updatedText += decimalSeparator + groups[1];
            }
            this.Text = updatedText;
            // Updates cursor position.
            this.SelectionStart = this.Text.Length;
        }

        // Restricts the entry of characters to digits (including hex), the negative sign,
        // the decimal point, and editing keystrokes (backspace).
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            string keyInput = e.KeyChar.ToString();

            if (Char.IsDigit(e.KeyChar))
            {
                UpdateText();
            }

            // Allows one decimal separator as input.
            else if (keyInput.Equals(decimalSeparator))
            {
                if (allowDecimal)
                {
                    UpdateText();
                }
                e.Handled = true;
            }

            //	        // Allows separator
            //	        else if (keyInput.Equals(groupSeparator))
            //	        {
            //	            if (allowSeparator)
            //	            {
            //	                UpdateText();
            //	            }
            //	            e.Handled = true;
            //	        }
            //
            //	        // Allows negative sign if the negative sign is the initial character.
            //	        else if (keyInput.Equals(negativeSign))
            //	        {
            //	            UpdateText();
            //	            e.Handled = true;
            //	        }

            else if (e.KeyChar == '\b')
            {
                // Allows Backspace key.
            }
            //
            //	        else if (e.KeyChar == '\r')
            //	        {
            //	            UpdateText();
            //	            // Validate input when Enter key pressed.
            //	            // Take other action.
            //	        }
            //
            else {
                // Consume this invalid key and beep
                e.Handled = true;
                // MessageBeep();
            }
        }

        #endregion Methods
    }
}