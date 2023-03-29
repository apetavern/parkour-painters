namespace GangJam;

public sealed partial class DashMechanic : ControllerMechanic
{
	public int DashRechargeTime => GangJam.InfiniteDash ? 0 : 3;
	private TimeSince _timeSinceLastDash;

	protected override bool ShouldStart()
	{
		if ( Player.IsDazed )
			return false;

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

	[Event.Tick.Client]
	private void Frame()
	{
		var dashPercent = Math.Ceiling( Math.Clamp( (_timeSinceLastDash / DashRechargeTime) * 100, 0, 100 ) );
		DebugOverlay.ScreenText( $"Dash: {dashPercent}%", 20 );
	}
}
