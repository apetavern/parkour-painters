namespace ParkourPainters.Entities;

public sealed partial class JumpMechanic : ControllerMechanic
{
	public override int SortOrder => 25;
	private float Gravity => 700f;

	protected override bool ShouldStart()
	{
		if ( Player.IsDazed )
			return false;

		if ( !Input.Pressed( InputButton.Jump ) )
			return false;

		if ( !Controller.GroundEntity.IsValid() )
			return false;

		return true;
	}

	protected override void OnStart()
	{
		float flGroundFactor = 1.0f;
		float flMul = 325f;
		float startz = Velocity.z;

		Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		Controller.GetMechanic<WalkMechanic>()
			.ClearGroundEntity();
	}
}
