using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Mods.Core.Commands
{
    public class HelpCommand : Command
    {
        public override string Name => "help";

        public override string Description => "Provides information about available commands";

        public override string Usage => "[command1] [command2] ...";

        public override string Help => "Provide help for 'command1', 'command2', etc. if specified; otherwise, output a list of commands.";

        public override void Run(CommandArguments args)
        {
            if (args.Count == 0)
            {
                ListCommands();
            }
            else
            {
                foreach (var name in args)
                {
                    HelpWithCommand(name);
                }
            }
        }

        public void ListCommands()
        {
            foreach (var command in Console.Commands.Values.OrderBy(c => c.Name))
            {
                Console.WriteLine("/{0}: {1}", command.Name, command.Description);
            }
        }

        public void HelpWithCommand(string name)
        {
            if (Console.Commands.TryGetValue(name, out var command))
            {
                Console.WriteLine("======= {0} =======", command.Name);
                Console.WriteLine(string.IsNullOrWhiteSpace(command.Usage) ? "Usage: /{0}" : "Usage: /{0} {1}", command.Name, command.Usage);
                Console.WriteLine("Description: {0}", command.Description);
                Console.WriteLine();
                Console.WriteLine(command.Help);
                Console.WriteLine("-------------------");
            }
            else
            {
                Console.WriteLine("Command not found: {0}", name);
            }
        }
    }
}
