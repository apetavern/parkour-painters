namespace ParkourPainters.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="IEnumerable{T}"/> interface.
/// </summary>
internal static class IEnumerableExtensions
{
	/// <summary>
	/// Combines a hash with each of the elements by the selector.
	/// </summary>
	/// <typeparam name="T">The type of the items inside the sequence.</typeparam>
	/// <param name="e">The sequence to combine.</param>
	/// <param name="selector">The member selector.</param>
	/// <returns>The combined hash of each element.</returns>
	internal static int HashCombine<T>( this IEnumerable<T> e, Func<T, decimal> selector )
	{
		var result = 0;

		foreach ( var el in e )
			result = HashCode.Combine( result, selector.Invoke( el ) );

		return result;
	}
}
