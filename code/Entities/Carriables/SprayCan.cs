namespace GangJam;

public partial class SprayCan : BaseCarriable
{
	protected override string ModelPath => "models/entities/spray_paint/spray_paint.vmdl";
	protected override float PrimaryFireRate => 0.05f;

	public override void OnEquipped( Player player )
	{
		base.OnEquipped( player );

		player.SetAnimParameter( "b_haspaint", true );
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		// Play spray animation if we're spraying.
		Player.SetAnimParameter( "b_spray", Input.Down( InputButton.PrimaryAttack ) );
	}

	protected override void OnPrimaryAttack()
	{
		base.OnPrimaryAttack();

		var reachTrace = Trace.Ray( Player.EyePosition, Player.EyePosition + Player.EyeRotation.Forward * 200f ).WithTag( "graffiti_spot" ).Run();

		if ( reachTrace.Entity is GraffitiSpot graffitiSpot )
			graffitiSpot.OnSprayReceived( Player );
	}
}
