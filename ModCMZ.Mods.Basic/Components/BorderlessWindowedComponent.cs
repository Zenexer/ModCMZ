using DNA.CastleMinerZ;
using Microsoft.Xna.Framework;
using ModCMZ.Core.Game;
using ModCMZ.Mods.Basic.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModCMZ.Mods.Basic.Components
{
    public class BorderlessWindowedComponent : GameComponent
    {
        public new GameApp Game { get; }

        private bool _isBorderlessWindowed;
        public bool IsBorderlessWindowed
        {
            get => _isBorderlessWindowed;
            set
            {
                if (value == _isBorderlessWindowed)
                {
                    return;
                }

                Settings.Default.BorderlessWindowed = value;
                Settings.Default.Save();

                ForceSetBorderlessWindowed(value);
            }
        }

        public BorderlessWindowedComponent(GameApp game)
            : base(game.Game)
        {
            Game = game;
        }

        public override void Initialize()
        {
            ForceSetBorderlessWindowed(Settings.Default.BorderlessWindowed);

            base.Initialize();
        }

        private void ForceSetBorderlessWindowed(bool value)
        {
            _isBorderlessWindowed = value;

            var form = Game.Form;

            if (value)
            {
                Game.Game.IsFullScreen = false;
                CastleMinerZGame.GlobalSettings.FullScreen = false;

                form.FormBorderStyle = FormBorderStyle.None;
                form.WindowState = FormWindowState.Maximized;
            }
            else
            {
                form.FormBorderStyle = FormBorderStyle.Sizable;
                form.MaximizeBox = true;
            }
        }
    }
}
