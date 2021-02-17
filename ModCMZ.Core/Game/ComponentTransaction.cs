using Microsoft.Xna.Framework;

namespace ModCMZ.Core.Game
{
	public sealed class ComponentTransaction
	{
		public IGameComponent Component { get; private set; }
		public TransactionState State { get; private set; }

		public ComponentTransaction(IGameComponent component, TransactionState state)
		{
			Component = component;
			State = state;
		}
	}
}
