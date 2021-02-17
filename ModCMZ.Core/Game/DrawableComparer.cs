using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ModCMZ.Core.Game
{
	public class DrawableComparer : IComparer<IDrawable>, IEqualityComparer<IDrawable>
	{
		#region IComparer<IDrawable> Members

		public int Compare(IDrawable x, IDrawable y)
		{
			return x.DrawOrder - y.DrawOrder;
		}

		#endregion

		#region IEqualityComparer<IDrawable> Members

		public bool Equals(IDrawable x, IDrawable y)
		{
			return object.Equals(x, y);
		}

		public int GetHashCode(IDrawable obj)
		{
			return obj == null ? 0 : obj.GetHashCode();
		}

		#endregion
	}
}
