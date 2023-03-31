namespace ParkourPainters.Entities;

public sealed partial class DashMechanic : ControllerMechanic
{
	public double ActiveDashPercentage => Math.Ceiling( Math.Clamp( _timeSinceLastDash / DashRechargeTime * 100, 0, 100 ) );
	private int DashRechargeTime => ParkourPainters.InfiniteDash ? 0 : 3;
	private TimeSince _timeSinceLastDash { get; set; }

	protected override bool ShouldStart()
	{
		if ( Player.IsDazed )
			return false;

		if ( _timeSinceLastDash <= DashRechargeTime )
			return false;

		if ( !Input.Pressed( InputButton.Run ) )
			return false;

		if ( Player.WallJumpMechanic.IsActive || Player.LedgeGrabMechanic.IsActive || Player.GrindMechanic.IsActive )
			return false;

		return true;
	}

	protected override void OnStart()
	{
		float flGroundFactor = Controller.GroundEntity.IsValid() ? 3f : 1.2f;
		float flMul = 150f * 1.2f;
		float forMul = 150f * 2.2f;

		var direction = Player.Rotation.Forward.Normal;

		var particles = Particles.Create( "particles/dash/dash_base.vpcf", Player );
		particles.SetEntityBone( 0, Player, Player.GetBoneIndex( "spine_0" ) );
		particles.SetOrientation( 1, Player.Rotation );

		Controller.Velocity = direction * forMul * flGroundFactor;
		Controller.Velocity = Controller.Velocity.WithZ( flMul * flGroundFactor );
		Controller.Velocity -= new Vector3( 0, 0, 800f * 0.5f ) * Time.Delta;

		_timeSinceLastDash = 0;
	}

#if DEBUG
	[Event.Tick.Client]
	private void DebugDraw()
	{
		DebugOverlay.ScreenText( $"Dash: {ActiveDashPercentage}%", 20 );
	}
#endif
}
