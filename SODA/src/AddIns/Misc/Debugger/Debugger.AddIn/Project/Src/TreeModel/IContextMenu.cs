// <file>
//     <copyright license="BSD-new" see="prj:///COPYING"/>
//     <owner name="David Srbecký" email="dsrbecky@gmail.com"/>
//     <version>$Revision: 3648 $</version>
// </file>

using System.Windows.Forms;

namespace Debugger.AddIn.TreeModel
{
	public interface IContextMenu
	{
		ContextMenuStrip GetContextMenu();
	}
}
