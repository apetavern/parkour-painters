namespace GangJam;

partial class Player
{
	/// <summary>
	/// Contains the clothes that have been placed onto the pawn.
	/// </summary>
	private ClothingContainer ClothingContainer { get; set; }

	/// <summary>
	/// Sets up the pawns clothing.
	/// </summary>
	public void SetupClothing()
	{
		ClothingContainer?.ClearEntities();
		ClothingContainer = Team.GetClothes();

		// Find correct skin.
		{
			var preferredClothing = new ClothingContainer();
			preferredClothing.LoadFromClient( Client );

			var skin = preferredClothing.Clothing.Where( x => x.Category == Clothing.ClothingCategory.Skin ).FirstOrDefault();
			if ( skin is not null )
			{
				var skinName = skin.Title + "_pixelated";
				foreach ( var clothingItem in ResourceLibrary.GetAll<Clothing>() )
				{
					if ( !clothingItem.Title.Contains( skinName ) )
						continue;

					ClothingContainer.Clothing.Add( clothingItem );
					break;
				}
			}
		}

		ClothingContainer.DressEntity( this );
	}
}
