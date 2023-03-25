namespace GangJam.State;

/// <summary>
/// The state for when waiting for players to join the game.
/// </summary>
internal class WaitingState : Entity, IGameState
{
	/// <summary>
	/// The active instance of <see cref="WaitingState"/>. This can be null.
	/// </summary>
	internal static WaitingState Instance => GangJam.Current.CurrentState as WaitingState;

	/// <summary>
	/// Contains all of the clients that have been selected to be apart of team one.
	/// This can only be accessed on the server after the state has exited.
	/// </summary>
	internal ImmutableArray<IClient> TeamOne { get; private set; }
	/// <summary>
	/// Contains all of the clients that have been selected to be a part of team two.
	/// This can only be accessed on the server after the state has exited.
	/// </summary>
	internal ImmutableArray<IClient> TeamTwo { get; private set; }

	/// <inheritdoc/>
	void IGameState.Enter( IGameState lastState )
	{
	}

	/// <inheritdoc/>
	void IGameState.Exit()
	{
		var teamOneBuilder = ImmutableArray.CreateBuilder<IClient>();
		var teamTwoBuilder = ImmutableArray.CreateBuilder<IClient>();

		for ( var i = 0; i < Game.Clients.Count; i++ )
		{
			var cl = Game.Clients.ElementAt( i );

			if ( i % 2 != 0 )
				teamOneBuilder.Add( cl );
			else
				teamTwoBuilder.Add( cl );
		}

		TeamOne = teamOneBuilder.ToImmutable();
		TeamTwo = teamTwoBuilder.ToImmutable();
	}

	/// <inheritdoc/>
	void IGameState.ClientJoined( IClient cl )
	{
		var player = new Player();
		cl.Pawn = player;
		player.Respawn();

		MoveToSpawnpoint( player );
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
		if ( Game.Clients.Count >= 2 )
			PlayState.SetActive();
	}

	/// <summary>
	/// Moves a clients pawn to a random spawnpoint.
	/// </summary>
	/// <param name="player">The pawn to move.</param>
	private void MoveToSpawnpoint( Player player )
	{
		var spawnpoints = Entity.All.OfType<SpawnPoint>();
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();
		if ( randomSpawnPoint is null )
			return;

		var tx = randomSpawnPoint.Transform;
		tx.Position += Vector3.Up * 50.0f; // raise it up
		player.Transform = tx;
	}

	/// <summary>
	/// Sets the <see cref="WaitingState"/> as the active state in the game.
	/// </summary>
	public static void SetActive()
	{
		GangJam.Current.SetState<WaitingState>();
	}
}
