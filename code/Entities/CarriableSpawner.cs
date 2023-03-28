namespace GangJam;

/// <summary>
/// A spawner for a <see cref="BaseCarriable"/>.
/// </summary>
[Library( "ent_carriable_spawner" )]
[Title( "Carriable Spawner" ), Category( "Spray Down" )]
[EditorModel( "models/entities/spray_paint/spray_paint.vmdl" )]
[HammerEntity]
public sealed partial class CarriableSpawner : AnimatedEntity
{
	/// <summary>
	/// The time in seconds since an item was last picked up from the spawner.
	/// </summary>
	[Net] public TimeSince TimeSinceLastPickup { get; private set; }

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		SetModel( "models/entities/spray_paint/spray_paint.vmdl" );
		Scale = 2f;
		EnableTouch = true;

		_ = new PickupTrigger() { Position = Position, Parent = this };
	}

	/// <inheritdoc/>
	public sealed override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is not Player player )
			return;

		if ( player.CanEquip( typeof( SprayCan ) ) )
			player.Equip( new SprayCan() );

		TimeSinceLastPickup = 0f;
	}
}
