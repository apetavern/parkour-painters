namespace ParkourPainters.Entities;

internal partial class JumpPowerup : BasePowerup
{
	public float IncreaseFactor = 1.3f;

	/// <inheritdoc/>
	internal override string Icon => "upgrade";

	/// <inheritdoc/>
	internal override string Description => "Jump higher!";

	/// <inheritdoc/>
	internal override float ExpiryTime => 10;
}
