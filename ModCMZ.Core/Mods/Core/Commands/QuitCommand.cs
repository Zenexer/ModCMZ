using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModCMZ.Core.Mods.Core.Commands
{
    public class QuitCommand : Command
    {
        public override string Name => "quit";

        public override string Description => "Immediately closes the game";

        public override string Usage => "";

        public override string Help => "Immediately quits the game without any warning.";

        public override void Run(CommandArguments args)
        {
            Application.Exit();
        }
    }
}
