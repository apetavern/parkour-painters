namespace SpeedPainters.Entities;

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
	/// A quick little description that popups on the users screen explaining the powerup.
	/// </summary>
	internal virtual string Description => string.Empty;

	/// <summary>
	/// The time in seconds till the power up will expire.
	/// </summary>
	internal virtual float ExpiryTime => float.MaxValue;

	/// <summary>
	/// The particles that indicate that a powerup is being used.
	/// </summary>
	internal Particles ActiveParticles { get; set; }

	/// <summary>
	/// The time in seconds since the power up was added to the player.
	/// </summary>
	[Net] internal TimeSince TimeSinceAdded { get; private set; }

	/// <inheritdoc/>
	protected override void OnActivate()
	{
		base.OnActivate();

		TimeSinceAdded = 0;

		if ( Game.IsServer )
		{
			ActiveParticles = Particles.Create( "particles/powerups/power_up_base.vpcf", Entity );

			if ( Entity.Team is not null )
				ActiveParticles.SetPosition( 1, Entity.Team.Group.SprayColor.ToVector3() );
		}

		Entity.PlaySound( "powerup_pickup" );

		if ( Entity.IsLocalPawn && !string.IsNullOrEmpty( Description ) )
			Game.RootPanel.AddChild( new UI.PowerupPopup() { Desc = Description } );
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();

		ActiveParticles?.Destroy();
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
