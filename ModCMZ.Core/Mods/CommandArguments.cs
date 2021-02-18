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

        public string this[int index]
        {
            get
            {
                if (index >= Count)
                {
                    throw new CommandException($"Command tried to access argument {index} (zero-based), but only {Count} arguments were provided");
                }

                return _tokens[index + 1];
            }
        }

        public int Count => _tokens.Length - 1;

        public IEnumerator<string> GetEnumerator() => _tokens.Skip(1).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AssertEmpty()
        {
            if (Count > 0)
            {
                throw new CommandException("Command doesn't accept arguments");
            }
        }

        public void AssertMinArgumentCount(int min)
        {
            if (Count < min)
            {
                throw new CommandException($"Command requires at least {min} argument(s)");
            }
        }

        public void AssertMaxArgumentCount(int max)
        {
            if (Count > max)
            {
                throw new CommandException($"Command requires no more than {max} argument(s)");
            }
        }

        public void AssertArgumentCountBetween(int min, int max)
        {
            if (Count < min || Count > max)
            {
                throw new CommandException($"Command requires between {min} and {max} argument(s), inclusive");
            }
        }

        public void AssertArgumentCount(int exactCount)
        {
            if (exactCount == 0)
            {
                AssertEmpty();
            }
            else if (Count != exactCount)
            {
                throw new CommandException($"Command requires exactly {exactCount} argument(s)");
            }
        }

        public float ReadFloat(int index)
        {
            var str = this[index];

            if (!float.TryParse(str, out var value))
            {
                throw new CommandException($"Command requires argument {index} (zero-based) to be a valid number (float)");
            }

            return value;
        }
    }
}
