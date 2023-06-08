namespace SpeedPainters.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="IReadOnlyList{T}"/> interface.
/// </summary>
internal static class IReadOnlyListExtensions
{
	/// <summary>
	/// Returns the index of the first occurrence of a given value in a range of
	/// this list. The list is searched forwards from beginning to end.
	/// The elements of the list are compared to the given value using the
	/// Object.Equals method.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the list.</typeparam>
	/// <param name="list">The list to search.</param>
	/// <param name="itemToLookFor">The item to look for.</param>
	/// <returns>The index of the found item. -1 if not found.</returns>
	internal static int IndexOf<T>( this IReadOnlyList<T> list, T itemToLookFor )
	{
		var index = 0;

		foreach ( var item in list )
		{
			if ( item.Equals( itemToLookFor ) )
				return index;

			index++;
		}

		return -1;
	}
}
