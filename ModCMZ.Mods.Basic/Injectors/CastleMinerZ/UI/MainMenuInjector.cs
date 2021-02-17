using System.Diagnostics;
using DNA.CastleMinerZ.UI;
using ModCMZ.Core.Injectors;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ModCMZ.Mods.Basic.Injectors.DNA.CastleMinerZ.UI
{
    [Injector("CastleMinerZ", "DNA.CastleMinerZ.UI.MainMenu")]
    public class MainMenuInjector : Injector
    {
        // I think this was meant to reveal the "Redeem Code" menu from back when there were integrated cheat codes.
        [MethodInjector(".ctor")]
        public void InjectCtor()
        {
#if CLICKONCE
            try
            {
                Replace(
                    i =>
                    i.OpCode == OpCodes.Ldc_I4_0
                    && i.Next.OpCode == OpCodes.Stfld
                    && ((FieldReference)i.Next.Operand).FullName == "System.Boolean DNA.Drawing.UI.MenuItemElement::Visible",
                    OpCodes.Ldc_I4_1);
                return;
            }
            catch
            {
            }
#endif

            AppendModCall();
        }
    }
}
