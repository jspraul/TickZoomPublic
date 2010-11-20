namespace TickZoom.GUI.Framework
{
    using System;

    public interface IResult
    {
        void Execute();
        event EventHandler Completed;
    }
}