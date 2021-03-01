using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DNA;
using DNA.CastleMinerZ;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using ModCMZ.Core.Runtime.DNA.CastleMinerZ.GraphicsProfileSupport;

namespace ModCMZ.Core.Game
{
    /// <summary>
    /// Similar to App, but stays within the domain of CastleMiner Z for the most part, and wraps the Game object.
    /// </summary>
    public class GameApp
    {
        private readonly GameComponentComparer m_GameComponentComparer = new GameComponentComparer();
        private readonly UpdateableComparer m_UpdateableComparer = new UpdateableComparer();
        private readonly DrawableComparer m_DrawableComparer = new DrawableComparer();
        private SortedSet<IGameComponent> m_Components;
        private SortedSet<IUpdateable> m_UpdateableComponents;
        private SortedSet<IDrawable> m_DrawableComponents;
        private readonly ConcurrentQueue<ComponentTransaction> m_ComponentTransactions = new ConcurrentQueue<ComponentTransaction>();

        public event EventHandler LoadingComponents;
        public event EventHandler LoadingContent;

        public GameComponentCollection Components { get; private set; }
        public bool IsInitialized { get; private set; }
        public CastleMinerZGame Game { get; private set; }
        public Form Form { get; private set; }

        public ModContentManager Content => (ModContentManager)Game.Content;

        public GameApp()
        {
            Components = new GameComponentCollection();
            Components.ComponentAdded += Components_ComponentAdded;
            Components.ComponentRemoved += Components_ComponentRemoved;
        }

        private void Components_ComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            m_ComponentTransactions.Enqueue(new ComponentTransaction(e.GameComponent, TransactionState.Add));
        }

        private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            m_ComponentTransactions.Enqueue(new ComponentTransaction(e.GameComponent, TransactionState.Remove));
        }

        public static void InitializeHook(DNAGame instance)
        {
            var current = App.Current.Game;
            current.Game = (CastleMinerZGame)instance;

            current.Initialize();
        }

        public static void UpdateHook(DNAGame instance, GameTime time)
        {
            App.Current.Game.Update(time);
        }

        public static void DrawHook(DNAGame instance, GameTime time)
        {
            App.Current.Game.Draw(time);
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            if (Game == null)
            {
                throw new NullReferenceException("GameApp.Game cannot be null during initialization.");
            }

            try
            {
                AttachToWindow();
            }
            catch (Exception ex)
            {
                App.Current.Fatal("Error attaching to game window: {0}", ex.Message);
            }

            try
            {
                App.Current.OnGameReady(this);
                LoadComponents();
            }
            finally
            {
                IsInitialized = true;
            }
        }

        private void LoadComponents()
        {
            m_Components = new SortedSet<IGameComponent>(m_GameComponentComparer);
            m_UpdateableComponents = new SortedSet<IUpdateable>(m_UpdateableComparer);
            m_DrawableComponents = new SortedSet<IDrawable>(m_DrawableComparer);

            OnLoadingContent();
            OnLoadingComponents();
            InitializeComponents();
        }

        private void AttachToWindow()
        {
            var handle = Game.Window.Handle;
            if (handle == IntPtr.Zero)
            {
                throw new Exception("Game window handle is null (zero).");
            }

            var form = Control.FromHandle(handle) as Form;
            if (form == null)
            {
                throw new Exception("Game control is not a window.");
            }

            Form = form;
        }

        private void OnLoadingContent()
        {
            if (LoadingContent != null)
            {
                LoadingContent(this, EventArgs.Empty);
            }
        }

        private void OnLoadingComponents()
        {
            if (LoadingComponents != null)
            {
                LoadingComponents(this, EventArgs.Empty);
            }
        }

        private void InitializeComponents()
        {
            foreach (var component in m_Components)
            {
                component.Initialize();
            }
        }

        public void Update(GameTime time)
        {
            PerformTransactions();

            foreach (var component in m_UpdateableComponents.Where(c => c.Enabled))
            {
                component.Update(time);
            }
        }

        public void Draw(GameTime time)
        {
            foreach (var component in m_DrawableComponents.Where(c => c.Visible).OrderBy(c => c.DrawOrder))
            {
                component.Draw(time);
            }
        }

        private void PerformTransactions()
        {
            for (ComponentTransaction transaction; m_ComponentTransactions.TryDequeue(out transaction); )
            {
                switch (transaction.State)
                {
                case TransactionState.Add:
                    AddComponentUnsafe(transaction.Component);
                    break;

                case TransactionState.Remove:
                    RemoveComponentUnsafe(transaction.Component);
                    break;

                case TransactionState.ReorderUpdateable:
                    ReorderUpdateableComponents();
                    break;

                case TransactionState.ReorderDrawable:
                    ReorderDrawableComponentsUnsafe();
                    break;
                }
            }
        }

        private void AddComponentUnsafe(IGameComponent component)
        {
            m_Components.Add(component);

            var updateable = component as IUpdateable;
            if (updateable != null)
            {
                updateable.UpdateOrderChanged += updateable_UpdateOrderChanged;
                m_UpdateableComponents.Add(updateable);
            }

            var drawable = component as IDrawable;
            if (drawable != null)
            {
                drawable.DrawOrderChanged += drawable_DrawOrderChanged;
                m_DrawableComponents.Add(drawable);
            }

            if (IsInitialized)
            {
                component.Initialize();
            }
        }

        private void updateable_UpdateOrderChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void drawable_DrawOrderChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RemoveComponentUnsafe(IGameComponent component)
        {
            m_Components.Remove(component);

            var updateable = component as IUpdateable;
            if (updateable != null)
            {
                updateable.UpdateOrderChanged -= updateable_UpdateOrderChanged;
                m_UpdateableComponents.Remove(updateable);
            }

            var drawable = component as IDrawable;
            if (drawable != null)
            {
                drawable.DrawOrderChanged -= drawable_DrawOrderChanged;
                m_DrawableComponents.Remove(drawable);
            }

            var disposable = component as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        private void ReorderUpdateableComponents()
        {
            m_UpdateableComponents = new SortedSet<IUpdateable>(m_UpdateableComponents, m_UpdateableComparer);
        }

        private void ReorderDrawableComponentsUnsafe()
        {
            m_DrawableComponents = new SortedSet<IDrawable>(m_DrawableComponents, m_DrawableComparer);
        }

        private void ReorderComponentsUnsafe()
        {
            if (IsInitialized)
            {
                return;
            }

            m_Components = new SortedSet<IGameComponent>(m_Components, m_GameComponentComparer);
        }
    }
}
