namespace TickZoom.GUI.Framework
{
    public interface IShell
    {
        #region Properties

        bool IsBusy
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        bool CanClose();

        void OpenScreen(IScreen screen);

        #endregion Methods
    }
}