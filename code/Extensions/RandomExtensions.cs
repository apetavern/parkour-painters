namespace GangJam.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="Random"/> class.
/// </summary>
internal static class RandomExtensions
{
	/// <summary>
	/// Returns a random entry from an <see cref="Enum"/>.
	/// </summary>
	/// <typeparam name="TEnum">The enum to get a value from.</typeparam>
	/// <param name="random">The random class to use.</param>
	/// <returns>A random entry from an <see cref="Enum"/>.</returns>
	internal static TEnum FromEnum<TEnum>( this Random random ) where TEnum : struct, Enum
	{
		return random.FromArray( Enum.GetValues<TEnum>() );
	}

	/// <summary>
	/// Returns a random key and value from a <see cref="IReadOnlyDictionary{TKey, TValue}"/>.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys stored in the <see cref="IReadOnlyDictionary{TKey, TValue}"/>.</typeparam>
	/// <typeparam name="TValue">The type of the values stored in the <see cref="IReadOnlyDictionary{TKey, TValue}"/>.</typeparam>
	/// <param name="random">The random class to use.</param>
	/// <param name="dictionary">The dictionary to get a random entry from.</param>
	/// <returns>A tuple containing the random key with its associated value. Default values are returned if the dictionary is empty.</returns>
	internal static KeyValuePair<TKey, TValue> FromDictionary<TKey, TValue>( this Random random, IReadOnlyDictionary<TKey, TValue> dictionary )
	{
		if ( dictionary.Count == 0 )
			return new KeyValuePair<TKey, TValue>( default, default );

		var index = random.Next( dictionary.Count );
		var curIndex = 0;

		foreach ( var pair in dictionary )
		{
			if ( curIndex == index )
				return pair;

			curIndex++;
		}

		// TODO: This should throw a System.Diagnostics.UnreachableException but S&box whitelist blocks it :)
		throw new Exception( $"{nameof( RandomExtensions )}.{nameof( FromDictionary )}: We should never be here" );
	}
}
