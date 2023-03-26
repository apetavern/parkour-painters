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
			Log.Warning( $"\"{stateName}\" is not a game state" );
			return;
		}

		Current.SetState( type.Create<IGameState>() );
		Log.Info( $"Switched to \"{stateName}\" state" );
	}
}
