namespace GangJam.State;

/// <summary>
/// The state for when waiting for players to join the game.
/// </summary>
[Category( "Setup" )]
internal sealed class WaitingState : Entity, IGameState
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

			var defaultPlayer = new Player();
			client.Pawn = defaultPlayer;
			defaultPlayer.Respawn();
		}
	}

	/// <inheritdoc/>
	void IGameState.Exit()
	{
		var builders = new ImmutableArray<IClient>.Builder[GangJam.MaxTeams];
		for ( var i = 0; i < builders.Length; i++ )
			builders[i] = ImmutableArray.CreateBuilder<IClient>();

		for ( var i = 0; i < Game.Clients.Count; i++ )
			builders[i % builders.Length].Add( Game.Clients.ElementAt( i ) );

		var teams = ImmutableArray.CreateBuilder<ImmutableArray<IClient>>();
		for ( var i = 0; i < builders.Length; i++ )
			teams.Add( builders[i].ToImmutable() );

		Teams = teams.ToImmutable();
	}

	/// <inheritdoc/>
	void IGameState.ClientJoined( IClient cl )
	{
		var player = new Player();
		cl.Pawn = player;
		player.Respawn();
	}

	/// <inheritdoc/>
	void IGameState.ClientDisconnected( IClient cl,  NetworkDisconnectionReason reason )
	{
	}

	/// <inheritdoc/>
	void IGameState.ClientTick()
	{
	}

	/// <inheritdoc/>
	void IGameState.ServerTick()
	{
		if ( Game.Clients.Count >= GangJam.NumTeams )
			PlayState.SetActive();
	}

	/// <summary>
	/// Sets the <see cref="WaitingState"/> as the active state in the game. This can only be invoked on the server.
	/// </summary>
	internal static void SetActive()
	{
		Game.AssertServer();

		GangJam.Current.SetState<WaitingState>();
	}
}
