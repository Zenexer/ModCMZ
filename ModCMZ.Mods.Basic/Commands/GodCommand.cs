using DNA.CastleMinerZ.UI;
using ModCMZ.Core.Mods;
using ModCMZ.Mods.Basic.Runtime.DNA.CastleMinerZ.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Mods.Basic.Commands
{
    public class GodCommand : Command
    {
        public override string Name => "god";

        public override string Description => "Toggles god mode";

        public override string Usage => "";

        public override string Help => "Toggles invincibility for the current player.";

        public override void Run(CommandArguments args)
        {
            args.AssertEmpty();

            InGameHUDMod.IsGodModeEnabled = !InGameHUDMod.IsGodModeEnabled;

            Console.WriteLine("God mode {0}.", InGameHUDMod.IsGodModeEnabled ? "enabled" : "disabled");
        }
    }
}
