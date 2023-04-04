namespace ParkourPainters;

partial class ParkourPainters
{
	string[] PrecachedMaterials =
	{
		"materials/sprays/punk_anarchy_01a.vmat",
		"materials/sprays/nerds_csharp_01a.vmat",
		"materials/sprays/nineties_cup_01a.vmat",
		"materials/sprays/yakuza_kanji.vmat",
		"materials/sprays/spray_area_hatching.vmat",
	};

	/// <summary>
	/// Sets up the precache for players joining.
	/// </summary>
	private void SetupPrecache()
	{
		Game.AssertServer();

		// Precache all materials.
		foreach ( var material in PrecachedMaterials )
			Precache.Add( material );
	}
}
