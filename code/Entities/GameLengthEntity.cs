namespace SpeedPainters.Entities;

/// <summary>
/// A basic entity that will override the <see cref="ParkourPainters.GameLength"/> value.
/// </summary>
[Title( "Game Time" ), Category( "Speed Painters" )]
[HammerEntity]
internal sealed partial class GameLengthEntity : Entity
{
	/// <summary>
	/// The only instance of this entity in existance.
	/// </summary>
	internal static GameLengthEntity Instance { get; private set; }

	/// <summary>
	/// The time in seconds that the game will last for.
	/// </summary>
	[Property, Net] internal float GameLength { get; private set; } = 150;

	public GameLengthEntity()
	{
		Instance = this;
	}
}
