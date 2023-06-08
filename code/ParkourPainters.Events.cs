namespace SpeedPainters;

partial class ParkourPainters
{
	/// <summary>
	/// Contains all events in the game that can be listened to.
	/// </summary>
	public static class Events
	{
		/// <summary>
		/// <para>Invoked once a new game state has been entered.
		/// This event is invoked on the server AFTER the game states <see cref="IGameState.Enter(IGameState)"/> has been invoked.</para>
		/// <para>Parameter 0 is the new game state being entered (type of <see cref="IGameState"/>).</para>
		/// <para>Parameter 1 is the old game state being left (type of <see cref="IGameState"/>).</para>
		/// </summary>
		internal const string EnterGameState = "ParkourPainters_entergamestate";
		/// <summary>
		/// <para>Invoked once a game state has been exited.
		/// This event is invoked on the server BEFORE the game states <see cref="IGameState.Exit"/> has been invoked.</para>
		/// <para>Parameter 0 is the game state being left (type of <see cref="IGameState"/>).</para>
		/// </summary>
		internal const string ExitGameState = "ParkourPainters_exitgamestate";
		/// <summary>
		/// <para>Invoked once a teams <see cref="GraffitiArea"/> has been interacted with by another team.
		/// This event is invoked on the server.</para>
		/// <para>Parameter 0 is the old team that owned the <see cref="GraffitiArea"/> (type of <see cref="Team"/>).</para>
		/// <para>Parameter 1 is the new team that owns the <see cref="GraffitiArea"/> (type of <see cref="Team"/>).</para>
		/// <para>Parameter 2 is the player that caused the tampering (type of <see cref="Player"/>).</para>
		/// </summary>
		internal const string GraffitiSpotTampered = "ParkourPainters_graffitispottampered";
		/// <summary>
		/// <para>Invoked once a team has completed a <see cref="GraffitiArea"/>.
		/// This event is invoked on the server.</para>
		/// <para>Parameter 0 is the team that completed the <see cref="GraffitiArea"/> (type of <see cref="Team"/>).</para>
		/// <para>Parameter 1 is the player that completed the <see cref="GraffitiArea"/> (type of <see cref="Player"/>).</para>
		/// </summary>
		internal const string GraffitiSpotCompleted = "ParkourPainters_graffitispotcompleted";

		/// <summary>
		/// <para>Invoked once a new game state has been entered.
		/// This event is invoked on the server AFTER the game states <see cref="IGameState.Enter(IGameState)"/> has been invoked.</para>
		/// <para>Parameter 0 is the new game state being entered (type of <see cref="IGameState"/>).</para>
		/// <para>Parameter 1 is the old game state being left (type of <see cref="IGameState"/>).</para>
		/// </summary>
		public class EnterGameStateAttribute : EventAttribute
		{
			public EnterGameStateAttribute() : base( EnterGameState )
			{
			}
		}

		/// <summary>
		/// <para>Invoked once a game state has been exited.
		/// This event is invoked on the server BEFORE the game states <see cref="IGameState.Exit"/> has been invoked.</para>
		/// <para>Parameter 0 is the game state being left (type of <see cref="IGameState"/>).</para>
		/// </summary>
		public class ExitGameStateAttribute : EventAttribute
		{
			public ExitGameStateAttribute() : base( ExitGameState )
			{
			}
		}

		/// <summary>
		/// <para>Invoked once a teams <see cref="GraffitiArea"/> has been interacted with by another team.
		/// This event is invoked on the server.</para>
		/// <para>Parameter 0 is the old team that owned the <see cref="GraffitiArea"/> (type of <see cref="Team"/>).</para>
		/// <para>Parameter 1 is the new team that owns the <see cref="GraffitiArea"/> (type of <see cref="Team"/>).</para>
		/// <para>Parameter 2 is the player that caused the tampering (type of <see cref="Player"/>).</para>
		/// </summary>
		public class GraffitiSpotTamperedAttribute : EventAttribute
		{
			public GraffitiSpotTamperedAttribute() : base( GraffitiSpotTampered )
			{
			}
		}

		/// <summary>
		/// <para>Invoked once a team has completed a <see cref="GraffitiArea"/>.
		/// This event is invoked on the server.</para>
		/// <para>Parameter 0 is the team that completed the <see cref="GraffitiArea"/> (type of <see cref="Team"/>).</para>
		/// <para>Parameter 1 is the player that completed the <see cref="GraffitiArea"/> (type of <see cref="Player"/>).</para>
		/// </summary>
		public class GraffitiSpotCompletedAttribute : EventAttribute
		{
			public GraffitiSpotCompletedAttribute() : base( GraffitiSpotCompleted )
			{
			}
		}
	}
}
