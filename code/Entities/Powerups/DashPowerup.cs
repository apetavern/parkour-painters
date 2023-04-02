namespace ParkourPainters.Entities;

internal partial class DashPowerup : BasePowerup
{
	public int RechargeTime = 1;

	/// <inheritdoc/>
	internal override string Icon => "fast_forward";

	/// <inheritdoc/>
	internal override string Description => "Faster dash recharge!";

	/// <inheritdoc/>
	internal override float ExpiryTime => 10;
}
