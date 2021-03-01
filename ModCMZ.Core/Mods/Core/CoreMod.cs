using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ModCMZ.Core.Game;
using ModCMZ.Core.Mods.Core.Components;

namespace ModCMZ.Core.Mods.Core
{
	[Mod(nameof(ModCMZ.Core), "Core", Author = "Paul Buonopane", Description = "Provides the core functionality for ModCMZ.  Cannot be removed.")]
	public class CoreMod : Mod
	{
		public static CoreMod Current { get; private set; }

		public KeyboardComponent Keyboard { get; private set; }

		public ConsoleComponent Console { get; private set; }

		public CoreMod()
		{
			DomainReady += CoreMod_DomainReady;
			GameReady += CoreMod_GameReady;
		}

		private void CoreMod_DomainReady()
		{
			Current = this;
		}

		private void CoreMod_GameReady(GameApp game)
		{
			game.Components.Add(Keyboard = new KeyboardComponent(game, this));
			game.Components.Add(Console = new ConsoleComponent(game, this));
		}
	}
}
