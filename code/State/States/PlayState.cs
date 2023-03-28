namespace GangJam.State;

/// <summary>
/// The state for when the main game loop has begun.
/// </summary>
[Category( "Setup" )]
public sealed partial class PlayState : Entity, IGameState
{
	/// <summary>
	/// The active instance of <see cref="PlayState"/>. This can be null.
	/// </summary>
	public static PlayState Instance => GangJam.Current.CurrentState as PlayState;

	/// <summary>
	/// Whether or not the game has been abandoned.
	/// </summary>
	[Net] internal bool Abandoned { get; private set; }

	/// <summary>
	/// Contains all of the participating teams.
	/// </summary>
	[Net] internal IList<Team> Teams { get; private set; }

	/// <summary>
	/// The time in seconds since the game started.
	/// </summary>
	[Net] public TimeSince TimeSinceGameStarted { get; private set; }
	/// <summary>
	/// The time in seconds until the game ends.
	/// </summary>
	public TimeUntil TimeUntilGameEnds => GangJam.GameLength - TimeSinceGameStarted;

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

		// Delete all teams that are no longer in use.
		foreach ( var team in Children.OfType<Team>() )
			team.Delete();
	}

	/// <inheritdoc/>
	void IGameState.Enter( IGameState lastState )
	{
		ImmutableArray<ImmutableArray<IClient>> teams;
		// This is here to jump into play state for debugging.
		if ( lastState is not WaitingState waitingState || waitingState.Teams == default )
		{
			// TODO: This can be way better.
			var builder = ImmutableArray.CreateBuilder<ImmutableArray<IClient>>( GangJam.NumTeams );
			for ( var i = 0; i < builder.Count; i++ )
				builder.Add( ImmutableArray.Create<IClient>() );

			for ( var i = 0; i < Game.Clients.Count; i++ )
				builder[i % builder.Count] = builder[i % builder.Count].Add( Game.Clients.ElementAt( i ) );

			teams = builder.ToImmutable();
		}
		else
			teams = waitingState.Teams;

		// Setup teams.
		for ( var i = 0; i < teams.Length; i++ )
		{
			var randomGroup = Random.Shared.FromDictionary( GroupResource.All ).Value;
			var team = new Team( randomGroup, teams[i] )
			{
				Parent = this
			};

			Teams.Add( team );
		}

		// Respawn all players with their clothes.
		foreach ( var client in Game.Clients )
			(client.Pawn as Player)?.Respawn();

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
		foreach ( var team in Teams )
		{
			if ( !team.Members.Contains( cl ) )
				continue;

			Abandoned = true;
			GameOverState.SetActive();
			return;
		}
	}

	/// <inheritdoc/>
	void IGameState.ClientTick()
	{
		DebugOverlay.ScreenText( $"Time Left: {TimeUntilGameEnds} seconds" );

		var linesUsed = 2;

		for ( var i = 0; i < Teams.Count; i++ )
		{
			var team = Teams[i];
			DebugOverlay.ScreenText( $"Team {i + 1} ({team.Group.Name}, Score: {team.Score}):", linesUsed );
			linesUsed++;

			for ( var j = 0; j < team.Members.Count; j++ )
			{
				DebugOverlay.ScreenText( team.Members[j].Name, linesUsed );
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
	internal static void SetActive()
	{
		Game.AssertServer();

		GangJam.Current.SetState<PlayState>();
	}
}
