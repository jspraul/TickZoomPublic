namespace TickZoom.GUI.Framework
{
    public interface IShell
    {
        bool IsBusy { get; set; }
        void OpenScreen(IScreen screen);
        bool CanClose();
    }
}