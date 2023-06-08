namespace ParkourPainters;

partial class ParkourPainters
{
	/// <summary>
	/// Defines how long the grace period will be before automatically starting the game.
	/// </summary>
	[ConVar.Replicated( "pp_gamestartgraceperiod" )]
	internal static float GameStartGracePeriod { get; private set; } = 10;

	/// <summary>
	/// Defines how long games will be.
	/// </summary>
	[ConVar.Replicated( "pp_gamelength" )]
	public static float GameLength { get; private set; } = 150;

	/// <summary>
	/// Defines how long until the game moves back to the <see cref="WaitingState"/>.
	/// </summary>
	[ConVar.Replicated( "pp_gameresettimer" )]
	internal static float GameResetTimer { get; private set; } = 7;

	/// <summary>
	/// Defines how many players are needed for the game to start.
	/// </summary>
	[ConVar.Replicated( "pp_minimumplayers" )]
	public static float MinimumPlayers { get; private set; } = 2;

	/// <summary>
	/// Defines the maximum amount of teams to have in the game.
	/// </summary>
	[ConVar.Replicated( "pp_maxteams" )]
	public static int MaxTeams { get; private set; } = 4;

	/// <summary>
	/// Defines whether or not each team is a unique type.
	/// </summary>
	[ConVar.Replicated( "pp_enforceuniqueteams" )]
	internal static bool EnforceUniqueTeams { get; private set; } = true;

	/// <summary>
	/// Defines the maximum amount of players that can be in a team.
	/// </summary>
	[ConVar.Replicated( "pp_maxplayersperteam" )]
	public static int MaxPlayersPerTeam { get; private set; } = 100;

	/// <summary>
	/// Defines whether or not a clients clothing choices should be mixed with a <see cref="ClothingCollectionResource"/>s clothes.
	/// </summary>
	[ConVar.Replicated( "pp_mixclientclothes" )]
	internal static bool MixClientClothes { get; private set; } = true;

	/// <summary>
	/// The default map we will swap to if nobody voted.
	/// </summary>
	[ConVar.Replicated( "pp_defaultmap" )]
	public static string DefaultMap { get; private set; } = "apetavern.pp_rooftops";

	/// <summary>
	/// The maximum number of games that can be played before map switch.
	/// </summary>
	[ConVar.Replicated( "pp_gamelimit" )]
	public static int GameLimit { get; private set; } = 2;

	/// <summary>
	/// Defines whether or not dashing can be infinitely used.
	/// </summary>
	[ConVar.Replicated( "pp_cheat_infinitedash" )]
	internal static bool InfiniteDash { get; private set; } = false;


}
