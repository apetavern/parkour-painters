using GangJam.Extensions;

namespace GangJam.State;

/// <summary>
/// The state for when the game has finished and is now displaying the result.
/// </summary>
internal partial class GameOverState : Entity, IGameState
{
	/// <summary>
	/// The active instance of <see cref="GameOverState"/>. This can be null.
	/// </summary>
	internal static GameOverState Instance => GangJam.Current.CurrentState as GameOverState;

	/// <summary>
	/// The result of the game that was played.
	/// </summary>
	[Net] internal GameResult GameResult { get; private set; }

	/// <inheritdoc/>
	void IGameState.Enter( IGameState lastState )
	{
		// This is here to debug game over screens.
		if ( lastState is not PlayState playState )
		{
			GameResult = Random.Shared.FromEnum<GameResult>();
			return;
		}

		// Deduce the state of the game.
		if ( playState.TeamOneScore > playState.TeamTwoScore )
			GameResult = GameResult.TeamOneWon;
		else if ( playState.TeamTwoScore > playState.TeamOneScore )
			GameResult = GameResult.TeamTwoWon;
		else
			GameResult = GameResult.Draw;
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
	}

	/// <summary>
	/// Sets the <see cref="GameOverState"/> as the active state in the game. This can only be called on the server.
	/// </summary>
	public static void SetActive()
	{
		Game.AssertServer();

		GangJam.Current.SetState<GameOverState>();
	}
}
