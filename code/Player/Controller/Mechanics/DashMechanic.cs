namespace GangJam;

public sealed partial class DashMechanic : ControllerMechanic
{
	public int DashRechargeTime => 3;
	private TimeSince _timeSinceLastDash;

	protected override bool ShouldStart()
	{
		if ( _timeSinceLastDash <= DashRechargeTime )
			return false;

		if ( !Input.Pressed( InputButton.Run ) )
			return false;

		if ( Player.WallJumpMechanic.IsActive || Player.LedgeGrabMechanic.IsActive )
			return false;

		return true;
	}

	protected override void OnStart()
	{
		float flGroundFactor = Controller.GroundEntity.IsValid() ? 3f : 1.2f;
		float flMul = 150f * 1.2f;
		float forMul = 150f * 2.2f;

		var direction = Player.Rotation.Forward.Normal;

		Controller.Velocity = direction * forMul * flGroundFactor;
		Controller.Velocity = Controller.Velocity.WithZ( flMul * flGroundFactor );
		Controller.Velocity -= new Vector3( 0, 0, 800f * 0.5f ) * Time.Delta;

		_timeSinceLastDash = 0;
	}
}