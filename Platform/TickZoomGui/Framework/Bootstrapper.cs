namespace TickZoom.GUI.Framework
{
    using System.Windows;
    using System.Windows.Forms;

    using TickZoom;

    public static class Bootstrapper
    {
        #region Methods

        public static Form CreateShell()
        {
            var shell = new ViewModel();
            var view = new Form1(shell);

            ViewModelBinder.Bind(shell, view);

            return view;
        }

        #endregion Methods
    }
}