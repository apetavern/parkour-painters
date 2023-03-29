namespace GangJam.Entities;

/// <summary>
/// Contains logic for animating a players model.
/// </summary>
internal sealed class PlayerAnimator : EntityComponent<Player>, ISingletonComponent
{
	/// <summary>
	/// The spray particles that come out when using the can.
	/// </summary>
	private Particles SparkParticles { get; set; }

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
			animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.LedgeGrab;
		}
		else if ( player.WallJumpMechanic.IsActive )
			animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.WallSlide;
		else if ( player.GrindMechanic.IsActive )
		{
			if ( Game.IsClient && SparkParticles is null )
			{
				SparkParticles = Particles.Create( "particles/sparks/sparks_base.vpcf", player );
				SparkParticles.SetEntityBone( 0, player, player.GetBoneIndex( "ankle_L" ) );
			}
			animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.Grind;
		}
		else
		{
			if ( Game.IsClient )
			{
				SparkParticles?.Destroy();
				SparkParticles = null;
			}
		}
	}
}
