using Microsoft.Xna.Framework.Content;
using ModCMZ.Core.Runtime.DNA.CastleMinerZ.GraphicsProfileSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Runtime.DNA.CastleMinerZ.Inventory
{
    public static class InventoryItemMod
    {
        // This isn't a typo.
        public static void Initalize(ContentManager content) => App.Current.OnRegisteringItems((ModContentManager)content);
    }
}
