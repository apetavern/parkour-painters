namespace ParkourPainters.State;

/// <summary>
/// The state for when the players are voting for a map.
/// </summary>
[Category( "Setup" )]
internal sealed partial class MapVoteState : Entity, IGameState
{
	/// <inheritdoc/>
	public string StateName { get; set; } = "Map Vote";

	/// <summary>
	/// The active instance of <see cref="MapVoteState"/>. This can be null.
	/// </summary>
	internal static MapVoteState Instance => ParkourPainters.Current?.CurrentState as MapVoteState;

	/// <summary>
	/// The time in seconds until the game swaps maps.
	/// </summary>
	[Net] public TimeUntil TimeUntilMapSwitch { get; private set; }

	/// <summary>
	/// Client -> ident of map they voted on.
	/// </summary>
	[Net] public IDictionary<IClient, string> Votes { get; private set; }

	/// <summary>
	/// The idents of the maps that can be voted on.
	/// </summary>
	[Net] public IList<string> MapIdents { get; set; }

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		_ = FetchMapIdents();
	}

	public sealed override void ClientSpawn()
	{
		Game.RootPanel.AddChild<MapVotePanel>();
	}

	/// <inheritdoc/>
	void IGameState.Enter( IGameState lastState )
	{
		TimeUntilMapSwitch = 10;
	}

	/// <inheritdoc/>
	void IGameState.Exit()
	{

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
		if ( TimeUntilMapSwitch > 0 )
			return;

		if ( Votes.Count == 0 )
		{
			Game.ChangeLevel( ParkourPainters.DefaultMap );
			return;
		}

		Game.ChangeLevel
		(
			Votes.GroupBy( e => e.Value )
			.OrderBy( e => e.Count() )
			.Last().Key
		);
	}

	/// <summary>
	/// Sets the <see cref="MapVoteState"/> as the active state in the game. This can only be invoked on the server.
	/// </summary>
	internal static void SetActive()
	{
		Game.AssertServer();

		ParkourPainters.Current.SetState<MapVoteState>();
	}

	private async Task FetchMapIdents()
	{
		var queryResult = await Package.FindAsync( $"type:map game:{Game.Server.GameIdent.Replace( "#local", "" )}", take: 99 );
		MapIdents = queryResult.Packages.Select( ( p ) => p.FullIdent ).ToList();
	}

	[ConCmd.Server]
	public static void SetVote( string map )
	{
		if ( Instance is null )
			return;

		var client = ConsoleSystem.Caller;
		if ( Instance.Votes.ContainsKey( client ) )
			Instance.Votes[client] = map;
		else
			Instance.Votes.Add( client, map );
	}
}
