namespace GangJam.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="TeamType"/> enum.
/// </summary>
internal static class TeamTypeExtensions
{
	/// <summary>
	/// Contains all of the file names of clothing resources to put onto a <see cref="TeamType.Nerd"/> team member.
	/// </summary>
	private static readonly List<string> NerdClothing = new()
	{
	};

	/// <summary>
	/// Contains all of the file names of clothing resources to put onto a <see cref="TeamType.Punk"/> team member.
	/// </summary>
	private static readonly List<string> PunkClothing = new()
	{
		"messy_hair",
		"punk_jacket",
		"punk_jeans"
	};

	/// <summary>
	/// Gets the clothes that should be placed onto the teams members.
	/// </summary>
	/// <param name="type">The type of team to get clothes for.</param>
	/// <returns></returns>
	/// <exception cref="Exception">Thrown when receiving an invalid <see cref="TeamType"/>.</exception>
	internal static ClothingContainer GetClothes( this TeamType type )
	{
		var clothing = type switch
		{
			TeamType.Nerd => NerdClothing,
			TeamType.Punk => PunkClothing,
			// TODO: This should be an UnreachableException but S&box whitelist blows
			_ => throw new Exception( $"{nameof( TeamTypeExtensions )}.{nameof( GetClothes )}: Got {type} for a {nameof( TeamType )}" )
		};

		var container = new ClothingContainer();
		foreach ( var clothingIdent in clothing )
		{
			var item = ResourceLibrary.Get<Clothing>( "data/clothing/" + clothingIdent + ".clothing" );
			if ( item is null )
			{
				Log.Warning( $"\"{clothingIdent}\" is not a clothing resource" );
				continue;
			}
			
			container.Clothing.Add( item );
		}

		return container;
	}
}
