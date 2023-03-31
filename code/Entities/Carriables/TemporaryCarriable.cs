namespace ParkourPainters.Entities;

/// <summary>
/// The base for any limited time use item.
/// </summary>
internal partial class TemporaryCarriable : BaseCarriable
{
	/// <summary>
	/// The time in seconds until the powerup is automatically removed.
	/// </summary>
	public virtual float ExpiryTime => float.MaxValue;

	/// <summary>
	/// The time in seconds since the power up was spawned.
	/// </summary>
	[Net] protected TimeSince TimeSinceSpawned { get; set; }

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		TimeSinceSpawned = 0;
	}

	/// <inheritdoc/>
	public override void Simulate( IClient client )
	{
		if ( TimeSinceSpawned >= ExpiryTime )
		{
			if ( Game.IsServer )
				Owner.Inventory.RemoveFromInventory( this );

			return;
		}

		base.Simulate( client );
	}
}
