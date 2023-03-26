namespace GangJam;

public sealed partial class WallJumpMechanic : ControllerMechanic
{
	private float WallJumpConnectangle => 0.95f;
	private float WallJumpStrength => 400f;
	private float WallJumpKickStrength => 250f;
	private float WallJumpFriction => 500f;
	private float WallJumpTraceDistance => 25f;

	private TimeUntil _timeUntilWallJumpDisengage = Time.Now;
	private Vector3 _hitNormal;

	protected override bool ShouldStart()
	{
		if ( Controller.GroundEntity.IsValid() )
			return false;

		if ( Controller.Velocity.WithZ( 0 ).Length < 1.0f )
			return false;

		if ( Controller.GetMechanic<LedgeGrabMechanic>().IsActive )
			return false;

		var tr = Trace.Ray( Controller.Player.Position, Controller.Player.Position + Vector3.Down * 5f )
			.Ignore( Controller.Player )
			.WorldOnly()
			.Run();

		if ( tr.Hit )
			return false;

		var playerEyeNormal = Controller.Player.Rotation.Forward.WithZ( 0 ).Normal;
		var center = Controller.Position.WithZ( Controller.Position.z + 48 );
		var dest = center + (playerEyeNormal * WallJumpTraceDistance);

		var mid = Trace.Ray( center, dest )
			.Ignore( Controller.Player )
			.Run();

		if ( mid.Hit )
		{
			bool canGrab = mid.Normal.Dot( -playerEyeNormal ) > 1.0 - WallJumpConnectangle;
			if ( canGrab )
			{
				GrabWall( mid );
				return true;
			}
		}

		return false;
	}

	protected override void Simulate()
	{
		Controller.Velocity -= Controller.Velocity.Normal * WallJumpFriction * Time.Delta;

		if ( Controller.GroundEntity.IsValid() )
		{
			Cancel();
			return;
		}

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
	}

	private void GrabWall( TraceResult tr )
	{
		_hitNormal = tr.Normal;
		_timeUntilWallJumpDisengage = Time.Now + 1.5f;
	}

	private void DoWallJump()
	{
		var jumpVec = _hitNormal * WallJumpKickStrength;

		Controller.Velocity = Controller.Velocity.WithZ( WallJumpStrength );
		Controller.Velocity += jumpVec;
		Controller.Position += Controller.Velocity * Time.Delta;
		IsActive = false;
	}

	private void Cancel()
	{
		IsActive = false;
	}
}
