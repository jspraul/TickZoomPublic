namespace TickZoom.GUI.Framework
{
    public class ScreenConductor : ViewModelBase
    {
        private IScreen _activeScreen;

        public IScreen ActiveScreen
        {
            get { return _activeScreen; }
            set { OpenScreen(value); }
        }

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
    }
}