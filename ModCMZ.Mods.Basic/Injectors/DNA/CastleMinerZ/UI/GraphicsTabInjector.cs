using ModCMZ.Core.Injectors;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Mods.Basic.Injectors.DNA.CastleMinerZ.UI
{
    [Injector("CastleMinerZ", "DNA.CastleMinerZ.UI.GraphicsTab")]
    [Serializable]
    public sealed class GraphicsTabInjector : Injector
    {
        [MethodInjector(".ctor")]
        public void InjectCtor()
        {
            var insertAfterFieldName = Type.Fields.Single(f => f.Name == "_fullScreen").FullName;
            var insertAfterInstr = Instructions.Single(i =>
                i.OpCode == OpCodes.Callvirt && ((MethodReference)i.Operand).Name == "Add"
                && i.Previous.OpCode == OpCodes.Ldfld && ((FieldReference)i.Previous.Operand).FullName == insertAfterFieldName
            );

            InsertModCallAfter(insertAfterInstr, "Ctor_AfterFullScreen");
        }

        [MethodInjector("OnSelected")]
        public void InjectOnSelected() => PrependModCall();

        [MethodInjector("OnUpdate")]
        public void InjectOnUpdate() => PrependModCall();
    }
}
