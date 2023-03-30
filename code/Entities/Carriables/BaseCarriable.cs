namespace ParkoutPainters.Entities;

/// <summary>
/// The base class for all carriable items.
/// </summary>
public abstract partial class BaseCarriable : AnimatedEntity
{
	/// <summary>
	/// The player that owns the carriable.
	/// </summary>
	protected Player Player => Owner as Player;

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
	[Net] protected TimeSince TimeSinceLastPrimary { get; set; }

	/// <summary>
	/// The time in seconds since the secondary attack was last used.
	/// </summary>
	[Net] protected TimeSince TimeSinceLastSecondary { get; set; }

	/// <summary>
	/// Whether or not the primary attack is released.
	/// </summary>
	[Net, Predicted] protected bool HasReleasedPrimary { get; set; }
	
	/// <summary>
	/// Whether or not the secondary attack is released.
	/// </summary>
	[Net, Predicted] protected bool HasReleasedSecondary { get; set; }

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		SetModel( ModelPath );
	}

	/// <inheritdoc/>
	public sealed override void Simulate( IClient client )
	{
		base.Simulate( client );

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
	/// <param name="player">The player that is equipping the <see cref="BaseCarriable"/>.</param>
	public virtual void OnEquipped( Player player )
	{
		Owner = player;
		SetParent( player, true );

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

		Owner = null;
	}

	/// <summary>
	/// Invoked once a primary attack has been requested.
	/// </summary>
	protected virtual void OnPrimaryAttack()
	{
		if ( Player.IsDazed )
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
		if ( Player.IsDazed )
			return;

		TimeSinceLastSecondary = 0;
	}

	/// <summary>
	/// Invoked once a secondary attack has finished.
	/// </summary>
	protected virtual void OnSecondaryReleased() { }
}
