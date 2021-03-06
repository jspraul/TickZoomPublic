﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision: 2111 $</version>
// </file>

using System;
using ICSharpCode.Core;

namespace ICSharpCode.XmlEditor
{
	/// <summary>
	/// Inserts a new text node to after the selected node.
	/// </summary>
	public class InsertTextNodeAfterCommand : AbstractMenuCommand
	{		
		public override void Run()
		{
			XmlTreeViewContainerControl view = Owner as XmlTreeViewContainerControl;
			if (view != null) {
				view.InsertTextNodeAfter();
			}
		}
	}
}

