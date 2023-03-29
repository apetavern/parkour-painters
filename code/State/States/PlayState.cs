﻿namespace GangJam.State;

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
	[Net] private IList<Team> teams { get; set; }
	/// <summary>
	/// A readonly list containing all participating teams.
	/// </summary>
	public IReadOnlyList<Team> Teams => teams as IReadOnlyList<Team>;

	/// <summary>
	/// The time in seconds since the game started.
	/// </summary>
	[Net] public TimeSince TimeSinceGameStarted { get; private set; }
	/// <summary>
	/// The time in seconds until the game ends.
	/// </summary>
	public TimeUntil TimeUntilGameEnds => GangJam.GameLength - TimeSinceGameStarted;

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	/// <inheritdoc/>
	protected sealed override void OnDestroy()
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
		ImmutableArray<ImmutableArray<IClient>> teamMembers;
		// This is here to jump into play state for debugging.
		if ( lastState is not WaitingState waitingState || waitingState.Teams == default )
		{
			var builder = ImmutableArray.CreateBuilder<ImmutableArray<IClient>>( GangJam.MaxTeams );
			for ( var i = 0; i < builder.Count; i++ )
				builder.Add( ImmutableArray.Create<IClient>() );

			for ( var i = 0; i < Game.Clients.Count; i++ )
				builder[i % builder.Count] = builder[i % builder.Count].Add( Game.Clients.ElementAt( i ) );

			teamMembers = builder.ToImmutable();
		}
		else
			teamMembers = waitingState.Teams;

		// Setup teams.
		for ( var i = 0; i < teamMembers.Length; i++ )
		{
			var randomGroup = Random.Shared.FromDictionary( GroupResource.All ).Value;
			var team = new Team( randomGroup, teamMembers[i] )
			{
				Parent = this
			};

			teams.Add( team );
		}

		// Respawn all players with their clothes.
		foreach ( var client in Game.Clients )
		{
			client.Pawn?.Delete();

			var player = PrefabLibrary.Spawn<Entities.Player>( client.GetTeam().Group.PlayerPrefab );
			client.Pawn = player;
			player.Respawn();
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
		cl.Pawn = new Spectator();
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
	}

	/// <inheritdoc/>
	void IGameState.ServerTick()
	{
		if ( TimeUntilGameEnds <= 0 )
			GameOverState.SetActive();
	}

#if DEBUG
	[Event.Tick.Client]
	private void DebugDraw()
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
#endif

	/// <summary>
	/// Sets the <see cref="PlayState"/> as the active state in the game. This can only be invoked on the server.
	/// </summary>
	internal static void SetActive()
	{
		Game.AssertServer();

		GangJam.Current.SetState<PlayState>();
	}
}
