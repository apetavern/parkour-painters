namespace GangJam.Resources;

/// <summary>
/// Encapsulates all information that relates to a playable group in the game.
/// </summary>
[GameResource( "Group", "group", "Defines a playable group" )]
public sealed class GroupResource : GameResource
{
	public static IReadOnlyDictionary<string, GroupResource> All => _all;
	public static Dictionary<string, GroupResource> _all { get; set; } = new();

	/// <summary>
	/// The name of the group.
	/// </summary>
	public string Name { get; set; }
	/// <summary>
	/// A brief description of the group.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// The collection of clothing affiliated with the group.
	/// </summary>
	public ClothingCollectionResource ClothingCollection { get; set; }

	/// <inheritdoc/>
	protected override void PostLoad()
	{
		base.PostLoad();

		_all.Add( Name, this );
	}

	/// <inheritdoc/>
	protected override void PostReload()
	{
		base.PostReload();

		_all[Name] = this;
	}
}
