using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using ModCMZ.Core.Game;
using ModCMZ.Core.Runtime.DNA.CastleMinerZ.GraphicsProfileSupport;

namespace ModCMZ.Core.Mods
{
    public interface IMod
    {
        string Id { get; }
        string Name { get; }
        Version Version { get; }
        string Author { get; }
        string Description { get; }

        Stream OpenContentStream(string assetName);
        void OnLaunched();
        void OnLaunching();
        void OnGameReady(GameApp game);
        void OnDomainReady();
        void OnComponentsReady();
        void OnRegisteringItems(ModContentManager content);
        void OnClaimingContent(ModContentManager content);
    }
}
