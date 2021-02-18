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
    public class WhoisCommand : Command
    {
        public override string Name => "whois";

        public override string Description => "Retrieves info about a player";

        public override string Usage => "<target>";

        public override string Help => "Prints verbose information about any player on the server.";

        public override void Run(CommandArguments args)
        {
            args.AssertArgumentCount(1);

            var gamer = GetGamer(args[0]);
            var player = gamer.GetPlayer();

            var states = new Dictionary<string, bool>
            {
                { "host", gamer.IsHost },
                { "self", gamer.IsLocal },
                { "left session", gamer.HasLeftSession },
                { "flying", player.FlyMode },
                { "in lava", player.InLava },
                { "in water", player.InWater },
                { "under lava", player.UnderLava },
                { "underwater", player.Underwater },
                { "dead", player.Dead },
                { "alive", player.ValidLivingGamer },
                { "active", player.IsActive },
                { "can fall", !player.LockedFromFalling },
                { "grenade ready", player.ReadyToThrowGrenade },
                { "reloading", player.Reloading },
                { "shouldering", player.Shouldering },
                { "using tool", player.UsingTool },
            };

            var statesStr = string.Join(", ", states.Where(kv => kv.Value).Select(kv => kv.Key));

            var fields = new Dictionary<string, string>
            {
                { "Global ID", gamer.Id.ToString() },
                { "Gamertag", gamer.Gamertag },
                { "Player ID", BitConverter.ToString(gamer.PlayerID.Data).Replace("-", "").ToLowerInvariant() },
                { "IP address", gamer.PublicAddress?.ToString() ?? "(none)" },
                { "State", statesStr },
                { "Location", player.LocalPosition.ToString() },
                { "Distance", ((int)Vector2.Distance(new Vector2(0, 0), new Vector2(player.LocalPosition.X, player.LocalPosition.Z))).ToString() },
            };

            foreach (var field in fields)
            {
                Console.WriteLine("{0}: {1}", field.Key, field.Value);
            }
        }
    }
}
