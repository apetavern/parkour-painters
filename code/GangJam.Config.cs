﻿namespace GangJam;

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

	/// <summary>
	/// Defines whether or not a clients clothing choices should be mixed with a <see cref="ClothingCollectionResource"/>s clothes.
	/// </summary>
	[ConVar.Replicated( "gj_mixclientclothes" )]
	internal static bool MixClientClothes { get; private set; } = true;

	/// <summary>
	/// Defines whether or not a teamates can daze each other.
	/// </summary>
	[ConVar.Replicated( "gj_friendlyfire" )]
	internal static bool FriendlyFire { get; private set; } = true;

	/// <summary>
	/// Defines the amount of time that a player will be dazed for when dazed by another player.
	/// </summary>
	[ConVar.Replicated( "gj_dazetime" )]
	internal static float DazeTime { get; private set; } = 3;

	/// <summary>
	/// Defines the amount of time that a player will be immune after being dazed by another player.
	/// </summary>
	[ConVar.Replicated( "gj_immunetime" )]
	internal static float ImmuneTime { get; private set; } = 5;
}
