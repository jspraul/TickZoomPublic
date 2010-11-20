namespace TickZoom.GUI.Framework
{
    using System;

    public class BusyResult : IResult
    {
        private readonly bool _isBusy;

        public BusyResult(bool isBusy)
        {
            _isBusy = isBusy;
        }

        public void Execute()
        {
//            var shell = IoC.GetInstance<IShell>();
//            shell.IsBusy = _isBusy;
//            Completed(this, EventArgs.Empty);
        }

        public event EventHandler Completed = delegate { };
    }
}