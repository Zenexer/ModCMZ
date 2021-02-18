using ModCMZ.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Mods.Basic.Commands
{
    public class TpCommand : Command
    {
        public override string Name => "tp";

        public override string Description => "Teleports a player";

        public override string Usage => "[subject] <target>";

        public override string Help => "Teleports [subject] to <target>.  If no subject is specified, the current player is the subject.";

        public override void Run(CommandArguments args)
        {
            args.AssertArgumentCountBetween(1, 2);

            var subjectStr = args.Count > 1 ? args[0] : "@s";
            var targetStr = args[args.Count - 1];

            var subject = GetPlayer(subjectStr);
            var target = GetPlayer(targetStr);

            if (!subject.IsLocal && !CurrentNetworkSession.IsHost)
            {
                throw new CommandException("You can't teleport other players unless you're the host.");
            }

            subject.LocalPosition = target.LocalPosition;
        }
    }
}
