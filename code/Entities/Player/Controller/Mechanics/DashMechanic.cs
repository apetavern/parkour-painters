namespace ParkourPainters.Entities;

public sealed partial class DashMechanic : ControllerMechanic
{
	[Net, Predicted] private TimeSince TimeSinceLastDash { get; set; }
	public double ActiveDashPercentage => Math.Ceiling( Math.Clamp( TimeSinceLastDash / DashRechargeTime * 100, 0, 100 ) );
	private int DashRechargeTime => ParkourPainters.InfiniteDash ? 0 : Player.CurrentPowerup is DashPowerup powerup ? powerup.RechargeTime : 3;

	protected override bool ShouldStart()
	{
		if ( Player.IsDazed )
			return false;

		if ( TimeSinceLastDash <= DashRechargeTime )
			return false;

		if ( !Input.Pressed( InputButton.Run ) )
			return false;

		if ( Player.WallJumpMechanic.IsActive || Player.LedgeGrabMechanic.IsActive || Player.GrindMechanic.IsActive )
			return false;

		return true;
	}

	protected override void OnStart()
	{
		Controller.GetMechanic<WalkMechanic>().ClearGroundEntity();

		float flAirFactor = 1.3f;
		float flMul = 150f * 1.2f;
		float forMul = 150f * 2.2f;

		var direction = Player.Rotation.Forward.Normal;

		var particles = Particles.Create( "particles/dash/dash_base.vpcf", Player );
		particles.SetEntityBone( 0, Player, Player.GetBoneIndex( "spine_0" ) );
		particles.SetOrientation( 1, Player.Rotation );

		Controller.Velocity = direction * forMul * flAirFactor;
		Controller.Velocity = Controller.Velocity.WithZ( flMul * flAirFactor );
		Controller.Velocity -= new Vector3( 0, 0, 800f * 0.5f ) * Time.Delta;

		Player.PlaySound( "dash" );

		TimeSinceLastDash = 0;
	}

	[Event.Tick.Client]
	private void DebugDraw()
	{
		if ( !ParkourPainters.DebugMode )
			return;

		DebugOverlay.ScreenText( $"Dash: {ActiveDashPercentage}%", 20 );
	}
}
