using GangJam.Extensions;

namespace GangJam.State;

/// <summary>
/// The state for when the game has finished and is now displaying the result.
/// </summary>
[Category( "Setup" )]
internal sealed partial class GameOverState : Entity, IGameState
{
	/// <summary>
	/// The active instance of <see cref="GameOverState"/>. This can be null.
	/// </summary>
	internal static GameOverState Instance => GangJam.Current.CurrentState as GameOverState;

	/// <summary>
	/// The result of the game that was played.
	/// </summary>
	[Net] internal GameResult GameResult { get; private set; }

	/// The team that has won the game.
	/// If <see cref="GameResult"/> is <see cref="GameResult.Abandoned"/> or <see cref="GameResult.Draw"/>, this will be null.
	/// </summary>
	[Net] internal Team WinningTeam { get; private set; }
	/// <summary>
	/// The teams that have drawn.
	/// If <see cref="GameResult"/> is <see cref="GameResult.Abandoned"/> or <see cref="GameResult.TeamWon"/>, this will be empty.
	/// </summary>
	[Net] internal IList<Team> DrawingTeams { get; private set; }

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	/// <inheritdoc/>
	void IGameState.Enter( IGameState lastState )
	{
		// This is here to debug game over screens.
		if ( lastState is not PlayState playState )
		{
			GameResult = Random.Shared.FromEnum<GameResult>();
			return;
		}

		var highestScoreTeam = playState.Teams.OrderBy( t => t.Score ).First();
		var equalTeams = playState.Teams.Where( t => t.Score == highestScoreTeam.Score ).ToArray();

		if ( equalTeams.Length > 1 )
		{
			GameResult = GameResult.Draw;
			for ( var i = 0; i < equalTeams.Length; i++ )
			{
				equalTeams[i].Parent = this;
				DrawingTeams.Add( equalTeams[i] );
			}
		}
		else
		{
			GameResult = GameResult.TeamWon;
			highestScoreTeam.Parent = this;
			WinningTeam = highestScoreTeam;
		}
	}

	/// <inheritdoc/>
	void IGameState.Exit()
	{
		WinningTeam?.Delete();
		foreach ( var drawingTeam in DrawingTeams )
			drawingTeam.Delete();
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
	/// Sets the <see cref="GameOverState"/> as the active state in the game. This can only be invoked on the server.
	/// </summary>
	public static void SetActive()
	{
		Game.AssertServer();

		GangJam.Current.SetState<GameOverState>();
	}
}
