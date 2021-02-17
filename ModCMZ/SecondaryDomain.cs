using ModCMZ.Core;
using ModCMZ.Mods.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ
{
	public class SecondaryDomain : MarshalByRefObject
	{
		[STAThread]
		public void Run(string[] args)
		{
			//DebugSandbox.Run();

			App.SetReferencedAssemblies(new[]
			{
				typeof(App).Assembly.Location,
				typeof(BasicMod).Assembly.Location,

				// Not necessary?
				//typeof(Microsoft.Xna.Framework.Point).Assembly.Location,
				//typeof(Microsoft.Xna.Framework.Game).Assembly.Location,
				//typeof(Microsoft.Xna.Framework.Graphics.GraphicsDevice).Assembly.Location,
				//typeof(Microsoft.Xna.Framework.Audio.AudioEngine).Assembly.Location,
			});

			var app = App.Create();
			app.ProgramArgs = args;
			app.Run();
		}
	}
}
