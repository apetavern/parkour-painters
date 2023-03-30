namespace ParkoutPainters.Entities;

/// <summary>
/// A spray can that can be used to daze players or graffiti the map.
/// </summary>
public sealed partial class SprayCan : BaseCarriable
{
	/// <inheritdoc/>
	protected override string ModelPath => "models/entities/spray_paint/spray_paint.vmdl";
	/// <inheritdoc/>
	protected override float PrimaryFireRate => 0.05f;

	/// <summary>
	/// The spray particles that come out when using the can.
	/// </summary>
	private Particles SprayParticles { get; set; }

	/// <inheritdoc/>
	public sealed override void OnEquipped( Player player )
	{
		base.OnEquipped( player );

		player.SetAnimParameter( "b_haspaint", true );
	}

	/// <inheritdoc/>
	public sealed override void OnHolstered()
	{
		Player.SetAnimParameter( "b_haspaint", false );
		HolsterToHip();

		base.OnHolstered();
	}

	/// <inheritdoc/>
	protected sealed override void OnPrimaryAttack()
	{
		if ( Player.IsDazed )
		{
			HasReleasedPrimary = true;
			OnPrimaryReleased();
			return;
		}

		base.OnPrimaryAttack();

		Player.SetAnimParameter( "b_spray", true );

		// Create spray particles
		if ( SprayParticles is null )
		{
			SprayParticles = Particles.Create( "particles/paint/spray_base.vpcf", this, "nozzle" );

			if ( Player.Team?.Group is null )
				return;

			SprayParticles.SetPosition( 1, Player.Team.Group.SprayColor.ToVector3() );
		}

		var nozzleTransform = GetAttachment( "nozzle" );

		var reachTrace = Trace.Ray( nozzleTransform.Value.Position - nozzleTransform.Value.Rotation.Forward * 20f, nozzleTransform.Value.Position + nozzleTransform.Value.Rotation.Forward * 200f )
			.WithAnyTags( "graffiti_spot", "player" )
			.Ignore( this )
			.Ignore( Player )
			.Run();

		if ( reachTrace.Entity is GraffitiSpot graffitiSpot )
			graffitiSpot.OnSprayReceived( Player );
		else if ( reachTrace.Entity is Player player )
			player.Daze( Player, DazeType.Inhalation );
	}

	/// <inheritdoc/>
	protected sealed override void OnPrimaryReleased()
	{
		base.OnPrimaryReleased();

		Player.SetAnimParameter( "b_spray", false );

		SprayParticles?.Destroy();
		SprayParticles = null;
	}
}
