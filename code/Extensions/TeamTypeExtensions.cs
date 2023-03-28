using GangJam.Resources;

namespace GangJam.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="TeamType"/> enum.
/// </summary>
internal static class TeamTypeExtensions
{
	/// <summary>
	/// Returns the <see cref="ClothingCollectionResource"/> that is associtated with the provided <see cref="TeamType"/>.
	/// </summary>
	/// <param name="type">The team to grab the collection from.</param>
	/// <returns>The <see cref="ClothingCollectionResource"/> that is associated with the provided <see cref="TeamType"/>.</returns>
	internal static ClothingCollectionResource GetClothingCollection( this TeamType type )
	{
		var path = "data/clothing_collections/" + type.ToString().ToLower() + ".clothc";
		return ResourceLibrary.Get<ClothingCollectionResource>( path );
	}
}
