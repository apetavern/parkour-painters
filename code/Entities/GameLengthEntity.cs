namespace ParkourPainters.Entities;

/// <summary>
/// A basic entity that will override the <see cref="ParkourPainters.GameLength"/> value.
/// </summary>
[Title( "Game Time" ), Category( "Parkour Painters" )]
[HammerEntity]
internal sealed class GameLengthEntity : Entity
{
	/// <summary>
	/// The only instance of this entity in existance.
	/// </summary>
	internal static GameLengthEntity Instance { get; private set; }

	/// <summary>
	/// The time in seconds that the game will last for.
	/// </summary>
	[Property] internal float GameLength { get; private set; } = 150;

	public GameLengthEntity()
	{
		Instance = this;
	}
}
