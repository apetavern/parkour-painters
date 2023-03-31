namespace ParkourPainters.Resources;

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
	[Category( "Meta" )]
	public string Name { get; set; } = "Group";

	/// <summary>
	/// A brief description of the group.
	/// </summary>
	[Category( "Meta" )]
	public string Description { get; set; } = "None provided";

	/// <summary>
	/// The color that should be used for effects when spraying.
	/// </summary>
	[Category( "Sprays" )]
	public Color SprayColor { get; set; } = Color.Black;

	/// <summary>
	/// A list of all possible sprays for this group.
	/// </summary>
	[Category( "Sprays" )]
	[ResourceType( "vmat" )]
	public List<string> AvailableSprays { get; set; }

	/// <summary>
	/// Contains all of the custom particles that show on a person when they are dazed.
	/// </summary>
	[Category( "Setup" )]
	[ResourceType( "vpcf" )]
	public Dictionary<DazeType, string> DazeParticles { get; set; } = new Dictionary<DazeType, string>()
	{
		{ DazeType.Inhalation, "particles/stun/stun_base.vpcf" },
		{ DazeType.PhysicalTrauma, "particles/stun/stun_base.vpcf" }
	};

	/// <summary>
	/// The collection of clothing affiliated with the group.
	/// </summary>
	[Category( "Setup" )]
	public ClothingCollectionResource ClothingCollection { get; set; }

	/// <summary>
	/// The prefab to spawn for the players pawn.
	/// </summary>
	[Category( "Setup" )]
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
