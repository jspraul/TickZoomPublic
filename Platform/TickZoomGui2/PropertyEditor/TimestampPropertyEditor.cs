using System;
using System.ComponentModel;
using System.Windows.Forms;

using TickZoom.Api;

public class TimestampPropertyEditor : PropertyEditorBase
{
    private MonthCalendar myMonthCalendar;
    protected override Control GetEditControl(string PropertyName, object value)
    {
        if (myMonthCalendar == null)
        {
            myMonthCalendar = new MonthCalendar();
            {
            	TimeStamp timeStamp = (TimeStamp) value;
            	myMonthCalendar.MaxDate = DateTime.Now;
            	myMonthCalendar.MinDate = new DateTime(1900,1,1);
            	myMonthCalendar.MaxSelectionCount = 1;
            	myMonthCalendar.SelectionStart = new DateTime(timeStamp.Year,timeStamp.Month,timeStamp.Day);
            	myMonthCalendar.SelectionEnd = new DateTime(timeStamp.Year,timeStamp.Month,timeStamp.Day);
            	myMonthCalendar.DateSelected += DateSelected;
            }
        }

        return myMonthCalendar;
    }

    protected override object GetEditedValue(Control EditControl, string PropertyName, object OldValue)
    {
    	return new TimeStamp( myMonthCalendar.SelectionStart);
    }

    private void DateSelected(object sender, EventArgs e)
    {
        CloseDropDownWindow();
    }
}