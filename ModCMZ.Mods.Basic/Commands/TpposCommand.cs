using Microsoft.Xna.Framework;
using ModCMZ.Core.Extensions;
using ModCMZ.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Mods.Basic.Commands
{
    public class TpposCommand : Command
    {
        public override string Name => "tppos";

        public override string Description => "Teleports to a set of coordinates";

        public override string Usage => "<x> <y> <z>";

        public override string Help => "Teleports the current player to (x, y, z).";

        public override void Run(CommandArguments args)
        {
            args.AssertArgumentCount(3);

            var location = new Vector3(args.ReadFloat(0), args.ReadFloat(1), args.ReadFloat(2));

            MyNetworkGamer.GetPlayer().LocalPosition = location;
        }
    }
}
