using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Injectors.DNA.CastleMinerZ.Inventory
{
    [Injector("CastleMinerZ", "DNA.CastleMinerZ.Inventory.InventoryItem")]
    public class InventoryItemInjector : Injector
    {
        // Yes, it's really spelled like that in InventoryItem.
        [MethodInjector("Initalize")]
        public void InjectInitalize() => PrependModCall();
    }
}
