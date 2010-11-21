namespace TickZoom.Presentation.Framework
{
    using System;
    using System.Collections.Generic;

    public class ResultEnumerator
    {
        #region Fields

        private readonly IEnumerator<IResult> _enumerator;

        #endregion Fields

        #region Constructors

        public ResultEnumerator(IEnumerable<IResult> children)
        {
            _enumerator = children.GetEnumerator();
        }

        #endregion Constructors

        #region Methods

        public void Enumerate()
        {
            ChildCompleted(null, EventArgs.Empty);
        }

        private void ChildCompleted(object sender, EventArgs args)
        {
            var previous = sender as IResult;

            if(previous != null)
                previous.Completed -= ChildCompleted;

            if(!_enumerator.MoveNext())
                return;

            var next = _enumerator.Current;
            next.Completed += ChildCompleted;
            next.Execute();
        }

        #endregion Methods
    }
}