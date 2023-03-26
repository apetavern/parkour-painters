namespace GangJam;

partial class GangJam
{
	/// <summary>
	/// Defines how long games will be.
	/// </summary>
	[ConVar.Replicated( "gj_gamelength" )]
	internal static float GameLength { get; private set; } = 300;
}
