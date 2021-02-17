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
    }
}
