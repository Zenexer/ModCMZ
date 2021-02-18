using DNA.CastleMinerZ.UI;
using ModCMZ.Core.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Mods.Basic.Commands
{
    public class ModeCommand : Command
    {
        public override string Name => "mode";

        public override string Description => "Sets the game mode";

        public override string Usage => "<Endurance|Survival|DragonEndurance|Creative|Exploration|Scavenger>";

        public override string Help => "Sets the game mode to the specified value.";

        public override void Run(CommandArguments args)
        {
            args.AssertArgumentCount(1);

            GameModeTypes mode;

            if (int.TryParse(args[0], out var intVal))
            {
                mode = (GameModeTypes)intVal;
            }
            else if (Enum.TryParse<GameModeTypes>(args[0], out var modeVal))
            {
                mode = modeVal;
            }
            else
            {
                throw new CommandException($"Invalid game mode: {args[0]}");
            }

            Game.Game.GameMode = mode;
        }
    }
}
