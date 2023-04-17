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
	public override bool IsAiming => Input.Down( InputButton.PrimaryAttack );

	/// <inheritdoc/>
	protected override string ModelPath => "models/entities/spray_paint/spray_paint.vmdl";

	/// <inheritdoc/>
	protected override float PrimaryFireRate => 0.05f;

	/// <summary>
	/// The amount of spray left in the spray can.
	/// </summary>
	[Net, Predicted] public int Ammo { get; internal set; } = MaxAmmo;

	/// <summary>
	/// The maximum amount of spray that can be held in the spray can.
	/// </summary>
	internal const int MaxAmmo = 400;

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

		Cleanup();

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

		Ammo--;
		Owner.SetAnimParameter( "b_spray", true );

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

		Owner.SetAnimParameter( "b_spray", false );
	}
}
