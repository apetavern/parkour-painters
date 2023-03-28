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
		if ( Player.IsDazed )
		{
			OnPrimaryReleased();
			return;
		}

		base.OnPrimaryAttack();

		Player.SetAnimParameter( "b_spray", true );

		// Create spray particles
		if ( Game.IsClient && SprayParticles is null )
		{
			SprayParticles = Particles.Create( "particles/paint/spray_base.vpcf", this, "nozzle" );

			if ( Player?.Team?.Group is null )
				return;

			SprayParticles.SetPosition( 1, Player.Team.Group.SprayColor.ToVector3() );
		}

		var reachTrace = Trace.Ray( Player.EyePosition, Player.EyePosition + Player.EyeRotation.Forward * 200f )
			.WithAnyTags( "graffiti_spot", "player" )
			.Ignore( this )
			.Ignore( Player )
			.Run();

		if ( reachTrace.Entity is GraffitiSpot graffitiSpot )
			graffitiSpot.OnSprayReceived( Player );
		else if ( reachTrace.Entity is Player player )
			player.Daze( Player, DazeType.Inhalation );
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
