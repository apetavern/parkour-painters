namespace ParkourPainters.Entities;

public sealed partial class JumpMechanic : ControllerMechanic
{
	public override int SortOrder => 25;
	public float Strength => Player.CurrentPowerup is JumpPowerup powerup ? 325f * powerup.IncreaseFactor : 325f;
	private float Gravity => 700f;

	protected override bool ShouldStart()
	{
		if ( Player.IsDazed )
			return false;

		if ( !Input.Pressed( InputAction.Jump ) )
			return false;

		if ( !Controller.GroundEntity.IsValid() )
			return false;

		return true;
	}

	protected override void OnStart()
	{
		float flGroundFactor = 1.0f;
		float startz = Velocity.z;

		Velocity = Velocity.WithZ( startz + Strength * flGroundFactor );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		Player.PlaySound( "jump" );

		Controller.GetMechanic<WalkMechanic>()
			.ClearGroundEntity();
	}
}
