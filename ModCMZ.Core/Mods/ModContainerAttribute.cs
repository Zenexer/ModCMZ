using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Mods
{
	/// <summary>
	/// Marks a module as containing a CastleMiner Z mod.  Can also be applied to an entire assembly, but it's more efficient to stick to a single module.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module, Inherited = false, AllowMultiple = false)]
	public sealed class ModContainerAttribute : Attribute
	{
		/// <summary>
		/// <c>true</c> if the mod uses bytecode manipulation; otherwise, <c>false</c>.  Defaults to <c>false</c>.
		/// </summary>
		public bool UsesInjection = false;
	}
}
