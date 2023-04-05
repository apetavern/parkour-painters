namespace ParkourPainters.Entities;

/// <summary>
/// A spawner for a <see cref="SprayCan"/>s ammo.
/// </summary>
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

	/// <summary>
	/// The amount of ammo the spawner gives.
	/// </summary>
	[Property] private int AmmoAmount { get; set; } = SprayCan.MaxAmmo;
	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		if ( SpawnerModel is null )
			SetModel( "models/entities/spray_paint/spray_paint.vmdl" );
		else
			Model = SpawnerModel;

		EnableTouch = true;

		_ = new PickupTrigger() { Position = Position, Parent = this };
		var bobbing = Components.Create<BobbingComponent>();
		bobbing.PositionOffset = Vector3.Up * 10;
	}

	/// <inheritdoc/>
	public sealed override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is not Player player )
			return;

		player.Inventory.GetItem<SprayCan>().Ammo += AmmoAmount;
		TimeSinceLastPickup = 0f;

		if ( OneTimeUse )
			Delete();
	}
}
