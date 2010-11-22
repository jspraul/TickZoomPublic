namespace TickZoom.Presentation.Framework
{
    using System;

    public interface IResult
    {
        #region Events

        event EventHandler Completed;

        #endregion Events

        #region Methods

        void Execute();

        #endregion Methods
    }
}