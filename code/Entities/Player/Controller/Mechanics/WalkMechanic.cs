namespace ParkourPainters.Entities;

public sealed partial class WalkMechanic : ControllerMechanic
{
	private readonly float _wishSpeed = 275f;
	private float _stopSpeed => 150f;
	private float _stepSize => 18.0f;
	private float _groundAngle => 46.0f;
	private float _groundFriction => 4.0f;
	private float _maxNonJumpVelocity => 140.0f;
	private float _surfaceFriction { get; set; } = 1f;
	private float _acceleration => 6f;

	public override float? WishSpeed
	{
		get
		{
			if ( Player.IsDazed )
				return 0;

			var speed = _wishSpeed;
			if ( Player.IsSprayed )
				speed *= ParkourPainters.SprayedSpeedFactor;

			if ( Player.CurrentPowerup is SpeedPowerup powerup )
				speed *= powerup.IncreaseFactor;

			return speed;
		}
	}

	protected override void Simulate()
	{
		if ( GroundEntity != null )
			WalkMove();

		// Rotate the player to the direction they want to move towards.
		var wishSpeed = Controller.GetWishVelocity( true ).Normal;
		if ( wishSpeed.Length > 0 )
		{
			var targetRot = Rotation.LookAt( wishSpeed ).Angles().WithPitch( 0 ).WithRoll( 0 );


			if ( Input.Down( InputButton.PrimaryAttack ) && Player.HeldItem != null && Player.GetAnimParameterInt( "special_movement_states" ) == 0 )
			{
				Player.Rotation = Rotation.Lerp( Player.Rotation, Rotation.LookAt( Player.EyeRotation.Forward.WithZ( 0 ) ), 25f * Time.Delta );
			}
			else
			{
				Player.Rotation = Rotation.Slerp( Player.Rotation, Rotation.From( targetRot ), 8f * Time.Delta );
			}
		}
		else if ( Input.Down( InputButton.PrimaryAttack ) && Player.HeldItem != null && Player.GetAnimParameterInt( "special_movement_states" ) == 0 )
		{
			Player.Rotation = Rotation.Lerp( Player.Rotation, Rotation.LookAt( Player.EyeRotation.Forward.WithZ( 0 ) ), 25f * Time.Delta );
		}

		CategorizePosition( Controller.GroundEntity != null );
	}

	/// <summary>
	/// Try to keep a walking player on the ground when running down slopes etc.
	/// </summary>
	private void StayOnGround()
	{
		var start = Controller.Position + Vector3.Up * 2;
		var end = Controller.Position + Vector3.Down * _stepSize;

		// See how far up we can go without getting stuck
		var trace = Controller.TraceBBox( Controller.Position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = Controller.TraceBBox( start, end );

		if ( trace.Fraction <= 0 )
			return;

		if ( trace.Fraction >= 1 )
			return;

		if ( trace.StartedSolid )
			return;

		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > _groundAngle )
			return;

		Controller.Position = trace.EndPosition;
	}

	private void WalkMove()
	{
		var ctrl = Controller;

		var wishVel = ctrl.GetWishVelocity( true );
		var wishdir = wishVel.Normal;
		var wishspeed = wishVel.Length;
		var friction = _groundFriction * _surfaceFriction;

		ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
		ctrl.ApplyFriction( _stopSpeed, friction );

		var accel = _acceleration;

		ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
		ctrl.Accelerate( wishdir, wishspeed, 0, accel );
		ctrl.Velocity = ctrl.Velocity.WithZ( 0 );

		// Add in any base velocity to the current velocity.
		ctrl.Velocity += ctrl.BaseVelocity;

		try
		{
			if ( ctrl.Velocity.Length < 1.0f )
			{
				ctrl.Velocity = Vector3.Zero;
				return;
			}

			var dest = (ctrl.Position + ctrl.Velocity * Time.Delta).WithZ( ctrl.Position.z );
			var pm = ctrl.TraceBBox( ctrl.Position, dest );

			if ( pm.Fraction == 1 )
			{
				ctrl.Position = pm.EndPosition;
				StayOnGround();
				return;
			}

			ctrl.StepMove();
		}
		finally
		{
			ctrl.Velocity -= ctrl.BaseVelocity;
		}

		StayOnGround();
	}

	/// <summary>
	/// We're no longer on the ground, remove it
	/// </summary>
	public void ClearGroundEntity()
	{
		if ( GroundEntity == null )
			return;

		GroundEntity = null;
		_surfaceFriction = 1.0f;
	}

	public void SetGroundEntity( Entity entity )
	{
		GroundEntity = entity;

		if ( GroundEntity != null )
		{
			Velocity = Velocity.WithZ( 0 );
			Controller.BaseVelocity = GroundEntity.Velocity;
		}
	}

	public void CategorizePosition( bool bStayOnGround )
	{
		_surfaceFriction = 1.0f;

		var point = Position - Vector3.Up * 2;
		var vBumpOrigin = Position;
		bool bMovingUpRapidly = Velocity.z > _maxNonJumpVelocity;
		bool bMoveToEndPos = false;

		if ( GroundEntity != null )
		{
			bMoveToEndPos = true;
			point.z -= _stepSize;
		}
		else if ( bStayOnGround )
		{
			bMoveToEndPos = true;
			point.z -= _stepSize;
		}

		if ( bMovingUpRapidly )
		{
			ClearGroundEntity();
			return;
		}

		var pm = Controller.TraceBBox( vBumpOrigin, point, 4.0f );

		var angle = Vector3.GetAngle( Vector3.Up, pm.Normal );
		Controller.CurrentGroundAngle = angle;

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > _groundAngle )
		{
			ClearGroundEntity();
			bMoveToEndPos = false;

			if ( Velocity.z > 0 )
				_surfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundEntity( pm );
		}

		if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
		{
			Position = pm.EndPosition;
		}
	}

	private void UpdateGroundEntity( TraceResult tr )
	{
		_surfaceFriction = tr.Surface.Friction * 1.25f;
		if ( _surfaceFriction > 1 )
			_surfaceFriction = 1;

		SetGroundEntity( tr.Entity );
	}
}
