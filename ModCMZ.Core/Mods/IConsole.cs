using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Mods
{
    public interface IConsole
    {
        bool IsVisible { get; set; }
        Dictionary<string, ICommand> Commands { get; }

        void WriteLine();

        void WriteLine(string text);

        void WriteLine(string format, params object[] args);

        void ClearScrollback();
    }
}
