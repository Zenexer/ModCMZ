using DNA.CastleMinerZ;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using Microsoft.Xna.Framework.Graphics;
using ModCMZ.Core;
using ModCMZ.Core.Game;
using ModCMZ.Core.Mods;
using ModCMZ.Core.Runtime.DNA.CastleMinerZ.GraphicsProfileSupport;
using ModCMZ.Core.Wrappers.DNA.CastleMinerZ.Inventory;
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
	[Mod(nameof(ModCMZ.Mods.Basic), "BasicMod", Author = "Paul Buonopane", Description = "Basic mods", VersionString = "1.0.0.0")]
	public class BasicMod : Mod
	{
		public static BasicMod Instance { get; private set; }

		public BorderlessWindowedComponent BorderlessWindowed { get; private set; }

		public BasicMod()
		{
			Instance = this;
			GameReady += BasicMod_GameReady;
			ComponentsReady += BasicMod_ComponentsReady;
            RegisteringItems += BasicMod_RegisteringItems;
            ClaimingContent += BasicMod_ClaimingContent;
		}

        private void BasicMod_ClaimingContent(Action<string> claim)
		{
			claim(@"UI\Screens\LoadScreen");
		}

        private void BasicMod_RegisteringItems(ModContentManager content)
        {
			// TODO
			//InventoryItemEx.RegisterItemClass(...)
        }

        private void BasicMod_GameReady(GameApp game)
		{
			game.Components.Add(BorderlessWindowed = new BorderlessWindowedComponent(game));
		}

		private void BasicMod_ComponentsReady()
		{
			App.Current.CoreMod.Keyboard.KeyPress += CoreMod_KeyPress;
		}

		private void CoreMod_KeyPress(object sender, KeyPressEventArgs e)
		{
			// TODO
		}
	}
}
