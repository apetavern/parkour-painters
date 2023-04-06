namespace ParkourPainters.Entities;

/// <summary>
/// A spawner for a <see cref="BaseCarriable"/>.
/// </summary>
[Title( "Carriable Spawner" ), Category( "Parkour Painters" )]
[EditorModel( "models/entities/spray_paint/spray_paint.vmdl" )]
[HammerEntity]
internal sealed partial class CarriableSpawner : AnimatedEntity
{
	/// <summary>
	/// Whether or not the <see cref="CarriableSpawner"/> can be used to retrieve a carriable.
	/// </summary>
	internal bool IsUnavailable => TimeSinceLastPickup <= UnavailableTime;

	/// <summary>
	/// The time in seconds since an item was last picked up from the spawner.
	/// </summary>
	[Net] internal TimeSince TimeSinceLastPickup { get; private set; }

	/// <summary>
	/// The body group chosen randomly during spawn.
	/// </summary>
	private int ChosenBodyGroup { get; set; }

	/// <summary>
	/// Whether or not this <see cref="CarriableSpawner"/> is a one time use.
	/// </summary>
	[Property] internal bool OneTimeUse { get; private set; }

	/// <summary>
	/// The time in seconds that the <see cref="CarriableSpawner"/> will not give another carriable after being used.
	/// </summary>
	[Property] private float UnavailableTime { get; set; } = 5;

	/// <summary>
	/// The name of a type that derives from <see cref="BaseCarriable"/> or <see cref="BasePowerup"/> to spawn.
	/// </summary>
	[Property] private string TargetType { get; set; }

	/// <summary>
	/// The model for the spawner chosen by hammer.
	/// </summary>
	[Property] private Model SpawnerModel { get; set; }

	private readonly Dictionary<string, int> _bodyGroupMapping = new()
	{
		{"StunWeapon", 3},
	};

	/// <summary>
	/// The type that was found from <see ref="TargetType"/>.
	/// </summary>
	private TypeDescription foundType;

	/// <summary>
	/// The alpha component of the render color when the spawner is unavailable.
	/// </summary>
	private const float UnavailableAlpha = 0f;

	/// <inheritdoc/>
	public sealed override void Spawn()
	{
		base.Spawn();

		if ( SpawnerModel is null )
		{
			Log.Error( $"{nameof( SpawnerModel )} was not set in hammer for {this}" );
			return;
		}

		var bodyGroupCount = _bodyGroupMapping.TryGetValue( TargetType, out int value ) ? value : 0;
		ChosenBodyGroup = bodyGroupCount == 0 ? 0 : Game.Random.Int( 0, bodyGroupCount - 1 );

		Model = SpawnerModel;
		SetBodyGroup( "weapontype", ChosenBodyGroup );
		Scale = 2f;
		EnableTouch = true;

		_ = new PickupTrigger() { Position = Position, Parent = this };
		var bobbing = Components.Create<BobbingComponent>();
		bobbing.PositionOffset = Vector3.Up * 10;

		var carriableType = TypeLibrary.GetType( TargetType );

		if ( carriableType is null )
		{
			Log.Error( $"The type \"{TargetType}\" from {this} does not exist" );
			return;
		}

		if ( !carriableType.TargetType.IsAssignableTo( typeof( BasePowerup ) ) && !carriableType.TargetType.IsAssignableTo( typeof( BaseCarriable ) ) )
		{
			Log.Error( $"The type {carriableType.Name} is not assignable to {nameof( BasePowerup )} or {nameof( BaseCarriable )}" );
			return;
		}

		foundType = carriableType;
	}

	/// <inheritdoc/>
	public sealed override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( IsUnavailable || other is not Player player )
			return;

		if ( foundType.TargetType.IsAssignableTo( typeof( BasePowerup ) ) )
			player.Components.Add( TypeLibrary.Create<BasePowerup>( TargetType ) );

		if ( foundType.TargetType.IsAssignableTo( typeof( BaseCarriable ) ) )
		{
			if ( !player.Inventory.CanAddItem( foundType ) )
				return;

			player.Inventory.AddToInventory( foundType, ChosenBodyGroup );
			player.PlaySound( "carriable_pickup" );
		}

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
