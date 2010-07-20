using System;
using System.ComponentModel;
using System.Windows.Forms;


    
public class TimeSpanPropertyEditor : PropertyEditorBase
{
    private DateTimePicker myDateTimePicker;
    protected override Control GetEditControl(string PropertyName, object value)
    {
        if (myDateTimePicker == null)
        {
            myDateTimePicker = new DateTimePicker();
            {
                myDateTimePicker.Value = DateTime.Today.Add((TimeSpan)value); 
                myDateTimePicker.Format = DateTimePickerFormat.Time; 
                myDateTimePicker.ShowUpDown = true; 
            }
        }

        return myDateTimePicker;
    }

    protected override object GetEditedValue(Control EditControl, string PropertyName, object OldValue)
    {
        return myDateTimePicker.Value.TimeOfDay;
    }

    private void myTreeView_DoubleClick(object sender, EventArgs e)
    {
        CloseDropDownWindow();
    }
}