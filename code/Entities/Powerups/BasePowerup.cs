namespace ParkourPainters.Entities;

/// <summary>
/// The base class for any powerup component.
/// </summary>
internal partial class BasePowerup : EntityComponent<Player>, ISingletonComponent
{
	/// <summary>
	/// The google font icon that will be displayed in the UI.
	/// </summary>
	internal virtual string Icon => string.Empty;

	/// <summary>
	/// The time in seconds till the power up will expire.
	/// </summary>
	internal virtual float ExpiryTime => float.MaxValue;

	/// <summary>
	/// The time in seconds since the power up was added to the player.
	/// </summary>
	[Net] internal TimeSince TimeSinceAdded { get; private set; }

	/// <inheritdoc/>
	protected override void OnActivate()
	{
		base.OnActivate();

		TimeSinceAdded = 0;
	}
}
