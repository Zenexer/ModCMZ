using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using ModCMZ.Core.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Mods.Basic.Runtime.DNA.CastleMinerZ.UI
{
    public static class InGameHUDMod
    {
        public static bool IsGodModeEnabled { get; set; } = false;

        public static void UpdateHostSession_GameHasBegun(NetworkSession instance, string serverName, bool? passwordProtected, bool? isPublic, NetworkSessionProperties sessionProps)
        {
            // Don't close the session
            sessionProps[1] = 0;
        }

        public static bool ApplyDamage(NetworkSession instance, float damageAmount, Vector3 damageSource)
        {
            return !IsGodModeEnabled;
        }
    }
}
