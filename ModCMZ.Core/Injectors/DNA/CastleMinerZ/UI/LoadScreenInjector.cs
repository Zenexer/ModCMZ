using DNA;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Injectors.DNA.CastleMinerZ.UI
{
    [Injector("CastleMinerZ", "DNA.CastleMinerZ.UI.LoadScreen")]
    [Serializable]
    public class LoadScreenInjector : Injector
    {
        [MethodInjector("OnUpdate")]
        public void InjectOnUpdate() => PrependModCallInterceptable();
    }
}
