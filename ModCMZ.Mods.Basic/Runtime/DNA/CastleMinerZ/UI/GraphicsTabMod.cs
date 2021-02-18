using DNA.CastleMinerZ;
using DNA.CastleMinerZ.UI;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using Microsoft.Xna.Framework;
using ModCMZ.Core;
using ModCMZ.Core.Wrappers.DNA.CastleMinerZ.Globalization;
using ModCMZ.Mods.Basic.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DNAGame = DNA.DNAGame;

namespace ModCMZ.Mods.Basic.Runtime.DNA.CastleMinerZ.UI
{
    public static class GraphicsTabMod
    {
        private static ConditionalWeakTable<GraphicsTab, WeakReference<CheckBoxControl>> _borderlessWindowed = new ConditionalWeakTable<GraphicsTab, WeakReference<CheckBoxControl>>();
        private static ConditionalWeakTable<GraphicsTab, WeakReference<CheckBoxControl>> _fullScreen = new ConditionalWeakTable<GraphicsTab, WeakReference<CheckBoxControl>>();
        private static ConditionalWeakTable<GraphicsTab, MutableBox<Rectangle>> _prevScreenSize = new ConditionalWeakTable<GraphicsTab, MutableBox<Rectangle>>();

        public static void Ctor_AfterFullScreen(GraphicsTab instance, bool inGame, ScreenGroup uiGroup)
        {
            _fullScreen.Add(instance, new WeakReference<CheckBoxControl>((CheckBoxControl)instance.Children[instance.Children.Count - 1]));

            var game = CastleMinerZGame.Instance;

            var borderlessWindowed = new CheckBoxControl(game._uiSprites["Unchecked"], game._uiSprites["Checked"])
            {
                Text = "Borderless Windowed:",
                TextColor = Color.White,
                Font = game._medFont
            };

            instance.Children.Add(borderlessWindowed);
            _borderlessWindowed.Add(instance, new WeakReference<CheckBoxControl>(borderlessWindowed));
        }

        public static void OnSelected(GraphicsTab instance)
        {
            if (BasicMod.Instance.BorderlessWindowed.IsBorderlessWindowed)
            {
                CastleMinerZGame.Instance.IsFullScreen = false;
            }

            GetBorderlessWindowed(instance).Checked = BasicMod.Instance.BorderlessWindowed.IsBorderlessWindowed;
        }

        private static CheckBoxControl GetBorderlessWindowed(GraphicsTab instance)
        {
            if (!_borderlessWindowed.TryGetValue(instance, out var borderlessWindowedRef))
            {
                throw new Exception($"Missing table entry for {nameof(_borderlessWindowed)}");
            }

            if (!borderlessWindowedRef.TryGetTarget(out var borderlessWindowed))
            {
                throw new Exception($"Missing target for {nameof(_borderlessWindowed)}");
            }

            return borderlessWindowed;
        }

        private static CheckBoxControl GetFullScreen(GraphicsTab instance)
        {
            if (!_fullScreen.TryGetValue(instance, out var fullScreenRef))
            {
                throw new Exception($"Missing table entry for {nameof(_fullScreen)}");
            }

            if (!fullScreenRef.TryGetTarget(out var fullScreen))
            {
                throw new Exception($"Missing target for {nameof(_fullScreen)}");
            }

            return fullScreen;
        }

        public static void OnUpdate(GraphicsTab instance, DNAGame game, GameTime gameTime)
        {
            var prevScreenSize = _prevScreenSize.GetOrCreateValue(instance);
            if (!instance.SelectedTab && prevScreenSize == Screen.Adjuster.ScreenRect)
            {
                return;
            }

            var borderlessWindowed = GetBorderlessWindowed(instance);
            var fullScreen = GetFullScreen(instance);

            if (instance.SelectedTab)
            {
                if (fullScreen.Checked && borderlessWindowed.Checked)
                {  // User is trying to switch from one to the other.
                    if (BasicMod.Instance.BorderlessWindowed.IsBorderlessWindowed)
                    {
                        borderlessWindowed.Checked = false;
                    }
                    else
                    {
                        fullScreen.Checked = false;
                    }
                }

                BasicMod.Instance.BorderlessWindowed.IsBorderlessWindowed = borderlessWindowed.Checked;
            }

            if (prevScreenSize != Screen.Adjuster.ScreenRect)
            {
                prevScreenSize.Value = Screen.Adjuster.ScreenRect;

                borderlessWindowed.Scale = Screen.Adjuster.ScaleFactor.Y;

                var xStep = (int)(215f * Screen.Adjuster.ScaleFactor.Y);
                var yStep = (int)(50f * Screen.Adjuster.ScaleFactor.Y);

                borderlessWindowed.LocalPosition = new Point(fullScreen.LocalPosition.X + xStep, fullScreen.LocalPosition.Y);
            }
        }
    }
}
