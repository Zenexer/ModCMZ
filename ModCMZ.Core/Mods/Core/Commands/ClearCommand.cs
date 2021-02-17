using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Mods.Core.Commands
{
    public class ClearCommand : Command
    {
        public override string Name => "clear";

        public override string Description => "Clears the screen";

        public override string Usage => "";

        public override string Help => "Clears all scrollback.  Input history is preserved.";

        public override void Run(CommandArguments args)
        {
            Console.ClearScrollback();
        }
    }
}
