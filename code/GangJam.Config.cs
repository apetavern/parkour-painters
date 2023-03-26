namespace GangJam;

partial class GangJam
{
	/// <summary>
	/// Defines how long games will be.
	/// </summary>
	[ConVar.Replicated( "gj_gamelength" )]
	internal static float GameLength { get; private set; } = 300;

	/// <summary>
	/// Defines how many teams are in the game.
	/// </summary>
	[ConVar.Replicated( "gj_numteams" )]
	internal static int NumTeams { get; private set; } = 2;
}
