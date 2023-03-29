namespace GangJam.Resources;

/// <summary>
/// Encapsulates all information that relates to a playable group in the game.
/// </summary>
[GameResource( "Group", "group", "Defines a playable group" )]
public sealed class GroupResource : GameResource
{
	public static IReadOnlyDictionary<string, GroupResource> All => _all;
	private static readonly Dictionary<string, GroupResource> _all = new();

	/// <summary>
	/// The name of the group.
	/// </summary>
	public string Name { get; set; }
	/// <summary>
	/// A brief description of the group.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// The color that should be used for effects when spraying.
	/// </summary>
	public Color SprayColor { get; set; }

	/// <summary>
	/// A list of all possible sprays for this group.
	/// </summary>
	[ResourceType( "vmat" )]
	public List<string> AvailableSprays { get; set; }

	/// <summary>
	/// The collection of clothing affiliated with the group.
	/// </summary>
	public ClothingCollectionResource ClothingCollection { get; set; }

	/// <summary>
	/// The prefab to spawn for the players pawn.
	/// </summary>
	public Prefab PlayerPrefab { get; set; }

	/// <inheritdoc/>
	protected sealed override void PostLoad()
	{
		base.PostLoad();

		_all.Add( Name, this );
	}

	/// <inheritdoc/>
	protected sealed override void PostReload()
	{
		base.PostReload();

		_all[Name] = this;
	}
}
