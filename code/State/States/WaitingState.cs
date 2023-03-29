namespace GangJam.State;

/// <summary>
/// The state for when waiting for players to join the game.
/// </summary>
[Category( "Setup" )]
internal sealed partial class WaitingState : Entity, IGameState
{
	/// <summary>
	/// The active instance of <see cref="WaitingState"/>. This can be null.
	/// </summary>
	internal static WaitingState Instance => GangJam.Current.CurrentState as WaitingState;

	/// <summary>
	/// Contains all of the clients that have been selected to be apart of teams.
	/// This can only be accessed on the server after the state has exited.
	/// </summary>
	internal ImmutableArray<ImmutableArray<IClient>> Teams { get; private set; }

	/// <summary>
	/// Contains all of the clients that have been selected to be a spectator.
	/// This can only be accessed on the server after the state has exited.
	/// </summary>
	internal ImmutableArray<IClient> Spectators { get; private set; }

	/// <summary>
	/// The time in seconds until the game will move to the <see cref="PlayState"/>.
	/// </summary>
	[Net] private TimeUntil TimeUntilGameStart { get; set; }
	/// <summary>
	/// Returns whether or not the game is getting ready to start.
	/// </summary>
	internal bool GameStarting => Game.Clients.Count >= 2 && TimeUntilGameStart > 0;

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	/// <inheritdoc/>
	void IGameState.Enter( IGameState lastState )
	{
		foreach ( var client in Game.Clients )
		{
			client.Pawn?.Delete();

			var defaultPlayer = new Entities.Player();
			client.Pawn = defaultPlayer;
			defaultPlayer.Respawn();
		}
	}

	/// <inheritdoc/>
	void IGameState.Exit()
	{
		(Teams, Spectators) = BuildDefaultTeams();
	}

	/// <inheritdoc/>
	void IGameState.ClientJoined( IClient cl )
	{
		var player = new Entities.Player();
		cl.Pawn = player;
		player.Respawn();

		TimeUntilGameStart = GangJam.GameStartGracePeriod;
	}

	/// <inheritdoc/>
	void IGameState.ClientDisconnected( IClient cl,  NetworkDisconnectionReason reason )
	{
		TimeUntilGameStart = GangJam.GameStartGracePeriod;
	}

	/// <inheritdoc/>
	void IGameState.ClientTick()
	{
	}

	/// <inheritdoc/>
	void IGameState.ServerTick()
	{
		if ( Game.Clients.Count >= 2 && TimeUntilGameStart <= 0 )
			PlayState.SetActive();
	}

#if DEBUG
	[Event.Tick.Client]
	private void DebugDraw()
	{
		if ( GameStarting )
			DebugOverlay.ScreenText( $"Game starting in {Math.Ceiling( TimeUntilGameStart )} seconds" );
		else
			DebugOverlay.ScreenText( "Waiting for players" );
	}
#endif

	/// <summary>
	/// Sets the <see cref="WaitingState"/> as the active state in the game. This can only be invoked on the server.
	/// </summary>
	internal static void SetActive()
	{
		Game.AssertServer();

		GangJam.Current.SetState<WaitingState>();
	}

	/// <summary>
	/// Returns a set of teams and a spectator group built from a very basic iteration.
	/// </summary>
	/// <returns>A set of teams and a spectator group built from a very basic iteration.</returns>
	internal static (ImmutableArray<ImmutableArray<IClient>>, ImmutableArray<IClient>) BuildDefaultTeams()
	{
		var teamBuilders = new ImmutableArray<IClient>.Builder[GangJam.MaxTeams];
		var spectatorBuilder = ImmutableArray.CreateBuilder<IClient>();

		for ( var i = 0; i < teamBuilders.Length; i++ )
			teamBuilders[i] = ImmutableArray.CreateBuilder<IClient>();

		for ( var i = 0; i < Game.Clients.Count; i++ )
		{
			var client = Game.Clients.ElementAt( i );

			// If adding this client would make the team count go over the limit then move them to spectators.
			if ( teamBuilders[i % teamBuilders.Length].Count + 1 > GangJam.MaxPlayersPerTeam )
				spectatorBuilder.Add( client );
			else
				teamBuilders[i % teamBuilders.Length].Add( client );
		}

		var teams = ImmutableArray.CreateBuilder<ImmutableArray<IClient>>();
		for ( var i = 0; i < teamBuilders.Length; i++ )
			teams.Add( teamBuilders[i].ToImmutable() );

		return (teams.ToImmutable(), spectatorBuilder.ToImmutable());
	}
}
