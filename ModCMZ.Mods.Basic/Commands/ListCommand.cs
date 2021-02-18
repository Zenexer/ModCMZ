using DNA.Net.GamerServices;
using ModCMZ.Core;
using ModCMZ.Core.Extensions;
using ModCMZ.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Mods.Basic.Commands
{
    public class ListCommand : Command
    {
        public override string Name => "list";

        public override string Description => "List all players on the server";

        public override string Usage => "";

        public override string Help => "Lists all players on the server.";

        public override void Run(CommandArguments args)
        {
            args.AssertEmpty();

            var gamers = CurrentNetworkSession.AllGamers;

            foreach (var gamer in gamers)
            {
                var gamertag = gamer.Gamertag;
                //var playerIdStr = BitConverter.ToString(gamer.PlayerID.Data).Replace("-", "").ToLowerInvariant();
                var ipAddress = gamer.PublicAddress == null ? "" : $" - {gamer.PublicAddress}";

                string status = "";
                if (gamer.IsHost)
                {
                    if (gamer.IsLocal)
                    {
                        status = " - host, local";
                    }
                    else
                    {
                        status = " - host";
                    }
                }
                else if (gamer.IsLocal)
                {
                    status = " - local"; 
                }

                Console.WriteLine($"{gamer.Id}: {gamertag}{status}{ipAddress}");
            }
        }
    }
}
