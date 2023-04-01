namespace ParkourPainters.Entities;

/// <summary>
/// The base class for any powerup component.
/// </summary>
internal partial class BasePowerup : EntityComponent<Player>, ISingletonComponent
{
	/// <summary>
	/// The google font icon that will be displayed in the UI.
	/// </summary>
	internal virtual string Icon => string.Empty;

	/// <summary>
	/// The time in seconds till the power up will expire.
	/// </summary>
	internal virtual float ExpiryTime => float.MaxValue;

	/// <summary>
	/// The time in seconds since the power up was added to the player.
	/// </summary>
	[Net] internal TimeSince TimeSinceAdded { get; private set; }

	/// <inheritdoc/>
	protected override void OnActivate()
	{
		base.OnActivate();

		TimeSinceAdded = 0;
	}

	/// <summary>
	/// A debug command to give yourself an ability
	/// </summary>
	[ConCmd.Admin( "pp_givepowerup" )]
	private static void GivePowerUp( string requestedPowerup )
	{
		if ( ConsoleSystem.Caller is null )
		{
			Log.Warning( "This command can only be used by players" );
			return;
		}

		if ( ConsoleSystem.Caller.Pawn is not Entities.Player player )
		{
			Log.Warning( "You do not have the correct pawn to use this command" );
			return;
		}

		var powerupType = TypeLibrary.GetType( requestedPowerup );
		if ( powerupType is null )
		{
			Log.Error( $"The type \"{powerupType}\" does not exist" );
			return;
		}

		if ( !powerupType.TargetType.IsAssignableTo( typeof( BasePowerup ) ) )
		{
			Log.Error( $"The type {powerupType.Name} is not assignable to {nameof( BasePowerup )}" );
			return;
		}

		player.Components.Add( powerupType.Create<BasePowerup>() );
	}
}
