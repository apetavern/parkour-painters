﻿namespace ParkourPainters.Entities;

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
	/// The amount of spray left in the spray can.
	/// </summary>
	[Net, Predicted] public int Ammo { get; internal set; } = MaxAmmo;

	/// <summary>
	/// The spray particles that come out when using the can.
	/// </summary>
	private Particles SprayParticles { get; set; }

	/// <summary>
	/// The maximum amount of spray that can be held in the spray can.
	/// </summary>
	internal const int MaxAmmo = 500;

	/// <inheritdoc/>
	public sealed override void OnEquipped()
	{
		base.OnEquipped();

		Owner.SetAnimParameter( "b_haspaint", true );
	}

	/// <inheritdoc/>
	public sealed override void OnHolstered()
	{
		base.OnHolstered();

		Owner.SetAnimParameter( "b_haspaint", false );
		if ( Game.IsServer )
			HolsterToHip();
	}

	/// <inheritdoc/>
	protected sealed override void OnPrimaryAttack()
	{
		if ( Owner.IsDazed || Ammo <= 0 )
		{
			if ( !HasReleasedPrimary )
			{
				HasReleasedPrimary = true;
				OnPrimaryReleased();
			}

			return;
		}

		base.OnPrimaryAttack();

		Ammo--;
		Owner.SetAnimParameter( "b_spray", true );

		// Create spray particles
		if ( SprayParticles is null )
		{
			SprayParticles = Particles.Create( "particles/paint/spray_base.vpcf", this, "nozzle" );

			if ( Owner.Team?.Group?.SprayColor is not null )
				SprayParticles.SetPosition( 1, Owner.Team.Group.SprayColor.ToVector3() );
		}

		var nozzleTransform = GetAttachment( "nozzle" );

		var reachTrace = Trace.Ray( nozzleTransform.Value.Position - nozzleTransform.Value.Rotation.Forward * 20f, nozzleTransform.Value.Position + nozzleTransform.Value.Rotation.Forward * 200f )
			.WithAnyTags( "graffiti_spot", "player" )
			.Ignore( this )
			.Ignore( Owner )
			.Run();

		if ( reachTrace.Entity is GraffitiSpot graffitiSpot )
			graffitiSpot.OnSprayReceived( Owner );
		else if ( reachTrace.Entity is Player player )
			player.Daze( Owner, DazeType.Inhalation );
	}

	/// <inheritdoc/>
	protected sealed override void OnPrimaryReleased()
	{
		base.OnPrimaryReleased();

		Owner.SetAnimParameter( "b_spray", false );

		SprayParticles?.Destroy();
		SprayParticles = null;
	}
}
