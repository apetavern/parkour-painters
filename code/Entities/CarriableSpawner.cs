namespace ParkoutPainters.Entities;

/// <summary>
/// A spawner for a <see cref="BaseCarriable"/>.
/// </summary>
[Library( "ent_carriable_spawner" )]
[Title( "Carriable Spawner" ), Category( "Parkour Painters" )]
[EditorModel( "models/entities/spray_paint/spray_paint.vmdl" )]
[HammerEntity]
internal sealed partial class CarriableSpawner : AnimatedEntity
{
	/// <summary>
	/// The time in seconds since an item was last picked up from the spawner.
	/// </summary>
	[Net] public TimeSince TimeSinceLastPickup { get; private set; }

	/// <summary>
	/// The name of a type that derives from <see cref="BaseCarriable"/> to spawn.
	/// </summary>
	[Property] private string CarriableType { get; set; }

	/// <summary>
	/// The model for the spawner chosen by hammer.
	/// </summary>
	[Property] private Model SpawnerModel { get; set; }

	/// <summary>
	/// The type that was found from <see ref="CarriableType"/>.
	/// </summary>
	private TypeDescription foundType;

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		if ( SpawnerModel is null )
		{
			Log.Error( $"{nameof( SpawnerModel )} was not set in hammer for {this}" );
			return;
		}

		Model = SpawnerModel;
		Scale = 2f;
		EnableTouch = true;

		_ = new PickupTrigger() { Position = Position, Parent = this };

		var carriableType = TypeLibrary.GetType( CarriableType );
		if ( carriableType is null )
		{
			Log.Error( $"The type \"{CarriableType}\" from {this} does not exist" );
			return;
		}

		if ( !carriableType.TargetType.IsAssignableTo( typeof(BaseCarriable) ) )
		{
			Log.Error( $"The type {carriableType.Name} is not assignable to {nameof( BaseCarriable )}" );
			return;
		}

		foundType = carriableType;
	}

	/// <inheritdoc/>
	public sealed override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is not Player player )
			return;

		if ( player.CanEquip( foundType ) )
			player.Equip( foundType );

		TimeSinceLastPickup = 0f;
	}
}
