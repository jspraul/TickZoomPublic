namespace TickZoom.GUI.Framework
{
    public class ScreenConductor : ViewModelBase
    {
        #region Fields

        private IScreen _activeScreen;

        #endregion Fields

        #region Properties

        public IScreen ActiveScreen
        {
            get { return _activeScreen; }
            set { OpenScreen(value); }
        }

        #endregion Properties

        #region Methods

        public void OpenScreen(IScreen screen)
        {
            if(screen == null)
                return;

            if(screen.Equals(_activeScreen))
                return;

            if(_activeScreen != null && !_activeScreen.CanClose())
                return;

            screen.Activate();
            _activeScreen = screen;

            NotifyOfPropertyChange(() => ActiveScreen);
        }

        #endregion Methods
    }
}