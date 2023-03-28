namespace GangJam;

partial class GangJam
{
	/// <summary>
	/// Contains all events in the game that can be listened to.
	/// </summary>
	public static class Events
	{
		/// <summary>
		/// Invoked once a new game state has been entered.
		/// This event is invoked on the server AFTER the game states <see cref="IGameState.Enter(IGameState)"/> has been invoked.
		/// </summary>
		public const string EnterGameState = "gangjam_entergamestate";
		/// <summary>
		/// Invoked once a game state has been exited.
		/// This event is invoked on the server BEFORE the game states <see cref="IGameState.Exit"/> has been invoked.
		/// </summary>
		public const string ExitGameState = "gangjam_exitgamestate";

		/// <summary>
		/// Invoked once a new game state has been entered.
		/// This event is invoked on the server AFTER the game states <see cref="IGameState.Enter(IGameState)"/> has been invoked.
		/// </summary>
		public class EnterGameStateAttribute : EventAttribute
		{
			public EnterGameStateAttribute() : base( EnterGameState )
			{
			}
		}

		/// <summary>
		/// Invoked once a game state has been exited.
		/// This event is invoked on the server BEFORE the game states <see cref="IGameState.Exit"/> has been invoked.
		/// </summary>
		public class ExitGameStateAttribute : EventAttribute
		{
			public ExitGameStateAttribute() : base( ExitGameState )
			{
			}
		}
	}
}
