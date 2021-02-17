using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ModCMZ.Core.Game;

namespace ModCMZ.Core.Mods
{
    public abstract class Mod : IMod
    {
        /// <summary>
        /// CastleMiner Z is about to launch.  The game's domain isn't ready yet, and the mod only exists in the main domain.
        /// </summary>
        /// <remarks>
        /// Warning: Invoked from the main application domain, not the game's application domain.  Don't store anything during this call.
        /// </remarks>
        public event EventHandler Launching;
        
        /// <summary>
        /// CastleMiner Z has launched.  
        /// </summary>
        public event EventHandler Launched;

        /// <summary>
        /// First event in the new application domain.
        /// </summary>
        /// <remarks>
        /// You can use this to add your other events, but that's probably a premature optimization.
        /// </remarks>
        public event EventHandler DomainReady;

        /// <summary>
        /// <see cref="GameApp"/> is ready.
        /// </summary>
        public event GameReadyEventHandler GameReady;

        /// <summary>
        /// Components have been initialized.  Called after <see cref="GameReady"/>.
        /// </summary>
        public event EventHandler ComponentsReady;

        private bool m_CheckedAttribute;
        private ModAttribute m_Attribute;
        private Version m_Version;

        public ModAttribute Attribute
        {
            get
            {
                if (!m_CheckedAttribute)
                {
                    m_Attribute = GetType().GetCustomAttribute<ModAttribute>();
                    m_CheckedAttribute = true;
                }

                return m_Attribute;
            }
        }

        public string Name
        {
            get
            {
                return Attribute == null ? null : Attribute.Name;
            }
        }

        public virtual Version Version
        {
            get
            {
                if (m_Version == null)
                {
                    if (Attribute == null)
                    {
                        return null;
                    }

                    if (Attribute.Version == null)
                    {
                        var attribute = GetType().Assembly.GetCustomAttribute<AssemblyVersionAttribute>();
                        if (attribute == null)
                        {
                            return null;
                        }

                        Version version;
                        if (Version.TryParse(attribute.Version, out version))
                        {
                            m_Version = version;
                        }
                    }
                    else
                    {
                        m_Version = Attribute.Version;
                    }
                }

                return m_Version;
            }
        }

        public virtual string Author
        {
            get
            {
                return Attribute == null ? null : Attribute.Author;
            }
        }

        public virtual string Description
        {
            get
            {
                return Attribute == null ? null : Attribute.Description;
            }
        }

        #region IMod Members

        public virtual void OnLaunched()
        {
            if (Launched != null)
            {
                Launched.Invoke(this, EventArgs.Empty);
            }
        }

        public virtual void OnLaunching()
        {
            if (Launching != null)
            {
                Launching.Invoke(this, EventArgs.Empty);
            }
        }

        public virtual void OnDomainReady()
        {
            if (DomainReady != null)
            {
                DomainReady.Invoke(this, EventArgs.Empty);
            }
        }

        public virtual void OnGameReady(GameApp game)
        {
            if (GameReady != null)
            {
                GameReady.Invoke(this, new GameReadyEventArgs(game));
            }
        }

        public virtual void OnComponentsReady()
        {
            if (ComponentsReady != null)
            {
                ComponentsReady.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
