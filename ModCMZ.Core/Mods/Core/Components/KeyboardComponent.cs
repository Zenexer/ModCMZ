using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ModCMZ.Core.Game;
using Keys = System.Windows.Forms.Keys;

namespace ModCMZ.Core.Mods.Core.Components
{
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    public class KeyboardComponent : GameComponent, IMessageFilter
    {
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_CHAR = 0x102;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_UNICHAR = 0x109;
        private const int UNICODE_NOCHAR = 0xffff;

        public event KeyboardStateChangedEventHandler StateChanged;
        public event KeyPressEventHandler KeyPress;
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;

        private int m_InterceptCount = 0;

        public new GameApp Game { get; private set; }

        public CoreMod Core { get; private set; }

        public KeyboardState State { get; private set; }

        public bool IsIntercepting
        {
            get
            {
                return m_InterceptCount > 0;
            }
        }

        public KeyboardComponent(GameApp game, CoreMod core)
            : base(game.Game)
        {
            Game = game;
            Core = core;

            UpdateOrder = -10000;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                Application.RemoveMessageFilter(this);
            }
            catch (Exception ex)
            {
                App.Current.Debug("Keyboard message filter removal threw an exception: {0}", ex);
            }

            base.Dispose(disposing);
        }

        public void Intercept()
        {
            m_InterceptCount++;
        }

        public void Release()
        {
            if (m_InterceptCount > 0)
            {
                m_InterceptCount--;
            }
        }

        public override void Initialize()
        {
            Application.AddMessageFilter(this);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            var oldState = State;
            var newState = Keyboard.GetState();

            if (newState != oldState)
            {
                State = newState;
                OnStateChanged(oldState, newState);
            }

            base.Update(gameTime);
        }

        private void OnStateChanged(KeyboardState oldState, KeyboardState newState)
        {
            if (!Game.Game.IsActive)
            {
                return;
            }

            if (StateChanged != null)
            {
                StateChanged(this, new KeyboardStateChangedEventArgs(oldState, newState));
            }
        }

        private async Task OnKeyPress(char c)
        {
            if (KeyPress != null)
            {
                await Task.Run(() => KeyPress(this, new KeyPressEventArgs(c)));
            }
        }

        private async Task OnKeyDown(int key)
        {
            if (KeyDown != null)
            {
                await Task.Run(() => KeyDown(this, new KeyEventArgs((Keys)key)));
            }
        }

        private async Task OnKeyUp(int key)
        {
            if (KeyUp != null)
            {
                await Task.Run(() => KeyUp(this, new KeyEventArgs((Keys)key)));
            }
        }

        #region IMessageFilter Members

        public bool PreFilterMessage(ref Message m)
        {
            Task task;

            switch (m.Msg)
            {
                case WM_KEYDOWN:  // Doesn't work
                case WM_SYSKEYDOWN:  // Does work, but useless
                    //Console.WriteLine(m.WParam.ToInt64());
                    task = OnKeyDown(m.WParam.ToInt32());
                    break;

                // But this works
                case WM_KEYUP:
                    task = OnKeyUp(m.WParam.ToInt32());
                    break;

                case WM_UNICHAR:
                    if (m.WParam.ToInt32() == UNICODE_NOCHAR)
                    {
                        return true;
                    }
                    goto case WM_CHAR;

                case WM_CHAR:
                    task = OnKeyPress((char)m.WParam);
                    break;
            }

            return false;
        }

        #endregion
    }
}
