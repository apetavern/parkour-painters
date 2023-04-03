namespace ParkourPainters.Entities;

/// <summary>
/// The base class for all carriable items.
/// </summary>
public abstract partial class BaseCarriable : AnimatedEntity
{
	/// <summary>
	/// The player that owns the carriable.
	/// </summary>
	public new Player Owner
	{
		get => (Player)base.Owner;
		set => base.Owner = value;
	}

	/// <summary>
	/// UI friendly name for the carriable.
	/// </summary>
	public virtual string CarriableName => "";

	/// <summary>
	/// Text that will be displayed next to the name ex. Remaining ammo.
	/// </summary>
	public virtual string SlotText => "";

	/// <summary>
	/// Whether or not this carriable can be equipped and used while climbing.
	/// </summary>
	public virtual bool CanUseWhileClimbing => true;

	/// <summary>
	/// The path to the world model to use.
	/// </summary>
	protected virtual string ModelPath => "";

	/// <summary>
	/// The time in seconds between each time the carriables primary attack can be used.
	/// </summary>
	protected virtual float PrimaryFireRate => 1f;

	/// <summary>
	/// The time in seconds between each time the carriables secondary attack can be used.
	/// </summary>
	protected virtual float SecondaryFireRate => 1f;

	/// <summary>
	/// Whether or not the primary fire can be continually fired if holding the corresponding action.
	/// </summary>
	protected virtual bool ContinualPrimaryFire => true;

	/// <summary>
	/// Whether or not the secondary fire can be continually fired if holding the corresponding action.
	/// </summary>
	protected virtual bool ContinualSecondaryFire => false;

	/// <summary>
	/// The time in seconds since the primary attack was last used.
	/// </summary>
	[Net, Predicted] protected TimeSince TimeSinceLastPrimary { get; set; }

	/// <summary>
	/// The time in seconds since the secondary attack was last used.
	/// </summary>
	[Net, Predicted] protected TimeSince TimeSinceLastSecondary { get; set; }

	/// <summary>
	/// Whether or not the primary attack is released.
	/// </summary>
	[Net, Predicted] protected bool HasReleasedPrimary { get; set; }

	/// <summary>
	/// Whether or not the secondary attack is released.
	/// </summary>
	[Net, Predicted] protected bool HasReleasedSecondary { get; set; }

	/// <summary>
	/// Returns whether or not this item is holstered.
	/// </summary>
	protected bool Holstered => Owner.HeldItem != this;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( ModelPath );
	}

	/// <inheritdoc/>
	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( Holstered || Owner.LifeState == LifeState.Dead )
		{
			Cleanup();
			return;
		}

		// Primary fire
		if ( Input.Down( InputButton.PrimaryAttack ) && !Input.Down( InputButton.SecondaryAttack ) )
		{
			if ( TimeSinceLastPrimary < PrimaryFireRate )
				return;

			if ( !ContinualPrimaryFire && !HasReleasedPrimary )
				return;

			OnPrimaryAttack();

			HasReleasedPrimary = false;
		}

		if ( Input.Released( InputButton.PrimaryAttack ) )
		{
			OnPrimaryReleased();
			HasReleasedPrimary = true;
		}

		// Secondary fire
		if ( Input.Down( InputButton.SecondaryAttack ) && !Input.Down( InputButton.PrimaryAttack ) )
		{
			if ( TimeSinceLastSecondary < SecondaryFireRate )
				return;

			if ( !ContinualSecondaryFire && !HasReleasedSecondary )
				return;

			OnSecondaryAttack();

			HasReleasedSecondary = false;
		}

		if ( Input.Released( InputButton.SecondaryAttack ) )
		{
			OnSecondaryReleased();
			HasReleasedSecondary = true;
		}
	}

	/// <summary>
	/// Invoked once the <see cref="BaseCarriable"/> has been equipped by a player.
	/// </summary>
	public virtual void OnEquipped()
	{
		SetParent( Owner, true );

		HasReleasedPrimary = true;
		HasReleasedSecondary = true;
	}

	/// <summary>
	/// Invoked once the <see cref="BaseCarriable"/> has been holstered.
	/// </summary>
	public virtual void OnHolstered()
	{
		if ( !HasReleasedPrimary )
		{
			HasReleasedPrimary = true;
			OnPrimaryReleased();
		}

		if ( !HasReleasedSecondary )
		{
			HasReleasedSecondary = true;
			OnSecondaryReleased();
		}
	}

	/// <summary>
	/// Invoked once a primary attack has been requested.
	/// </summary>
	protected virtual void OnPrimaryAttack()
	{
		if ( Owner.IsDazed )
			return;

		TimeSinceLastPrimary = 0;
	}

	/// <summary>
	/// Invoked once the primary attack has finished.
	/// </summary>
	protected virtual void OnPrimaryReleased() { }

	/// <summary>
	/// Invoked once a secondary attack has been requested.
	/// </summary>
	protected virtual void OnSecondaryAttack()
	{
		if ( Owner.IsDazed )
			return;

		TimeSinceLastSecondary = 0;
	}

	/// <summary>
	/// Invoked once a secondary attack has finished.
	/// </summary>
	protected virtual void OnSecondaryReleased() { }

	/// <summary>
	/// Invoked once a cleanup of the weapon has been requested.
	/// </summary>
	protected virtual void Cleanup() { }

	/// <summary>
	/// Holsters the <see cref="BaseCarriable"/> to the holster_spraycan attachment on the player.
	/// </summary>
	protected void HolsterToHip()
	{
		Game.AssertServer();

		HolsterTo( "holster_spraycan" );
	}

	/// <summary>
	/// Holsters the <see cref="BaseCarriable"/> to the holster_weapon attachment on the player.
	/// </summary>
	protected void HolsterToBack()
	{
		Game.AssertServer();

		HolsterTo( "holster_weapon" );
	}

	/// <summary>
	/// Holsters the <see cref="BaseCarriable"/> to an attachment point on the player.
	/// </summary>
	/// <param name="attachmentPoint">The name of the attachment point.</param>
	protected void HolsterTo( string attachmentPoint )
	{
		Game.AssertServer();

		var holster = Owner.GetAttachment( attachmentPoint, true )
			?? throw new ArgumentException( $"The attachment point \"{attachmentPoint}\" does not exist", nameof( attachmentPoint ) );

		SetParent( null );

		Position = holster.Position;
		Rotation = holster.Rotation;

		SetParent( Owner, attachmentPoint );
	}
}
