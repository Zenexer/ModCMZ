using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNA.Input;
using ModCMZ.Core.Mods.Core;

namespace ModCMZ.Core.Runtime.DNA.Input
{
	public static class InputManagerMod
	{
		public static bool Update(InputManager instance)
		{
			return !CoreMod.Current.Keyboard.IsIntercepting;
		}
	}
}
