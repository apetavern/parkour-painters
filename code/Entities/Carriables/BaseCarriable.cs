﻿namespace GangJam;

public abstract partial class BaseCarriable : AnimatedEntity
{
	protected Player Player => Owner as Player;

	protected virtual string ModelPath => "";

	protected virtual float PrimaryFireRate => 1f;

	protected virtual float SecondaryFireRate => 1f;

	protected virtual bool ContinualPrimaryFire => true;

	protected virtual bool ContinualSecondaryFire => false;

	[Net]
	protected TimeSince TimeSinceLastPrimary { get; set; }

	[Net]
	protected TimeSince TimeSinceLastSecondary { get; set; }

	[Net]
	public bool HasReleasedPrimary { get; set; }

	[Net]
	public bool HasReleasedSecondary { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( ModelPath );
	}

	public virtual void OnEquipped( Player player )
	{
		Owner = player;
		SetParent( player, true );

		HasReleasedPrimary = true;
		HasReleasedSecondary = true;
	}

	public virtual void OnHolstered()
	{
		Owner = null;
		Parent = null;
	}

	public override void Simulate( IClient client )
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

	protected virtual void OnPrimaryAttack()
	{
		if ( Player.IsDazed )
			return;

		TimeSinceLastPrimary = 0;
	}

	protected virtual void OnPrimaryReleased() { }

	protected virtual void OnSecondaryAttack()
	{
		if ( Player.IsDazed )
			return;

		TimeSinceLastSecondary = 0;
	}

	protected virtual void OnSecondaryReleased() { }
}
