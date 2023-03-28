namespace GangJam;

partial class GangJam
{
	/// <summary>
	/// A debug command for quick setting the active state in the game.
	/// </summary>
	/// <param name="stateName">The name of the type that implements <see cref="IGameState"/>.</param>
	[ConCmd.Admin( "gj_state" )]
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
	/// A debug command for forcing the callers team type and sets up their clothes.
	/// NOTE: This command will mess with gameplay elements.
	/// </summary>
	/// <param name="teamTypeName">The name of the <see cref="TeamType"/> to switch to.</param>
	[ConCmd.Admin( "gj_forceteamtype" )]
	private static void ForceTeamType( string teamTypeName )
	{
		if ( ConsoleSystem.Caller is null )
		{
			Log.Warning( "This command can only be used by a client" );
			return;
		}

		if ( !Enum.TryParse<TeamType>( teamTypeName, true, out var teamType ) )
		{
			Log.Warning( $"\"{teamTypeName}\" is not a " + nameof( TeamType ) );
			return;
		}

		if ( ConsoleSystem.Caller.Pawn is not Player player )
		{
			Log.Warning( "You do not have the correct pawn to use this command" );
			return;
		}

		player.Team = teamType;
		player.SetupClothing();
	}
}
