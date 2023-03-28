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
		if ( Team is null )
			return;

		ClothingContainer?.ClearEntities();
		var (clothingContainer, tintDictionary) = Team.Group.ClothingCollection.GetContainerWithTints( GangJam.MixClientClothes ? Client : null );
		ClothingContainer = clothingContainer;

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

					ClothingContainer.Toggle( clothingItem );
					break;
				}
			}
		}

		// Dress
		ClothingContainer.DressEntity( this );

		// Tint any clothing items that need it
		foreach ( var child in Children )
		{
			if ( !child.Tags.Has( "clothes" ) )
				continue;

			if ( child is not AnimatedEntity anim )
				continue;

			if ( anim.Model is null )
				continue;

			if ( tintDictionary.TryGetValue( anim.Model.Name, out var tint ) )
				anim.RenderColor = tint;
		}
	}
}
