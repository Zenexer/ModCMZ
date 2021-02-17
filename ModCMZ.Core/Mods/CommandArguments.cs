using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Mods
{
    public sealed class CommandArguments : IReadOnlyList<string>, IReadOnlyCollection<string>
    {
        private readonly string[] _tokens;

        internal CommandArguments(string[] tokens) => _tokens = tokens;

        public string this[int index] => _tokens[index + 1];

        public int Count => _tokens.Length - 1;

        public IEnumerator<string> GetEnumerator() => _tokens.Skip(1).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
