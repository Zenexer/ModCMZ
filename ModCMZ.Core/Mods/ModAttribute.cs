﻿using System;
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
        public string Name;
        public string Author;
        public string VersionString;
        public string Description;

        private Version m_Version;

        public Version Version
        {
            get
            {
                if (m_Version == null)
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

                    m_Version = version;
                }

                return m_Version;
            }
        }

        public ModAttribute(string name)
        {
            Name = name;
        }
    }
}
