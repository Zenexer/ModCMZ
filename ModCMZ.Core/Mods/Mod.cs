using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using ModCMZ.Core.Game;
using ModCMZ.Core.Runtime.DNA.CastleMinerZ.GraphicsProfileSupport;

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
        public event Action Launching;
        
        /// <summary>
        /// CastleMiner Z has launched.  
        /// </summary>
        public event Action Launched;

        /// <summary>
        /// First event in the new application domain.
        /// </summary>
        /// <remarks>
        /// You can use this to add your other events, but that's probably a premature optimization.
        /// </remarks>
        public event Action DomainReady;

        /// <summary>
        /// <see cref="GameApp"/> is ready.
        /// </summary>
        public event Action<GameApp> GameReady;

        /// <summary>
        /// Inventory items are being registered.
        /// </summary>
        public event Action<ModContentManager> RegisteringItems;

        public event Action<Action<string>> ClaimingContent;

        /// <summary>
        /// Components have been initialized.  Called after <see cref="GameReady"/>.
        /// </summary>
        public event Action ComponentsReady;

        private bool _checkedAttribute;
        private ModAttribute _attribute;
        private Version _version;

        public ModAttribute Attribute
        {
            get
            {
                if (!_checkedAttribute)
                {
                    _attribute = GetType().GetCustomAttribute<ModAttribute>();
                    _checkedAttribute = true;
                }

                return _attribute;
            }
        }

        public string Id => Attribute.Id;
        public string Name => Attribute?.Name;
        public virtual string Author => Attribute?.Author;
        public virtual string Description => Attribute?.Description;

        public virtual Version Version
        {
            get
            {
                if (_version == null)
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

                        if (Version.TryParse(attribute.Version, out var version))
                        {
                            _version = version;
                        }
                    }
                    else
                    {
                        _version = Attribute.Version;
                    }
                }

                return _version;
            }
        }

        private Dictionary<string, string> _embeddedContent;
        private IReadOnlyDictionary<string, string> EmbeddedContent
        {
            get
            {
                if (_embeddedContent == null)
                {
                    var embeddedContent = new Dictionary<string, string>();
                    var contentPrefix = $"{Assembly.GetName().Name}.Content.";

                    var allResourceNames = Assembly.GetManifestResourceNames();
                    var contentResourceNames = allResourceNames.Where(x => x.StartsWith(contentPrefix));

                    foreach (var resourceName in contentResourceNames)
                    {
                        var assetKey = resourceName.Substring(contentPrefix.Length);
                        assetKey = assetKey.Remove(assetKey.LastIndexOf('.'));
                        assetKey = assetKey.ToLowerInvariant();

                        embeddedContent.Add(assetKey, resourceName);
                    }

                    _embeddedContent = embeddedContent;
                }

                return _embeddedContent;
            }
        }

        public Assembly Assembly => GetType().Assembly;

        public virtual Stream OpenContentStream(string assetName)
        {
            var assetKey = assetName
                .Replace('\\', '.')
                .Replace('/', '.')
                .ToLowerInvariant();

            var resourceName = EmbeddedContent[assetKey];

            return Assembly.GetManifestResourceStream(resourceName);
        }


        public virtual void OnLaunched() => Launched?.Invoke();

        public virtual void OnLaunching() => Launching?.Invoke();

        public virtual void OnDomainReady() => DomainReady?.Invoke();

        public virtual void OnGameReady(GameApp game) => GameReady?.Invoke(game);

        public virtual void OnComponentsReady() => ComponentsReady?.Invoke();

        public virtual void OnRegisteringItems(ModContentManager content) => RegisteringItems?.Invoke(content);

        public virtual void OnClaimingContent(ModContentManager content) => ClaimingContent?.Invoke((assetName) => content.ClaimContent(this, assetName));
    }
}
