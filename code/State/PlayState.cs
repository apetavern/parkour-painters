namespace GangJam.State;

/// <summary>
/// The state for when the main game loop has begun.
/// </summary>
internal partial class PlayState : Entity, IGameState
{
	/// <summary>
	/// The active instance of <see cref="PlayState"/>. This can be null.
	/// </summary>
	internal static PlayState Instance => GangJam.Current.CurrentState as PlayState;
	
	/// <summary>
	/// Contains all of the clients in team one.
	/// </summary>
	[Net] internal IList<IClient> TeamOneClients { get; private set; }
	/// <summary>
	/// Contains all of the clients in team two.
	/// </summary>
	[Net] internal IList<IClient> TeamTwoClients { get; private set; }

	/// <summary>
	/// Team ones current score in the game.
	/// </summary>
	[Net] internal int TeamOneScore { get; private set; }
	/// <summary>
	/// Team twos current score in the game.
	/// </summary>
	[Net] internal int TeamTwoScore { get; private set; }

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
	void IGameState.Enter( IGameState lastState )
	{
		// This is here to jump into play state for debugging.
		if ( lastState is not WaitingState waitingState )
		{
			for ( var i = 0; i < Game.Clients.Count; i++ )
			{
				var cl = Game.Clients.ElementAt( i );

				if ( i % 2 != 0 )
					TeamOneClients.Add( cl );
				else
					TeamTwoClients.Add( cl );
			}

			return;
		}

		for ( var i = 0; i < waitingState.TeamOne.Length; i++ )
			TeamOneClients.Add( waitingState.TeamOne[i] );

		for ( var i = 0; i < waitingState.TeamTwo.Length; i++ )
			TeamTwoClients.Add( waitingState.TeamTwo[i] );

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

		var teamOneStart = 2;
		DebugOverlay.ScreenText( "Team One:", teamOneStart );

		int i;
		for ( i = 0; i < TeamOneClients.Count; i++ )
			DebugOverlay.ScreenText( TeamOneClients[i].Name, teamOneStart +  i + 1 );

		var teamTwoStart = i + 2;
		DebugOverlay.ScreenText( "Team Two:", teamTwoStart );

		for ( var j = 0; j < TeamTwoClients.Count; j++ )
			DebugOverlay.ScreenText( TeamTwoClients[j].Name, teamTwoStart + j + 1 );	
	}

	/// <inheritdoc/>
	void IGameState.ServerTick()
	{
		// TODO: Game over state.
		if ( TimeUntilGameEnds <= 0 )
			WaitingState.SetActive();
	}

	/// <summary>
	/// Sets the <see cref="PlayState"/> as the active state in the game. This can only be called on the server.
	/// </summary>
	public static void SetActive()
	{
		Game.AssertServer();

		GangJam.Current.SetState<PlayState>();
	}
}
