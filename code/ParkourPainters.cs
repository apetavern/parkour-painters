namespace ParkourPainters;

public sealed partial class ParkourPainters : GameManager
{
	/// <summary>
	/// The currently active instance of <see cref="ParkourPainters"/>.
	/// </summary>
	public static new ParkourPainters Current => (ParkourPainters)GameManager.Current;

	/// <summary>
	/// The current state of the game that is running.
	/// </summary>
	[Net] internal IGameState CurrentState { get; private set; }

	/// <summary>
	/// The number of games played so far.
	/// </summary>
	[Net]
	public int GamesPlayed { get; set; } = 0;

	public ParkourPainters()
	{
		if ( Game.IsClient )
		{
			_ = new UI.Hud();
		}
		else
		{
			SetupPrecache();
			WaitingState.SetActive();
		}
	}

	/// <inheritdoc/>
	public sealed override void ClientJoined( IClient cl )
	{
		base.ClientJoined( cl );

		CurrentState?.ClientJoined( cl );

		UI.TextChat.AddInfoChatEntry( $"{cl.Name} has joined" );
	}

	/// <inheritdoc/>
	public sealed override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		CurrentState?.ClientDisconnected( cl, reason );

		UI.TextChat.AddInfoChatEntry( $"{cl.Name} has left ({reason})" );
	}

	/// <inheritdoc/>
	public sealed override void OnVoicePlayed( IClient client )
	{
		client.Voice.WantsStereo = PlayState.Instance is not null;

		if ( client.Pawn is Entities.Player player )
			player.IsSpeaking();
	}

	/// <inheritdoc/>
	public sealed override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		CurrentState?.Simulate( cl );
	}

	/// <summary>
	/// Invoked every frame on the client-side.
	/// </summary>
	[GameEvent.Tick.Client]
	private void ClientTick()
	{
		CurrentState?.ClientTick();
	}

	/// <summary>
	/// Invoked every tick on the server-side.
	/// </summary>
	[GameEvent.Tick.Server]
	private void ServerTick()
	{
		// Prevent the potential crash whenever the convar is changed.
		if ( EnforceUniqueTeams && MaxTeams > GroupResource.All.Count )
		{
			Log.Warning( $"pp_maxteams is greater than the amount of available groups ({MaxTeams} > {GroupResource.All.Count})" );
			Log.Warning( $"pp_maxteams will be set to {GroupResource.All.Count} to avoid problems" );

			MaxTeams = GroupResource.All.Count;
		}

		CurrentState?.ServerTick();
	}

	/// <summary>
	/// Sets a new active game state. This can only be invoked on the server.
	/// </summary>
	/// <typeparam name="T">The type of the game state to set as active.</typeparam>
	internal void SetState<T>() where T : IGameState, new()
	{
		Game.AssertServer();

		SetState( new T() );
	}

	/// <summary>
	/// Sets a new active game state. This can only be invoked on the server.
	/// </summary>
	/// <param name="state">An instance of the game state to set as active.</param>
	private void SetState( IGameState state )
	{
		Game.AssertServer();

		Event.Run( Events.ExitGameState, CurrentState );
		var oldState = CurrentState;
		oldState?.Exit();

		CurrentState = state;
		CurrentState.Enter( oldState );
		Event.Run( Events.EnterGameState, CurrentState, oldState );

		oldState?.Delete();
	}
}
