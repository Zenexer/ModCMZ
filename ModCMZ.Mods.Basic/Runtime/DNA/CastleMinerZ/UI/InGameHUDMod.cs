using DNA.Net.GamerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Mods.Basic.Runtime.DNA.CastleMinerZ.UI
{
    public static class InGameHUDMod
    {
        public static void UpdateHostSession_GameHasBegun(NetworkSession instance, string serverName, bool? passwordProtected, bool? isPublic, NetworkSessionProperties sessionProps)
        {
            // Empty: don't close the session
        }
    }
}
