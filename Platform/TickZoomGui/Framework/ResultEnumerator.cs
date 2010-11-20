namespace TickZoom.GUI.Framework
{
    using System;
    using System.Collections.Generic;

    public class ResultEnumerator
    {
        private readonly IEnumerator<IResult> _enumerator;

        public ResultEnumerator(IEnumerable<IResult> children)
        {
            _enumerator = children.GetEnumerator();
        }

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
    }
}