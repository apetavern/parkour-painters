namespace ParkourPainters.State;

/// <summary>
/// The state for when the game has finished and is now displaying the result.
/// </summary>
[Category( "Setup" )]
internal sealed partial class GameOverState : Entity, IGameState
{
	/// <inheritdoc/>
	public string StateName { get; set; } = "Game Over";

	/// <summary>
	/// The active instance of <see cref="GameOverState"/>. This can be null.
	/// </summary>
	internal static GameOverState Instance => ParkourPainters.Current?.CurrentState as GameOverState;

	/// <summary>
	/// The result of the game that was played.
	/// </summary>
	[Net] internal GameResult GameResult { get; private set; }

	/// <summary>
	/// The team that has won the game.
	/// If <see cref="GameResult"/> is <see cref="GameResult.Abandoned"/> or <see cref="GameResult.Draw"/>, this will be null.
	/// </summary>
	[Net] internal Team WinningTeam { get; private set; }
	/// <summary>
	/// The teams that have drawn.
	/// If <see cref="GameResult"/> is <see cref="GameResult.Abandoned"/> or <see cref="GameResult.TeamWon"/>, this will be empty.
	/// </summary>
	[Net] internal IList<Team> DrawingTeams { get; private set; }

	/// <summary>
	/// The time in seconds until the game moves back to the <see cref="WaitingState"/>.
	/// </summary>
	[Net] private TimeUntil TimeUntilResetGame { get; set; }

	/// <summary>
	/// A list containing all of the graffiti spots to look at.
	/// </summary>
	[Net] public IList<GraffitiArea> Spots { get; private set; }

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	/// <inheritdoc/>
	void IGameState.Enter( IGameState lastState )
	{
		// This is here to debug game over screens.
		if ( lastState is not PlayState playState )
		{
			GameResult = Random.Shared.FromEnum<GameResult>();
			return;
		}

		if ( playState.Abandoned )
		{
			GameResult = GameResult.Abandoned;
			return;
		}

		var highestScoreTeam = playState.Teams.OrderBy( t => t.Score ).First();
		var equalTeams = playState.Teams.Where( t => t.Score == highestScoreTeam.Score ).ToArray();

		if ( equalTeams.Length > 1 )
		{
			GameResult = GameResult.Draw;
			for ( var i = 0; i < equalTeams.Length; i++ )
			{
				equalTeams[i].Parent = this;
				DrawingTeams.Add( equalTeams[i] );
			}
		}
		else
		{
			GameResult = GameResult.TeamWon;
			highestScoreTeam.Parent = this;
			WinningTeam = highestScoreTeam;
		}

		foreach ( var spot in All.OfType<GraffitiArea>() )
			Spots.Add( spot );

		foreach ( var client in Game.Clients )
		{
			var startPosition = client.Position;
			client.Pawn = new GameOverSpectator()
			{
				Position = startPosition
			};
		}

		TimeUntilResetGame = (GameOverSpectator.TravelTimeToSpot + GameOverSpectator.StareTime) * Spots.Count + ParkourPainters.GameResetTimer;
	}

	/// <inheritdoc/>
	void IGameState.Exit()
	{
		Spots.Clear();

		foreach ( var player in All.WithTags( "player" ) )
			player.Delete();

		WinningTeam?.Delete();
		foreach ( var drawingTeam in DrawingTeams )
			drawingTeam.Delete();
	}

	/// <inheritdoc/>
	void IGameState.ClientJoined( IClient cl )
	{
	}

	/// <inheritdoc/>
	void IGameState.ClientDisconnected( IClient cl, NetworkDisconnectionReason reason )
	{
	}

	/// <inheritdoc/>
	void IGameState.ClientTick()
	{
	}

	/// <inheritdoc/>
	void IGameState.ServerTick()
	{
		if ( TimeUntilResetGame <= 0 )
			WaitingState.SetActive();
	}

#if DEBUG
	[Event.Tick.Client]
	private void DebugDraw()
	{
		DebugOverlay.ScreenText( $"Moving to {nameof( WaitingState )} in {Math.Ceiling( TimeUntilResetGame )} seconds" );

		DebugOverlay.ScreenText( $"Result: {GameResult}", 1 );

		if ( GameResult == GameResult.Draw )
		{
			for ( var i = 0; i < DrawingTeams.Count; i++ )
				DebugOverlay.ScreenText( DrawingTeams[i].Name, i + 2 );
		}
		else
			DebugOverlay.ScreenText( WinningTeam.Name, 2 );
	}
#endif

	/// <summary>
	/// Sets the <see cref="GameOverState"/> as the active state in the game. This can only be invoked on the server.
	/// </summary>
	internal static void SetActive()
	{
		Game.AssertServer();

		ParkourPainters.Current.SetState<GameOverState>();
	}
}
