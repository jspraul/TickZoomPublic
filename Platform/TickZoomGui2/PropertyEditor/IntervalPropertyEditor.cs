using System;
using System.Windows.Forms;
//using PropertyEditorSharp;
using TickZoom.Api;
using TickZoom.PropertyEditor;

public class IntervalPropertyEditor : PropertyEditorBase
{
    //this is the control to be used in design time DropDown editor
    private IntervalEditorControl control;

    protected override Control GetEditControl(string PropertyName, object CurrentValue)
    {
        control = new IntervalEditorControl();
        control.Visible = true;
        control.Interval = (Interval) CurrentValue;
        return control;
    }

    protected override object GetEditedValue(Control EditControl, string PropertyName, object OldValue)
    {
        if (control == null) return OldValue;
        if (control.IsCanceled)
            return OldValue;
        else
            return control.Interval;
    }

}