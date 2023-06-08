namespace SpeedPainters.Resources;

/// <summary>
/// Contains all of the data for tinting a clothing item.
/// </summary>
public struct TintableClothingEntry
{
	/// <summary>
	/// The clothing resource to use.
	/// </summary>
	public Clothing Clothing { get; set; }

	/// <summary>
	/// The mode to tint the item.
	/// </summary>
	public TintMode TintMode { get; set; }

	/// <summary>
	/// Contains a list of colors that can be randomly picked to tint the item.
	/// </summary>
	[ShowIf( nameof( TintMode ), TintMode.RandomSelection )]
	public List<Color> RandomSelections { get; set; }

	/// <summary>
	/// A single color that can be used to tint the item.
	/// </summary>
	[ShowIf( nameof( TintMode ), TintMode.Single )]
	public Color SingleSelection { get; set; }
}
