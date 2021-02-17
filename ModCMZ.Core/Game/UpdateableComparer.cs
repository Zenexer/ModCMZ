using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ModCMZ.Core.Game
{
	public class UpdateableComparer : IComparer<IUpdateable>, IEqualityComparer<IUpdateable>
	{
		#region IComparer<IUpdateable> Members

		public int Compare(IUpdateable x, IUpdateable y)
		{
			return x.UpdateOrder - y.UpdateOrder;
		}

		#endregion

		#region IEqualityComparer<IUpdateable> Members

		public bool Equals(IUpdateable x, IUpdateable y)
		{
			return object.Equals(x, y);
		}

		public int GetHashCode(IUpdateable obj)
		{
			return obj == null ? 0 : obj.GetHashCode();
		}

		#endregion
	}
}
