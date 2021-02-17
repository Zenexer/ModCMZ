using DNA.Distribution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Net
{
	public class ModOnlineServices : NullOnlineServices, IDisposable
	{
		public ModOnlineServices(Guid productId)
			: base(productId)
		{
			_username = Environment.UserName;
			_steamUserID = unchecked((ulong)Environment.UserName.GetHashCode());
		}

		public void Dispose()
		{
			// For future use
		}
	}
}
