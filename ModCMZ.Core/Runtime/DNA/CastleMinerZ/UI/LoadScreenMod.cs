using DNA;
using DNA.CastleMinerZ.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Runtime.DNA.CastleMinerZ.UI
{
    public static class LoadScreenMod
    {
        private static readonly Keys[] _loadScreenCloseKeys = new[]
        {
            Keys.Escape,
            Keys.Enter,
            Keys.Space,
        };

        public static bool OnUpdate(LoadScreen instance, DNAGame game, GameTime gameTime)
        {
            if (instance.Finished)
            {
                return false;
            }

            var keyboardState = Keyboard.GetState();

            foreach (var key in _loadScreenCloseKeys)
            {
                if (keyboardState.IsKeyDown(key))
                {
                    instance.Finished = true;
                    return false;
                }
            }

            var mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                instance.Finished = true;
                return false;
            }

            return true;
        }
    }
}
