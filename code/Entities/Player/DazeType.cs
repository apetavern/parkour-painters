namespace ParkoutPainters.Entities;

/// <summary>
/// Defines a way that the player becomes dazed.
/// </summary>
public enum DazeType
{
	/// <summary>
	/// No daze.
	/// </summary>
	None,
	/// <summary>
	/// Inhaling something that caused a coughing fit.
	/// </summary>
	Inhalation,
	/// <summary>
	/// A physical interaction that causes disorientation.
	/// </summary>
	PhysicalTrauma
}
