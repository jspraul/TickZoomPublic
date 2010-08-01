// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 1624 $</version>
// </file>

using System;
using System.Runtime.Serialization;

namespace TickZoom.Loader
{
	/// <summary>
	/// Exception used when loading an Plugin fails.
	/// </summary>
	[Serializable]
	public class PluginLoadException : CoreException
	{
		public PluginLoadException() : base()
		{
		}
		
		public PluginLoadException(string message) : base(message)
		{
		}
		
		public PluginLoadException(string message, Exception innerException) : base(message, innerException)
		{
		}
		
		protected PluginLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
