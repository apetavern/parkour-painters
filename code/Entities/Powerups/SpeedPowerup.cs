namespace SpeedPainters.Entities;

internal partial class SpeedPowerup : BasePowerup
{
	public float IncreaseFactor = 1.5f;

	/// <inheritdoc/>
	internal override string Icon => "directions_run";

	/// <inheritdoc/>
	internal override string Description => "Movement speed increase!";

	/// <inheritdoc/>
	internal override float ExpiryTime => 10;
}
