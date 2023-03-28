namespace GangJam;

public partial class SprayCan : BaseCarriable
{
	protected override string ModelPath => "models/entities/spray_paint/spray_paint.vmdl";
	protected override float PrimaryFireRate => 0.05f;
	protected Particles SprayParticles { get; set; }

	public override void OnEquipped( Player player )
	{
		base.OnEquipped( player );

		player.SetAnimParameter( "b_haspaint", true );
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );
	}

	protected override void OnPrimaryAttack()
	{
		base.OnPrimaryAttack();

		Player.SetAnimParameter( "b_spray", true );

		// Create spray particles
		if ( Game.IsClient && SprayParticles is null )
		{
			SprayParticles = Particles.Create( "particles/paint/spray_base.vpcf", this, "nozzle" );
			SprayParticles.SetPosition( 1, Player.Team.Group.SprayColor.ToVector3() );
		}

		var reachTrace = Trace.Ray( Player.EyePosition, Player.EyePosition + Player.EyeRotation.Forward * 200f ).WithTag( "graffiti_spot" ).Run();

		if ( reachTrace.Entity is GraffitiSpot graffitiSpot )
			graffitiSpot.OnSprayReceived( Player );
	}

	protected override void OnPrimaryReleased()
	{
		base.OnPrimaryReleased();

		Player.SetAnimParameter( "b_spray", false );

		if ( Game.IsClient )
		{
			SprayParticles?.Destroy();
			SprayParticles = null;
		}
	}
}
