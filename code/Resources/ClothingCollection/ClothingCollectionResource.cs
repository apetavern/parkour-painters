﻿namespace GangJam.Resources;

/// <summary>
/// Encapsulates a set of clothing items that are alike and can be worn by a group of citizens.
/// </summary>
[GameResource( "Clothing Collection", "clothc", "Defines a group of clothing items that can be applied to a citizen" )]
public sealed class ClothingCollectionResource : GameResource
{
	public List<TintableClothingEntry> Skins { get; private set; }
	public List<TintableClothingEntry> Facials { get; private set; }
	public List<TintableClothingEntry> Hairs { get; private set; }
	public List<TintableClothingEntry> Hats { get; private set; }
	public List<TintableClothingEntry> Tops { get; private set; }
	public List<TintableClothingEntry> Gloves { get; private set; }
	public List<TintableClothingEntry> Bottoms { get; private set; }
	public List<TintableClothingEntry> Footwears { get; private set; }

	/// <summary>
	/// Returns a randomized <see cref="ClothingContainer"/> with the collections clothes applied on top of a clients if applicable.
	/// This also returns a dictionary containing all the model names that should be tinted alongside the color that was chosen.
	/// </summary>
	/// <param name="client">The client whose clothes will populate the container with at the start.</param>
	/// <returns>
	/// A randomized <see cref="ClothingContainer"/> with the collections clothes applied on top of a clients if applicable.
	/// This also returns a dictionary containing all the model names that should be tinted alongside the color that was chosen.
	/// </returns>
	public (ClothingContainer, IReadOnlyDictionary<string, Color>) GetContainerWithTints( IClient client = null )
	{
		var chosenClothes = new List<TintableClothingEntry>();
		var random = client is not null ? new Random( (int)client.SteamId ) : Random.Shared;

		void CheckList( List<TintableClothingEntry> entries )
		{
			if ( entries is null || entries.Count == 0 )
				return;

			Random rng = new Random( Time.Now.CeilToInt() );

			var chosenFromList = rng.FromList( entries );

			if ( !chosenClothes.Where( x => x.Clothing.Category == chosenFromList.Clothing.Category ).Any() )
				chosenClothes.Add( chosenFromList );

		}

		CheckList( Skins );
		CheckList( Facials );
		CheckList( Hairs );
		CheckList( Hats );
		CheckList( Tops );
		CheckList( Gloves );
		CheckList( Bottoms );
		CheckList( Footwears );

		var container = new ClothingContainer();
		var tintDictionary = new Dictionary<string, Color>();

		if ( client is not null )
			container.LoadFromClient( client );

		foreach ( var clothingItem in chosenClothes )
		{
			container.Toggle( clothingItem.Clothing );

			switch ( clothingItem.TintMode )
			{
				case TintMode.Random:
					tintDictionary.Add( clothingItem.Clothing.Model, random.Color() );
					break;
				case TintMode.RandomSelection:
					tintDictionary.Add( clothingItem.Clothing.Model, random.FromList( clothingItem.RandomSelections ) );
					break;
				case TintMode.Single:
					tintDictionary.Add( clothingItem.Clothing.Model, clothingItem.SingleSelection );
					break;
			}
		}

		return (container, tintDictionary);
	}
}
