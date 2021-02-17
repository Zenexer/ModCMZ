using System;
using DNA;
using ModCMZ.Core.Game;
using xna = Microsoft.Xna.Framework;

namespace ModCMZ.Core.Runtime.DNA
{
	public static class DNAGameMod
	{
		public static void Initialize(DNAGame instance)
		{
			GameApp.InitializeHook(instance);
		}

		public static void Update(DNAGame instance, xna::GameTime time)
		{
			GameApp.UpdateHook(instance, time);
		}

		public static void Draw(DNAGame instance, xna::GameTime time)
		{
			GameApp.DrawHook(instance, time);
		}
	}
}
