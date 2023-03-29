namespace GangJam.Entities;

partial class Player
{
	/// <summary>
	/// Contains the clothes that have been placed onto the pawn.
	/// </summary>
	private ClothingContainer ClothingContainer { get; set; }

	/// <summary>
	/// Sets up the pawns clothing.
	/// </summary>
	/// <param name="clothingCollection">A collection to override the default team collection.</param>
	public void SetupClothing( ClothingCollectionResource clothingCollection = null )
	{
		clothingCollection ??= Team?.Group.ClothingCollection;
		if ( clothingCollection is null )
			return;

		SetBodyGroup( "head", 0 );
		SetBodyGroup( "Chest", 0 );
		SetBodyGroup( "Legs", 0 );
		SetBodyGroup( "Hands", 0 );
		SetBodyGroup( "Feet", 0 );

		ClothingContainer?.ClearEntities();
		var (clothingContainer, tintDictionary) = clothingCollection.GetContainerWithTints( GangJam.MixClientClothes ? Client : null );
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
		foreach ( var child in Children.WithTags( "clothes" ) )
		{
			if ( child is not AnimatedEntity anim )
				continue;

			if ( anim.Model is null )
				continue;

			if ( tintDictionary.TryGetValue( anim.Model.Name, out var tint ) )
				anim.RenderColor = tint;
		}
	}
}
