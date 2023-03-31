namespace ParkourPainters.Entities;

/// <summary>
/// A spawner for a <see cref="SprayCan"/>s ammo.
/// </summary>
[Library( "ent_spray_can_ammo" )]
[Title( "Spray Can Ammo" ), Category( "Parkour Painters" )]
[EditorModel( "models/entities/spray_paint/spray_paint.vmdl" )]
[HammerEntity]
internal sealed partial class SprayCanAmmo : AnimatedEntity
{
	/// <summary>
	/// The time in seconds since an item was last picked up from the spawner.
	/// </summary>
	[Net] public TimeSince TimeSinceLastPickup { get; private set; }

	/// <summary>
	/// Whether or not this ammo spawner is a one time use.
	/// </summary>
	[Property] public bool OneTimeUse { get; private set; }

	/// <summary>
	/// The model for the spawner chosen by hammer.
	/// </summary>
	[Property] private Model SpawnerModel { get; set; }

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		Model = SpawnerModel;
		EnableTouch = true;

		_ = new PickupTrigger() { Position = Position, Parent = this };
	}

	/// <inheritdoc/>
	public sealed override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is not Player player )
			return;

		player.GetItem<SprayCan>().Ammo = SprayCan.MaxAmmo;
		TimeSinceLastPickup = 0f;

		if ( OneTimeUse )
			Delete();
	}
}
