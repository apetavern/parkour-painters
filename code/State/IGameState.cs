namespace ParkourPainters.State;

/// <summary>
/// Defines an entity that represents a state in the game loop.
/// </summary>
public interface IGameState : IEntity
{
	/// <summary>
	/// UI friendly name for the current state.
	/// </summary>
	public string StateName { get; set; }

	/// <summary>
	/// Invoked when entering this game state. This will only be invoked on the server.
	/// </summary>
	/// <param name="lastState">The previous state the game was in. This can be null.</param>
	void Enter( IGameState lastState );
	/// <summary>
	/// Invoked when leaving this game state. This will only be invoked on the server.
	/// </summary>
	void Exit();

	/// <summary>
	/// Invoked when a client has joined the game. This will only be invoked on the server.
	/// </summary>
	/// <param name="cl">The client that joined the game.</param>
	void ClientJoined( IClient cl );
	/// <summary>
	/// Invoked when a client has left the game. This will only be invoked on the server.
	/// </summary>
	/// <param name="cl">The client that left the game.</param>
	/// <param name="reason">The reason for the client leaving.</param>
	void ClientDisconnected( IClient cl, NetworkDisconnectionReason reason );

	/// <summary>
	/// Invoked every frame on the client-side.
	/// </summary>
	void ClientTick();
	/// <summary>
	/// Invoked every tick on the server-side.
	/// </summary>
	void ServerTick();

	/// <summary>
	/// Invoked when simulating a clients input.
	/// </summary>
	/// <param name="cl">The client that is being simulated.</param>
	void Simulate( IClient cl );
}
