namespace GangJam;

public partial class PlayerAnimator : EntityComponent<Player>, ISingletonComponent
{
	public virtual void Simulate( IClient cl )
	{
		var player = Entity;
		var controller = player.Controller;
		var animHelper = new CustomAnimationHelper( player );

		animHelper.WithWishVelocity( controller.GetWishVelocity() );
		animHelper.WithVelocity( controller.Velocity );
		animHelper.WithLookAt( player.EyePosition + player.EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = player.EyeRotation;
		animHelper.FootShuffle = 0f;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, player.Tags.Has( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.VoiceLevel = (Game.IsClient && cl.IsValid()) ? cl.Voice.LastHeard < 0.5f ? cl.Voice.CurrentLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = controller.GroundEntity != null;
		animHelper.IsSwimming = player.GetWaterLevel() >= 0.5f;
		animHelper.IsWeaponLowered = false;
		animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.None;

		if ( player.Components.Get<LedgeGrabMechanic>().IsActive )
		{
			// Ledge grab sets velocity to be zero, so let's use wish velocity here instead. 
			animHelper.WithVelocity( controller.GetWishVelocity( true ) );
			animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.LedgeGrab;
		}
		else if ( player.Components.Get<WallJumpMechanic>().IsActive )
		{
			animHelper.SpecialMovementType = CustomAnimationHelper.SpecialMovementTypes.WallSlide;
		}
	}
}
