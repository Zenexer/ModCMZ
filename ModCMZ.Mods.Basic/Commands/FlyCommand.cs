using ModCMZ.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Mods.Basic.Commands
{
    public class FlyCommand : Command
    {
        public override string Name => "fly";

        public override string Description => "Toggle flight";

        public override string Usage => "";

        public override string Help => "Toggle flight on or off, regardless of mode.";

        public override void Run(CommandArguments args)
        {
            args.AssertEmpty();

            var player = GetPlayer("@s");
            player.FlyMode = !player.FlyMode;

            Console.WriteLine("Flight {0}.", player.FlyMode ? "enabled" : "disabled");
        }
    }
}
