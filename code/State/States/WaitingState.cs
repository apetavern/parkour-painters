namespace SpeedPainters.State;

/// <summary>
/// The state for when waiting for players to join the game.
/// </summary>
[Category( "Setup" )]
internal sealed partial class WaitingState : Entity, IGameState
{
	/// <inheritdoc/>
	public string StateName { get; set; } = "Waiting";

	/// <summary>
	/// The active instance of <see cref="WaitingState"/>. This can be null.
	/// </summary>
	internal static WaitingState Instance => ParkourPainters.Current?.CurrentState as WaitingState;

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
	internal bool GameStarting => Game.Clients.Count >= ParkourPainters.MinimumPlayers && TimeUntilGameStart > 0;

	internal TimeSince _timeSinceLastMessage;

	// TODO: Revert back to Convar.Client.
	[ConVar.Replicated( "pp_warmupmusicenabled" )]
	public static bool EnableWarmupMusic { get; set; } = true;

	[ConVar.Client( "pp_warmupmusiclevel" )]
	public static float WarmupMusicLevel { get; set; } = 1.0f;

	private Sound WarmupMusic { get; set; }

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
			CreatePlayer( client );
		}

		TimeUntilGameStart = ParkourPainters.GameStartGracePeriod;
	}

	/// <inheritdoc/>
	void IGameState.Exit()
	{
		(Teams, Spectators) = BuildDefaultTeams();
		WarmupMusic.Stop();
	}

	/// <inheritdoc/>
	void IGameState.ClientJoined( IClient cl )
	{
		CreatePlayer( cl );

		TimeUntilGameStart = ParkourPainters.GameStartGracePeriod;
	}

	/// <inheritdoc/>
	void IGameState.ClientDisconnected( IClient cl, NetworkDisconnectionReason reason )
	{
		TimeUntilGameStart = ParkourPainters.GameStartGracePeriod;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		WarmupMusic.Stop();
	}

	/// <inheritdoc/>
	void IGameState.ClientTick()
	{
		if ( !EnableWarmupMusic )
			WarmupMusic.SetVolume( 0.0f );
		else
			WarmupMusic.SetVolume( WarmupMusicLevel );

		// if ( !WarmupMusic.IsPlaying )
		// 	WarmupMusic = Sound.FromScreen( "painters_warmup" );

		if ( !GameStarting || _timeSinceLastMessage < 1 )
			return;

		_timeSinceLastMessage = 0;
		UI.TextChat.AddInfoChatEntry( $"Starting in {Math.Round( TimeSpan.FromSeconds( TimeUntilGameStart ).TotalSeconds )}" );
	}

	/// <inheritdoc/>
	void IGameState.ServerTick()
	{
		if ( Game.Clients.Count >= 2 && TimeUntilGameStart <= 0 )
			PlayState.SetActive();
	}

	/// <summary>
	/// Creates a pawn the player can use while waiting for the game to start.
	/// </summary>
	/// <param name="client">The client that is getting this pawn.</param>
	private void CreatePlayer( IClient client )
	{
		var defaultPlayer = new Entities.Player();
		client.Pawn = defaultPlayer;

		var clothing = new ClothingContainer();
		clothing.LoadFromClient( client );
		defaultPlayer.SetupClothing( clothing );
		defaultPlayer.Respawn();
	}

	[GameEvent.Tick.Client]
	private void DebugDraw()
	{
		if ( !ParkourPainters.DebugMode )
			return;

		if ( GameStarting )
			DebugOverlay.ScreenText( $"Game starting in {Math.Ceiling( TimeUntilGameStart )} seconds" );
		else
			DebugOverlay.ScreenText( "Waiting for players" );
	}

	/// <summary>
	/// Sets the <see cref="WaitingState"/> as the active state in the game. This can only be invoked on the server.
	/// </summary>
	internal static void SetActive()
	{
		Game.AssertServer();

		ParkourPainters.Current.SetState<WaitingState>();
	}

	/// <summary>
	/// Returns a set of teams and a spectator group built from a very basic iteration.
	/// </summary>
	/// <returns>A set of teams and a spectator group built from a very basic iteration.</returns>
	internal static (ImmutableArray<ImmutableArray<IClient>>, ImmutableArray<IClient>) BuildDefaultTeams()
	{
		var teamBuilders = new ImmutableArray<IClient>.Builder[ParkourPainters.MaxTeams];
		var spectatorBuilder = ImmutableArray.CreateBuilder<IClient>();

		for ( var i = 0; i < teamBuilders.Length; i++ )
			teamBuilders[i] = ImmutableArray.CreateBuilder<IClient>();

		for ( var i = 0; i < Game.Clients.Count; i++ )
		{
			var client = Game.Clients.ElementAt( i );

			// If adding this client would make the team count go over the limit then move them to spectators.
			if ( teamBuilders[i % teamBuilders.Length].Count + 1 > ParkourPainters.MaxPlayersPerTeam )
				spectatorBuilder.Add( client );
			else
				teamBuilders[i % teamBuilders.Length].Add( client );
		}

		var teams = ImmutableArray.CreateBuilder<ImmutableArray<IClient>>();
		for ( var i = 0; i < teamBuilders.Length; i++ )
		{
			if ( teamBuilders[i].Count == 0 )
				continue;

			teams.Add( teamBuilders[i].ToImmutable() );
		}

		return (teams.ToImmutable(), spectatorBuilder.ToImmutable());
	}
}
