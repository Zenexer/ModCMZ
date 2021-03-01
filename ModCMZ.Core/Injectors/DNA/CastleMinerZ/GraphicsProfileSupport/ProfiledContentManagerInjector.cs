using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Injectors.DNA.CastleMinerZ.GraphicsProfileSupport
{
    [Injector("CastleMinerZ", "DNA.CastleMinerZ.GraphicsProfileSupport.ProfiledContentManager")]
    public class ProfiledContentManagerInjector : Injector
    {
        [MethodInjector("Load")]
        public void InjectLoad()
        {
            Type.Methods.Remove(Method);
        }
    }
}
