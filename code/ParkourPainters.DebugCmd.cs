#if DEBUG
namespace ParkourPainters;

partial class ParkourPainters
{
	/// <summary>
	/// A debug command for quick setting the active state in the game.
	/// </summary>
	/// <param name="stateName">The name of the type that implements <see cref="IGameState"/>.</param>
	[ConCmd.Admin( "pp_state" )]
	private static void SetStateCmd( string stateName )
	{
		var type = TypeLibrary.GetType( stateName );

		if ( type is null )
		{
			Log.Warning( $"No type with the name \"{stateName}\" exists" );
			return;
		}

		if ( !type.Interfaces.Contains( typeof( IGameState ) ) )
		{
			Log.Warning( $"\"{stateName}\" is not a " + nameof(IGameState) );
			return;
		}

		Current.SetState( type.Create<IGameState>() );
		Log.Info( $"Switched to \"{stateName}\" state" );
	}

	/// <summary>
	/// A debug command to change the players clothing for testing purposes.
	/// </summary>
	/// <param name="groupName">The name of the group whose clothes to change into.</param>
	[ConCmd.Admin( "pp_wearclothes" )]
	private static void WearClothes( string groupName )
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

		var loweredGroupName = groupName.ToLower();
		GroupResource chosenGroup = null;
		foreach ( var (groupResourceName, groupResource) in GroupResource.All )
		{
			if ( loweredGroupName != groupResourceName.ToLower() )
				continue;

			chosenGroup = groupResource;
			break;
		}

		if ( chosenGroup is null )
		{
			Log.Warning( $"No group with the name \"{groupName}\" exists" );
			return;
		}

		player.SetupClothing( chosenGroup.ClothingCollection );
		Log.Info( $"Changed clothing to {chosenGroup.Name}" );
	}

	/// <summary>
	/// A debug command to become a spectator.
	/// </summary>
	[ConCmd.Admin( "pp_becomespectator" )]
	private static void BecomeSpectator()
	{
		if ( ConsoleSystem.Caller is null )
		{
			Log.Warning( "This command can only be used by players" );
			return;
		}

		if ( ConsoleSystem.Caller.Pawn is Spectator )
		{
			Log.Warning( "You are already a spectator" );
			return;
		}

		ConsoleSystem.Caller.Pawn?.Delete();
		ConsoleSystem.Caller.Pawn = new Spectator();
	}

	/// <summary>
	/// A debug command to restore the players ammo.
	/// </summary>
	[ConCmd.Admin( "pp_giveammo" )]
	private static void GiveAmmo()
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

		player.Inventory.GetItem<SprayCan>().Ammo = SprayCan.MaxAmmo;
	}
}
#endif
