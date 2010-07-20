using System;
using System.ComponentModel;
using System.Windows.Forms;

using TickZoom.Api;

public class ElapsedPropertyEditor : PropertyEditorBase
{
    private DateTimePicker myDateTimePicker;
    protected override Control GetEditControl(string PropertyName, object value)
    {
        if (myDateTimePicker == null)
        {
            myDateTimePicker = new DateTimePicker();
            {
            	myDateTimePicker.Value = DateTime.Today.AddDays(((Elapsed)value).Internal);
                myDateTimePicker.Format = DateTimePickerFormat.Time; 
                myDateTimePicker.ShowUpDown = true; 
            }
        }

        return myDateTimePicker;
    }

    protected override object GetEditedValue(Control EditControl, string PropertyName, object OldValue)
    {
    	TimeSpan span = myDateTimePicker.Value.TimeOfDay;
    	return new Elapsed(span.Hours,span.Minutes,span.Seconds);
    }

    private void myTreeView_DoubleClick(object sender, EventArgs e)
    {
        CloseDropDownWindow();
    }
}