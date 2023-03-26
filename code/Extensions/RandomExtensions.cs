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
}
