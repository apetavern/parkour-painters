namespace ParkoutPainters.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="IEntity"/> interface.
/// </summary>
internal static class IEntityExtensions
{
	/// <summary>
	/// Returns a sequence of <see ref="T"/> that contains any of the tags passed.
	/// </summary>
	/// <typeparam name="T">The type of the entities to check.</typeparam>
	/// <param name="entities">The sequence of entities to check.</param>
	/// <param name="tags">An array of all the tags to search for.</param>
	/// <returns>A sequence of <see ref="T"/> that contains any of the tags passed.</returns>
	internal static IEnumerable<T> WithAnyTags<T>( this IEnumerable<T> entities, params string[] tags ) where T : IEntity
	{
		foreach ( var entity in entities )
		{
			foreach ( var tag in tags )
			{
				if ( !HasTag( entity.TagList, tag ) )
					continue;

				yield return entity;
				break;
			}
		}
	}

	/// <summary>
	/// Returns a sequence of <see ref="T"/> that contains all of the tags passed.
	/// </summary>
	/// <typeparam name="T">The type of the entities to check.</typeparam>
	/// <param name="entities">The sequence of entities to check.</param>
	/// <param name="tags">An array of all the tags to search for.</param>
	/// <returns>A sequence of <see ref="T"/> that contains all of the tags passed.</returns>
	internal static IEnumerable<T> WithTags<T>( this IEnumerable<T> entities, params string[] tags ) where T : IEntity
	{
		foreach ( var entity in entities )
		{
			for ( var i = 0; i < tags.Length; i++ )
			{
				if ( !HasTag( entity.TagList, tags[i] ) )
					break;

				if ( i == tags.Length - 1 )
					yield return entity;
			}
		}
	}

	/// <summary>
	/// Returns a sequence of <see ref="T"/> that does not contain any of the tags passed.
	/// </summary>
	/// <typeparam name="T">The type of the entities to check.</typeparam>
	/// <param name="entities">The sequence of entities to check.</param>
	/// <param name="tags">An array of all the tags to search for.</param>
	/// <returns>A sequence of <see ref="T"/> that does not contain any of the tags passed.</returns>
	internal static IEnumerable<T> WithoutTags<T>( this IEnumerable<T> entities, params string[] tags ) where T : IEntity
	{
		foreach ( var entity in entities )
		{
			for ( var i = 0; i < tags.Length; i++ )
			{
				if ( HasTag( entity.TagList, tags[i] ) )
					break;

				if ( i == tags.Length - 1 )
					yield return entity;
			}
		}
	}

	/// <summary>
	/// Returns whether or not a tag is inside a list of tags.
	/// </summary>
	/// <param name="tagList">The list of tags to search.</param>
	/// <param name="tagSpan">The tag to search for.</param>
	/// <returns></returns>
	private static bool HasTag( ReadOnlySpan<char> tagList, ReadOnlySpan<char> tagSpan )
	{
		// Quick check to see if the tag list only contains the tag.
		if ( tagList == tagSpan )
			return true;

		var tagIndex = tagList.IndexOf( tagSpan );
		// Sequence not found.
		if ( tagIndex == -1 )
			return false;

		// Sequence is at the start of the list.
		if ( tagIndex == 0 )
			return tagList[tagIndex + tagSpan.Length + 1] == ' ' ||
				HasTag( tagList[(tagIndex + tagSpan.Length + 2)..], tagSpan );
		// Sequence is at the end of the list.
		else if ( tagIndex == tagList.Length - tagSpan.Length )
			return tagList[tagIndex - 1] == ' ' ||
				HasTag( tagList[..(tagList.Length - tagSpan.Length - 2)], tagSpan );
		// Sequence is somewhere in the middle of the list.
		else
			return (tagList[tagIndex - 1] == ' ' && tagList[tagIndex + tagSpan.Length + 1] == ' ') ||
				HasTag( tagList[(tagIndex + tagSpan.Length + 2)..], tagSpan );
	}
}
