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

		foreach ( var clothing in clothes )
		{
			if ( PunkClothing.Any( clothing.Title.Contains ) )
				ClothingContainer.Clothing.Add( clothing );
		}

		ClothingContainer.DressEntity( this );
	}

	private static readonly List<string> PunkClothing = new()
	{
		"Mohawk",
		"Punk Jacket",
		"Punk Jeans",
	};
}
