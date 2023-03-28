namespace GangJam;

public partial class PlayerAnimator : EntityComponent<Player>, ISingletonComponent
{
	public virtual void Simulate( IClient cl )
	{
		var player = Entity;
		var controller = player.Controller;
		var animHelper = new CustomAnimationHelper( player );

		if ( player.IsDazed )
			player.SetAnimParameter( "daze_state", (int)player.DazeType );
		else
			player.SetAnimParameter( "daze_state", 0 );

		animHelper.WithWishVelocity( player.IsDazed ? Vector3.Zero : controller.GetWishVelocity() );
		animHelper.WithVelocity( player.IsDazed ? Vector3.Zero : controller.Velocity );

		if ( Math.Abs( Vector3.Dot( player.EyePosition, player.EyeRotation.Forward ) ) > 10 )
			animHelper.WithLookAt( player.EyePosition + player.EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );

		animHelper.AimAngle = player.EyeRotation;
		animHelper.FootShuffle = 0f;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, player.Tags.Has( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.VoiceLevel = (Game.IsClient && cl.IsValid()) ? cl.Voice.LastHeard < 0.5f ? cl.Voice.CurrentLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = controller.GroundEntity != null;
		animHelper.IsSwimming = player.GetWaterLevel() >= 0.5f;
		animHelper.IsWeaponLowered = false;
		animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.None;

		if ( player.LedgeGrabMechanic.IsActive )
		{
			// Ledge grab sets velocity to be zero, so let's use wish velocity here instead. 
			animHelper.WithVelocity( controller.GetWishVelocity( true ) );
			animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.LedgeGrab;
		}
		else if ( player.WallJumpMechanic.IsActive )
		{
			animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.WallSlide;
		}
	}
}
