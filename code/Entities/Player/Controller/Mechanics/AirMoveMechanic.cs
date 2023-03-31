namespace ParkourPainters.Entities;

public sealed partial class AirMoveMechanic : ControllerMechanic
{
	public float Gravity => 800.0f;
	public float AirControl => 30.0f;
	public float AirAcceleration => 35.0f;

	protected override bool ShouldStart() => true;

	protected override void Simulate()
	{
		var ctrl = Controller;
		ctrl.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		ctrl.Velocity += new Vector3( 0, 0, ctrl.BaseVelocity.z ) * Time.Delta;
		ctrl.BaseVelocity = ctrl.BaseVelocity.WithZ( 0 );

		var groundedAtStart = GroundEntity.IsValid();
		if ( groundedAtStart )
			return;

		// The player probably wanted to wall jump.
		if ( Input.Pressed( InputButton.Jump ) && Player.WallJumpMechanic.TimeSinceLeftWall < 0.2f && !Player.WallJumpMechanic.UsedWallJump && !Player.LedgeGrabMechanic.IsActive )
			Player.WallJumpMechanic.DoWallJump();

		var wishVel = ctrl.GetWishVelocity( true );
		var wishdir = wishVel.Normal;
		var wishspeed = wishVel.Length;

		ctrl.Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );
		ctrl.Velocity += ctrl.BaseVelocity;
		ctrl.Move();
		ctrl.Velocity -= ctrl.BaseVelocity;
		ctrl.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
	}
}
