using DNA.CastleMinerZ.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Wrappers.DNA.CastleMinerZ.Inventory
{
    public class InventoryItemEx
    {
        private static readonly Lazy<MethodInfo> _registerItemClass = new Lazy<MethodInfo>(() => typeof(InventoryItem).GetMethodEx("RegisterItemClass"));

        public static void RegisterItemClass(InventoryItem.InventoryItemClass itemClass) => _registerItemClass.Value.Invoke(null, new object[] { itemClass });
    }
}
