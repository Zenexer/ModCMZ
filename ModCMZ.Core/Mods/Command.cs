using DNA.CastleMinerZ;
using DNA.Net.GamerServices;
using ModCMZ.Core.Extensions;
using ModCMZ.Core.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Mods
{
    public abstract class Command : ICommand
    {
        private bool _disposed;

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Usage { get; }
        public abstract string Help { get; }

        public IConsole Console { get; set; }
        public GameApp Game => Console.Game;
        public NetworkSession CurrentNetworkSession => Game.Game?.CurrentNetworkSession ?? throw new CommandException("Not in a game");
        public NetworkGamer MyNetworkGamer => Game.Game?.MyNetworkGamer ?? throw new CommandException("Not in a game");

        public abstract void Run(CommandArguments args);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            Console = null;
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Player GetPlayer(string input) => GetGamer(input).GetPlayer();

        public NetworkGamer GetGamer(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new CommandException("Gamertag is empty or null");
            }

            switch (input[0])
            {
                case '@':
                    if (input.Length < 2)
                    {
                        throw new CommandException("Target selector symbol (@) must be followed by a valid selector (e.g., @s)");
                    }

                    switch (input[1])
                    {
                        case 's':
                            return MyNetworkGamer;

                        default:
                            throw new CommandException($"Unrecognized target selector: {input}");
                    }

                case '#':
                    if (input.Length < 2)
                    {
                        throw new CommandException("Gamer ID symbol (#) must be followed by a valid gamer ID");
                    }

                    var idStr = input.Substring(1);
                    if (idStr.Length > 3 || !byte.TryParse(idStr, out var id))
                    {
                        throw new CommandException($"Couldn't parse gamer ID: {idStr}");
                    }

                    var gamer = CurrentNetworkSession.FindGamerById(id);

                    if (gamer == null || gamer.IsDisposed)
                    {
                        throw new CommandException($"No gamer exists with ID #{id}");
                    }

                    return gamer;
            }

            var matches = (from g in (IEnumerable<NetworkGamer>)CurrentNetworkSession.AllGamers
                           where !g.IsDisposed && g.Gamertag.Equals(input, StringComparison.InvariantCultureIgnoreCase)
                           select g).ToArray();

            if (matches.Length > 1)
            {
                matches = matches.Where(g => !g.HasLeftSession).ToArray();
            }

            if (matches.Length > 1)
            {
                throw new CommandException($"Gamertag is ambiguous: {input}");
            }
            else if (matches.Length == 0)
            {
                throw new CommandException($"Gamertag doesn't exist: {input}");
            }

            return matches[0];
        }
    }
}
