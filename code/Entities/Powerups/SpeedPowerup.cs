namespace ParkourPainters.Entities;

internal partial class SpeedPowerup : BasePowerup
{
	public float IncreaseFactor = 1.5f;

	/// <inheritdoc/>
	internal override string Icon => "fast_forward";

	/// <inheritdoc/>
	internal override float ExpiryTime => 10;
}
