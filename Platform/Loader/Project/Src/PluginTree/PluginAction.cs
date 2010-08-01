// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 2318 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace TickZoom.Loader
{
	/// <summary>
	///   Specifies the action to be taken for a specific <see cref="Plugin"/>.
	/// </summary>
	public enum PluginAction
	{
		/// <summary>
		///   Enable the <see cref="Plugin"/>.
		/// </summary>
		Enable,
		/// <summary>
		///     Disable the <see cref="Plugin"/>.
		/// </summary>
		Disable,
		/// <summary>
		///     Install the <see cref="Plugin"/>.
		/// </summary>
		Install,
		/// <summary>
		///     Uninstall the <see cref="Plugin"/>.
		/// </summary>
		Uninstall,
		/// <summary>
		///     Update the <see cref="Plugin"/>.
		/// </summary>
		Update,
		/// <summary>
		/// The <see cref="Plugin"/> is disabled because it has been installed
		/// twice (duplicate identity).
		/// </summary>
		InstalledTwice,
		/// <summary>
		///     Tells that the <see cref="Plugin"/> cannot be loaded because not all dependencies are satisfied.
		/// </summary>
		DependencyError,
		/// <summary>
		/// A custom error has occurred (e.g. the Plugin disabled itself using a condition).
		/// </summary>
		CustomError
	}
}
