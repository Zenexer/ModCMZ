using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Mods.Core.Commands
{
    public class ExitCommand : Command
    {
        public override string Name => "exit";

        public override string Description => "Closes the console";

        public override string Usage => "";

        public override string Help => "Hides the console, preserving the scrollback.";

        public override void Run(CommandArguments args)
        {
            Console.IsVisible = false;
        }
    }
}
