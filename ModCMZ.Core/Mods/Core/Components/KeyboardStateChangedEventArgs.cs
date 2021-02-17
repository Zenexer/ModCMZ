using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace ModCMZ.Core.Mods.Core.Components
{
	public delegate void KeyboardStateChangedEventHandler(object sender, KeyboardStateChangedEventArgs e);

	public class KeyboardStateChangedEventArgs : EventArgs
	{
		public KeyboardState OldState { get; private set; }
		public KeyboardState NewState { get; private set; }

		public KeyboardStateChangedEventArgs(KeyboardState oldState, KeyboardState newState)
		{
			OldState = oldState;
			NewState = newState;
		}

		public bool IsNewKeyPressed(Keys key)
		{
			return OldState.IsKeyUp(key) && NewState.IsKeyDown(key);
		}

		public bool IsNewKeyReleased(Keys key)
		{
			return OldState.IsKeyDown(key) && NewState.IsKeyUp(key);
		}
	}
}
