using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModCMZ.Core.Game;

namespace ModCMZ.Core.Mods
{
	public delegate void GameReadyEventHandler(object sender, GameReadyEventArgs e);

	public class GameReadyEventArgs : EventArgs
	{
		public GameApp Game { get; private set; }

		public GameReadyEventArgs(GameApp game)
		{
			Game = game;
		}
	}
}
