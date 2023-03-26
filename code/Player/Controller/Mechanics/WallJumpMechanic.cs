namespace GangJam;

public sealed partial class WallJumpMechanic : ControllerMechanic
{
	private float WallJumpConnectangle => 0.75f;
	private float WallJumpStrength => 400f;
	private float WallJumpKickStrength => 250f;
	private float WallJumpFriction => -70f;
	private float WallJumpTraceDistance => 25f;

	private TimeUntil _timeUntilNextWallJump = Time.Now;
	private TimeUntil _timeUntilWallJumpDisengage = Time.Now;
	private Vector3 _hitNormal;

	protected override bool ShouldStart()
	{
		if ( Controller.GroundEntity.IsValid() )
			return false;

		if ( Controller.Velocity.z >= 0 )
			return false;

		if ( _timeUntilNextWallJump > Time.Now )
			return false;

		if ( Controller.Velocity.WithZ( 0 ).Length < 1.0f )
			return false;

		var playerEyeNormal = Controller.Player.Rotation.Forward.WithZ( 0 ).Normal;
		var center = Controller.Position.WithZ( Controller.Position.z + 48 );
		var dest = center + (playerEyeNormal * WallJumpTraceDistance);

		var tr = Trace.Ray( center, dest )
			.Ignore( Controller.Player )
			.Run();

		if ( tr.Hit )
		{
			bool canGrab = tr.Normal.Dot( -playerEyeNormal ) > 1.0 - WallJumpConnectangle;
			if ( canGrab )
			{
				GrabWall( tr );
				return true;
			}
		}

		return false;
	}

	protected override void Simulate()
	{
		base.Simulate();

		if ( Input.Pressed( InputButton.Jump ) )
		{
			DoWallJump();
			return;
		}

		if ( Input.Pressed( InputButton.Duck ) )
		{
			Cancel();
			return;
		}

		if ( _timeUntilWallJumpDisengage < Time.Now )
		{
			Cancel();
			return;
		}

		Controller.Velocity = Controller.Velocity.WithZ( WallJumpFriction );

		if ( Controller.GroundEntity.IsValid() )
		{
			Cancel();
			return;
		}
	}

	private void GrabWall( TraceResult tr )
	{
		_hitNormal = tr.Normal;
		_timeUntilWallJumpDisengage = Time.Now + 1.5f;
	}

	private void DoWallJump()
	{
		var jumpVec = _hitNormal * WallJumpKickStrength;

		_timeUntilNextWallJump = Time.Now + 0.25f;

		Controller.Velocity = Controller.Velocity.WithZ( WallJumpStrength );
		Controller.Velocity += jumpVec;
		Controller.Position += Controller.Velocity * Time.Delta;
		IsActive = false;
	}

	private void Cancel()
	{
		IsActive = false;
		_timeUntilNextWallJump = Time.Now + 2.0f;
	}
}
