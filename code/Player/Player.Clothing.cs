namespace GangJam;

public partial class Player
{
	public ClothingContainer ClothingContainer { get; protected set; }

	/// <summary>
	/// Set the clothes to whatever the player is wearing
	/// </summary>
	public void SetupClothing()
	{
		ClothingContainer = new();

		var clothes = ResourceLibrary.GetAll<Clothing>();


		var PlayerClothes = new ClothingContainer();
		PlayerClothes.LoadFromClient( Client );
		var Skin = PlayerClothes.Clothing.Where( x => x.Category == Clothing.ClothingCategory.Skin ).First();
		var SkinName = Skin.Title + "_pixelated";

		foreach ( var clothing in clothes )
		{
			if ( PunkClothing.Any( clothing.Title.Contains ) )
				ClothingContainer.Clothing.Add( clothing );

			if ( clothing.Title.Contains( SkinName ) )
				ClothingContainer.Clothing.Add( clothing );
		}

		ClothingContainer.DressEntity( this );
	}

	private static readonly List<string> PunkClothing = new()
	{
		"Messy Hair",
		"Punk Jacket",
		"Punk Jeans",
	};
}
