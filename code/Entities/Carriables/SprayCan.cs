namespace ParkourPainters.Entities;

/// <summary>
/// A spray can that can be used to daze players or graffiti the map.
/// </summary>
public sealed partial class SprayCan : BaseCarriable
{
	/// <inheritdoc/>
	public override string CarriableName => "Spray Can";

	/// <inheritdoc/>
	public override string SlotText => Ammo.ToString();

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

	private Sound _spraySound;
	private bool _isLooping = false;

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

		Owner.SetAnimParameter( "b_spray", false );
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
				OnPrimaryReleased();
				HasReleasedPrimary = true;
			}

			return;
		}

		base.OnPrimaryAttack();

		if ( !Prediction.FirstTime )
			return;

		ToggleSpraySound( true );

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

		var reachTrace = Trace.Ray( nozzleTransform.Value.Position - nozzleTransform.Value.Rotation.Forward * 20f, nozzleTransform.Value.Position + nozzleTransform.Value.Rotation.Forward * 80f )
			.Radius( 3f )
			.Ignore( this )
			.Ignore( Owner )
			.WithAnyTags( "graffiti_area", "player" )
			.Run();

		// Catch cases where the player is inside something.
		if ( reachTrace.StartedSolid )
			return;

		if ( reachTrace.Entity is GraffitiArea graffitiArea )
			graffitiArea.OnSprayReceived( Owner, reachTrace );
		else if ( reachTrace.Entity is Player player )
			player.Spray( Owner );
	}

	/// <inheritdoc/>
	protected sealed override void OnPrimaryReleased()
	{
		base.OnPrimaryReleased();

		Cleanup();
	}

	/// <inheritdoc/>
	protected sealed override void Cleanup()
	{
		base.Cleanup();

		Owner.SetAnimParameter( "b_spray", false );

		SprayParticles?.Destroy( true );
		SprayParticles = null;

		ToggleSpraySound( false );
	}

	private void ToggleSpraySound( bool toggle )
	{
		if ( toggle && !_isLooping )
		{
			_isLooping = true;
			_spraySound = PlaySound( "spray_loop" );
			return;
		}

		if ( !toggle )
		{
			_isLooping = false;
			_spraySound.Stop();
		}
	}
}
