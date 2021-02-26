using DNA.CastleMinerZ;
using DNA.CastleMinerZ.AI;
using Microsoft.Xna.Framework.Graphics;
using ModCMZ.Core;
using ModCMZ.Core.Mods;
using ModCMZ.Mods.Basic.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModCMZ.Mods.Basic
{
	[Mod("BasicMod", Author = "Paul Buonopane", Description = "Basic mods", VersionString = "1.0.0.0")]
	public class BasicMod : Mod
	{
		public static BasicMod Instance { get; private set; }

		public BorderlessWindowedComponent BorderlessWindowed { get; private set; }

		public BasicMod()
		{
			Instance = this;
			GameReady += BasicMod_GameReady;
			ComponentsReady += BasicMod_ComponentsReady;
		}

		private void BasicMod_GameReady(object sender, GameReadyEventArgs e)
		{
			var game = e.Game;
			game.Components.Add(BorderlessWindowed = new BorderlessWindowedComponent(game));
		}

		private void BasicMod_ComponentsReady(object sender, EventArgs e)
		{
			App.Current.CoreMod.Keyboard.KeyPress += CoreMod_KeyPress;
		}

		private void CoreMod_KeyPress(object sender, KeyPressEventArgs e)
		{
			// TODO
		}
	}
}
