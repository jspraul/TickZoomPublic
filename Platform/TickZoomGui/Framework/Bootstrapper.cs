using System.Windows.Forms;
using TickZoom;

namespace TickZoom.GUI.Framework
{
    using System.Windows;
    
    public static class Bootstrapper
    {
        public static Form CreateShell()
        {
            
			var shell = new ViewModel();
			var view = new Form1(shell);

            ViewModelBinder.Bind(shell, view);

            return view;
        }
    }
}