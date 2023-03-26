namespace GangJam.State;

/// <summary>
/// The state for when the main game loop has begun.
/// </summary>
[Category( "Setup" )]
internal sealed partial class PlayState : Entity, IGameState
{
	/// <summary>
	/// The active instance of <see cref="PlayState"/>. This can be null.
	/// </summary>
	internal static PlayState Instance => GangJam.Current.CurrentState as PlayState;

	/// <summary>
	/// Whether or not the game has been abandoned.
	/// </summary>
	/// <summary>
	/// Contains all of the participating teams.
	/// </summary>
	[Net] internal IList<Team> Teams { get; private set; }

	/// <summary>
	/// The time in seconds since the game started.
	/// </summary>
	[Net] internal TimeSince TimeSinceGameStarted { get; private set; }
	/// <summary>
	/// The time in seconds until the game ends.
	/// </summary>
	internal TimeUntil TimeUntilGameEnds => GangJam.GameLength - TimeSinceGameStarted;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	/// <inheritdoc/>
	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( !Game.IsServer )
			return;

		foreach ( var team in Children.OfType<Team>() )
			team.Delete();
	}

	/// <inheritdoc/>
	void IGameState.Enter( IGameState lastState )
	{
		// This is here to jump into play state for debugging.
		if ( lastState is not WaitingState waitingState )
		{
			var builders = new ImmutableArray<IClient>.Builder[GangJam.NumTeams];
			for ( var i = 0; i < builders.Length; i++ )
				builders[i] = ImmutableArray.CreateBuilder<IClient>();

			for ( var i = 0; i < Game.Clients.Count; i++ )
				builders[i % builders.Length].Add( Game.Clients.ElementAt( i ) );

			for ( var i = 0; i < builders.Length; i++ )
				Teams.Add( new Team( builders[i] ) );

			return;
		}

		for ( var i = 0; i < waitingState.Teams.Length; i++ )
		{
			var team = new Team( waitingState.Teams[i] )
			{
				Parent = this
			};
			Teams.Add( team );
		}

		TimeSinceGameStarted = 0;
	}

	/// <inheritdoc/>
	void IGameState.Exit()
	{
	}

	/// <inheritdoc/>
	void IGameState.ClientJoined( IClient cl )
	{
		// TODO: Make them a spectator?
	}

	/// <inheritdoc/>
	void IGameState.ClientDisconnected( IClient cl, NetworkDisconnectionReason reason )
	{
		// TODO: Abandon game if not a spectator?
	}

	/// <inheritdoc/>
	void IGameState.ClientTick()
	{
		DebugOverlay.ScreenText( $"Time Left: {TimeUntilGameEnds} seconds" );

		var linesUsed = 2;

		for ( var i = 0; i < Teams.Count; i++ )
		{
			DebugOverlay.ScreenText( $"Team {i + 1}:", linesUsed );
			linesUsed++;

			for ( var j = 0; j < Teams[i].Members.Count; j++ )
			{
				DebugOverlay.ScreenText( Teams[i].Members[j].Name, linesUsed );
				linesUsed++;
			}

			linesUsed++;
		}
	}

	/// <inheritdoc/>
	void IGameState.ServerTick()
	{
		if ( TimeUntilGameEnds <= 0 )
			GameOverState.SetActive();
	}

	/// <summary>
	/// Sets the <see cref="PlayState"/> as the active state in the game. This can only be invoked on the server.
	/// </summary>
	public static void SetActive()
	{
		Game.AssertServer();

		GangJam.Current.SetState<PlayState>();
	}
}
