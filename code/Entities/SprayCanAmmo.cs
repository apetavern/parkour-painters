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
	/// Whether or not the <see cref="SprayCanAmmo"/> can be used to retrieve a carriable.
	/// </summary>
	internal bool IsUnavailable => TimeSinceLastPickup <= UnavailableTime;

	/// <summary>
	/// The time in seconds since an item was last picked up from the spawner.
	/// </summary>
	[Net] public TimeSince TimeSinceLastPickup { get; private set; }

	/// <summary>
	/// Whether or not this ammo spawner is a one time use.
	/// </summary>
	[Property] public bool OneTimeUse { get; private set; }

	/// <summary>
	/// The time in seconds that the <see cref="SprayCanAmmo"/> will not give another carriable after being used.
	/// </summary>
	[Property] private float UnavailableTime { get; set; } = 5;

	/// <summary>
	/// The model for the spawner chosen by hammer.
	/// </summary>
	[Property] private Model SpawnerModel { get; set; }

	/// <summary>
	/// The amount of ammo the spawner gives.
	/// </summary>
	[Property] private int AmmoAmount { get; set; } = 50;

	/// <summary>
	/// The alpha component of the render color when the spawner is unavailable.
	/// </summary>
	private const float UnavailableAlpha = 0f;

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

		if ( IsUnavailable || other is not Player player )
			return;

		var sprayCan = player.Inventory.GetItem<SprayCan>();
		sprayCan.Ammo = Math.Clamp( sprayCan.Ammo + AmmoAmount, 0, SprayCan.MaxAmmo );
		TimeSinceLastPickup = 0f;

		if ( OneTimeUse )
			Delete();
	}

	/// <summary>
	/// Handles the render color of the spawner when it is (un)available.
	/// </summary>
	[Event.Tick.Client]
	private void ClientTick()
	{
		RenderColor = IsUnavailable
			? RenderColor.WithAlpha( UnavailableAlpha )
			: RenderColor.WithAlpha( 1 );
	}
}
