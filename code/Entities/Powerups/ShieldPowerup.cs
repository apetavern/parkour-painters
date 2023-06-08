namespace SpeedPainters.Entities;

internal partial class ShieldPowerup : BasePowerup
{
	/// <inheritdoc/>
	internal override string Icon => "security";

	/// <inheritdoc/>
	internal override string Description => "Protective shield!";

	/// <inheritdoc/>
	internal override float ExpiryTime => 10;
}
