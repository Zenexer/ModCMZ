using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ModCMZ.Core.Game
{
	public class GameComponentComparer : IComparer<IGameComponent>, IEqualityComparer<IGameComponent>
	{
		#region IComparer<IGameComponent> Members

		public int Compare(IGameComponent x, IGameComponent y)
		{
			var xUpdateable = x as IUpdateable;
			var xDrawable = x as IDrawable;
			var yUpdateable = y as IUpdateable;
			var yDrawable = y as IDrawable;

			if (xUpdateable != null)
			{
				if (yUpdateable != null)
				{
					return xUpdateable.UpdateOrder - yUpdateable.UpdateOrder;
				}
				return -1;
			}
			if (yUpdateable != null)
			{
				return 1;
			}

			if (xDrawable != null)
			{
				if (yDrawable != null)
				{
					return xDrawable.DrawOrder - yDrawable.DrawOrder;
				}
				return -1;
			}
			if (yDrawable != null)
			{
				return 1;
			}

			return 0;
		}

		#endregion

		#region IEqualityComparer<IGameComponent> Members

		public bool Equals(IGameComponent x, IGameComponent y)
		{
			return object.Equals(x, y);
		}

		public int GetHashCode(IGameComponent obj)
		{
			return obj == null ? 0 : obj.GetHashCode();
		}

		#endregion
	}
}
