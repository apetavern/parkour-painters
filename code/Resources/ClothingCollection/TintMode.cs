namespace ParkoutPainters.Resources;

/// <summary>
/// Represents a way to tint a clothing item.
/// </summary>
public enum TintMode
{
	/// <summary>
	/// No tint should be applied.
	/// </summary>
	None,
	/// <summary>
	/// A completely random color will be applied.
	/// </summary>
	Random,
	/// <summary>
	/// A random color from a selection of preset colors will be applied.
	/// </summary>
	RandomSelection,
	/// <summary>
	/// A single color will be applied.
	/// </summary>
	Single
}
