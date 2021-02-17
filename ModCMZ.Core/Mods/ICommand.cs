using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Mods
{
    public interface ICommand : IDisposable
    {
        string Name { get; }
        string Description { get; }
        string Usage { get; }
        string Help { get; }
        IConsole Console { get; set; }

        void Run(CommandArguments args);
    }
}
