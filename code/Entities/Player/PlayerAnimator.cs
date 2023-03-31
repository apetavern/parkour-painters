namespace ParkourPainters.Entities;

/// <summary>
/// Contains logic for animating a players model.
/// </summary>
internal sealed class PlayerAnimator : EntityComponent<Player>, ISingletonComponent
{
	/// <summary>
	/// Simulates the animator.
	/// </summary>
	/// <param name="cl">The client that is simulating the animator.</param>
	internal void Simulate( IClient cl )
	{
		var player = Entity;
		var controller = player.Controller;
		var animHelper = new CustomAnimationHelper( player )
		{
			AimAngle = player.EyeRotation,
			DazedState = player.DazeType,
			FootShuffle = 0f,
			IsGrounded = controller.GroundEntity != null,
			IsSwimming = player.GetWaterLevel() >= 0.5f,
			IsWeaponLowered = false,
			SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.None,
			VoiceLevel = (Game.IsClient && cl.IsValid()) ? cl.Voice.LastHeard < 0.5f ? cl.Voice.CurrentLevel : 0.0f : 0.0f,
		};
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, player.Tags.Has( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );

		animHelper.WithWishVelocity( player.IsDazed ? Vector3.Zero : controller.GetWishVelocity() );
		animHelper.WithVelocity( player.IsDazed ? Vector3.Zero : controller.Velocity );

		if ( Math.Abs( Vector3.Dot( player.EyePosition, player.EyeRotation.Forward ) ) > 50 && controller.Velocity.IsNearlyZero( 10 ) )
		{
			animHelper.WithLookAt( player.EyePosition + player.EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		}
		else
		{
			animHelper.WithLookAt( player.EyePosition + player.Rotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		}

		if ( player.LedgeGrabMechanic.IsActive )
		{
			// Ledge grab sets velocity to be zero, so let's use wish velocity here instead. 
			animHelper.WithVelocity( controller.GetWishVelocity( true ) );

			var kneePos = player.EyePosition + Vector3.Down * 64;
			var kneeTrace = Trace.Sphere( 4f, kneePos, kneePos + player.Rotation.Forward * 30f )
				.Ignore( player )
				.WithoutTags( "player" )
				.WithTag( "solid" )
				.Run();

			if ( kneeTrace.Hit )
				animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.LedgeGrab;
			else
				animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.LedgeGrabDangle;
		}
		else if ( player.WallJumpMechanic.IsActive )
		{
			animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.WallSlide;
		}
		else if ( player.GrindMechanic.IsActive )
		{
			animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.Grind;
		}
	}

	/// <summary>
	/// Sets velocity animation to default on the player.
	/// </summary>
	internal void Reset()
	{
		var animHelper = new CustomAnimationHelper( Entity );
		animHelper.WithWishVelocity( Vector3.Zero );
		animHelper.WithVelocity( Vector3.Zero );
	}
}
