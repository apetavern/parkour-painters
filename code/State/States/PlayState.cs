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

	/// <summary>
	/// A cache for mapping clients to their teams.
	/// </summary>
	private readonly Dictionary<IClient, Team> clientTeamMap = new();

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

		clientTeamMap.Clear();

		if ( !Game.IsServer )
			return;

		// Delete all teams that are no longer in use.
		foreach ( var team in Children.OfType<Team>() )
			team.Delete();
	}

	/// <summary>
	/// Gets the team that a client is playing for.
	/// </summary>
	/// <param name="client">The client whose team to look for.</param>
	/// <returns>The team that the client is playing for.</returns>
	internal Team GetTeamFor( IClient client )
	{
		if ( clientTeamMap.TryGetValue( client, out var cachedTeam ) )
			return cachedTeam;

		var team = Teams.Where( team => team.Members.Contains( client ) ).FirstOrDefault();
		clientTeamMap.Add( client, team );

		return team;
	}

	/// <inheritdoc/>
	void IGameState.Enter( IGameState lastState )
	{
		ImmutableArray<ImmutableArray<IClient>> teamMembers;
		ImmutableArray<IClient> spectators;
		
		if ( lastState is WaitingState waitingState && waitingState.Teams != default )
		{
			teamMembers = waitingState.Teams;
			spectators = waitingState.Spectators;
		}
		// This is here to jump into play state for debugging.
		else
			(teamMembers, spectators) = WaitingState.BuildDefaultTeams();

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
			if ( spectators.Contains( client ) )
				continue;

			client.Pawn?.Delete();

			var player = PrefabLibrary.Spawn<Entities.Player>( client.GetTeam().Group.PlayerPrefab );
			client.Pawn = player;
			player.Respawn();
		}

		// Create all spectators.
		foreach ( var spectator in spectators )
		{
			spectator.Pawn?.Delete();
			spectator.Pawn = new Spectator();
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
