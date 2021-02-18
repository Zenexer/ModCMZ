using DNA.CastleMinerZ;
using DNA.Net.GamerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Extensions
{
    public static class GamerExtension
    {
        public static Player GetPlayer(this NetworkGamer gamer) => (Player)gamer.Tag;
    }
}
