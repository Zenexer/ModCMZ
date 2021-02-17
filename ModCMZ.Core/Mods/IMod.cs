using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModCMZ.Core.Game;

namespace ModCMZ.Core.Mods
{
    public interface IMod
    {
        string Name { get; }
        Version Version { get; }
        string Author { get; }
        string Description { get; }

        void OnLaunched();

        void OnLaunching();

        void OnGameReady(GameApp game);

        void OnDomainReady();

        void OnComponentsReady();
    }
}
