using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModCMZ.Core.Game
{
	public enum TransactionState
	{
		None = 0,
		Add,
		Remove,
		ReorderUpdateable,
		ReorderDrawable,
	}
}
