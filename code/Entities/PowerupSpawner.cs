namespace ParkourPainters.Entities;

/// <summary>
/// A spawner for a <see cref="BasePowerup"/>.
/// </summary>
[Title( "Powerup Spawner" ), Category( "Parkour Painters" )]
[EditorModel( "models/entities/powerups/dash.vmdl" )]
[HammerEntity]
internal sealed partial class PowerupSpawner : AnimatedEntity
{
	/// <summary>
	/// Whether or not the <see cref="PowerupSpawner"/> can be used to retrieve a carriable.
	/// </summary>
	internal bool IsUnavailable => TimeSinceLastPickup <= UnavailableTime;

	/// <summary>
	/// The time in seconds since an item was last picked up from the spawner.
	/// </summary>
	[Net] internal TimeSince TimeSinceLastPickup { get; private set; }

	/// <summary>
	/// Whether or not this <see cref="PowerupSpawner"/> is a one time use.
	/// </summary>
	[Property] internal bool OneTimeUse { get; private set; }

	/// <summary>
	/// The time in seconds that the <see cref="PowerupSpawner"/> will not give another carriable after being used.
	/// </summary>
	[Property, Net] private float UnavailableTime { get; set; } = 5;

	/// <summary>
	/// Enum for the different powerup types
	/// </summary>
	private enum PowerupTypes
	{
		DashPowerup,
		JumpPowerup,
		ShieldPowerup,
		SpeedPowerup
	}

	/// <summary>
	/// Dropdown enum for the type of powerup this spawner spawns
	/// </summary>
	[Property] private PowerupTypes TypeOfPowerup { get; set; }

	/// <summary>
	/// The name of a type that derives from <see cref="BaseCarriable"/> or <see cref="BasePowerup"/> to spawn.
	/// </summary>
	private string TargetType => TypeOfPowerup.ToString();

	/// <summary>
	/// The model for the spawner.
	/// </summary>
	private string SpawnerModel => "models/entities/powerups/" + TypeOfPowerup.ToString().Replace( "Powerup", "" ) + ".vmdl";

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

		SetModel( SpawnerModel );

		Scale = 2f;

		EnableTouch = true;

		_ = new PickupTrigger() { Position = Position, Parent = this };
		var bobbing = Components.Create<BobbingComponent>();
		bobbing.PositionOffset = Vector3.Up * 10;

		var carriableType = TypeLibrary.GetType( TargetType );

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
			player.PlaySound( "carriable_pickup" );
		}

		TimeSinceLastPickup = 0f;

		if ( OneTimeUse )
			Delete();
	}

	/// <summary>
	/// Handles the render color of the spawner when it is (un)available.
	/// </summary>
	[GameEvent.Tick.Client]
	private void ClientTick()
	{
		RenderColor = IsUnavailable
			? RenderColor.WithAlpha( UnavailableAlpha )
			: RenderColor.WithAlpha( 1 );
	}
}
