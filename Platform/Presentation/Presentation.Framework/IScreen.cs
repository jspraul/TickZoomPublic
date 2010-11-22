namespace TickZoom.Presentation.Framework
{
    public interface IScreen
    {
        #region Methods

        void Activate();

        bool CanClose();

        #endregion Methods
    }
}