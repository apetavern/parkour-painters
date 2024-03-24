using Sandbox;
using Sandbox.UI;

public enum GameState
{
	PreGame,
	MidGame,
	PostGame
}

public sealed class GameManager : Component
{
	public GameState State { get; set; }

	[Property] public float RemainingTime { get; set; }

	protected override void OnAwake()
	{
		State = GameState.PreGame;
		SetupGame();
	}

	public void SetupGame()
	{
		Scene.TimeScale = 0.0f;
	}

	public void StartGame()
	{
		Scene.TimeScale = 1.0f;
		State = GameState.MidGame;
	}

	public void FinishGame()
	{
		Scene.TimeScale = 0.0f;
		State = GameState.PostGame;
	}

	protected override void OnFixedUpdate()
	{
		RemainingTime -= Time.Delta;

		if ( RemainingTime < 0 )
			FinishGame();
		else return;
	}

}

