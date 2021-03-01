using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Mods
{
    /// <summary>
    /// Marks an individual mod.  Must be applied to a class implementing <see cref="IMod"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ModAttribute : Attribute
    {
        public string Id;
        public string Name;
        public string Author;
        public string VersionString;
        public string Description;

        private Version _version;

        public Version Version
        {
            get
            {
                if (_version == null)
                {
                    if (VersionString == null)
                    {
                        return null;
                    }

                    Version version;
                    if (!Version.TryParse(VersionString, out version))
                    {
                        throw new InvalidOperationException(string.Format("Version string for mod {0} is invalid.", Name));
                    }

                    _version = version;
                }

                return _version;
            }
        }

        public ModAttribute(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
