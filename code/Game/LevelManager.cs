using Sandbox;
using System.ComponentModel.DataAnnotations;

public sealed class LevelManager : Component
{
	[Property] public float RemainingTime { get; set; }

	public string GetTimeString()
	{
		var time = RemainingTime;
		var minutes = (int)time / 60;
		var seconds = (int)time % 60;
		return $"{minutes:00}:{seconds:00}";
	}

	protected override void OnFixedUpdate()
	{
		RemainingTime -= Time.Delta;
	}
}
